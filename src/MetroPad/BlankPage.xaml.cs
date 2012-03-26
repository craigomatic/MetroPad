using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using MetroPad.Input;
using MetroPad.ViewModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MetroPad
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankPage : Page
    {
        private string _BaseText;

        public BlankPage()
        {
            this.InitializeComponent();

            (App.ViewModel.NewCommand as CommandBase).ExecuteDelegate = e => _NewFile();
            (App.ViewModel.OpenCommand as CommandBase).ExecuteDelegate = e => _OpenFile();
            (App.ViewModel.SaveCommand as CommandBase).ExecuteDelegate = e => _SaveFile();
            (App.ViewModel.EditCommand as CommandBase).ExecuteDelegate = e => _EditFile();

            this.DataContext = App.ViewModel;
        }

        private void _EditFile()
        {
            App.ViewModel.SelectedDocument.IsEditing = !App.ViewModel.SelectedDocument.IsEditing;

            _UpdateStatusTextForFile(App.ViewModel.SelectedDocument.StorageFile);

            TextEditor.IsReadOnly = !App.ViewModel.SelectedDocument.IsEditing;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void _NewFile()
        {
            App.ViewModel.StatusText = "Editing an unsaved document";
            App.ViewModel.SelectedDocument = new ViewModel.DocumentViewModel
            {
                IsEditing = true
            };
            
            TextEditor.Document.SetText(Windows.UI.Text.TextSetOptions.ApplyRtfDocumentDefaults, string.Empty);
        }

        private async void _SaveFile()
        {
            //save as when there isn't an existing storage file
            if (App.ViewModel.SelectedDocument.StorageFile == null)
            {
                _SaveFileAs();
                return;
            }

            try
            {
                var currentText = string.Empty;
                TextEditor.Document.GetText(Windows.UI.Text.TextGetOptions.None, out currentText);

                await Windows.Storage.FileIO.WriteTextAsync(App.ViewModel.SelectedDocument.StorageFile, currentText);

                //using (var stream = await App.ViewModel.SelectedDocument.StorageFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                //{
                //    Note: Seems like something is a little buggy with the SaveToStream method, it's saving the original state of the document
                //    as opposed to the current state of the document which the GetText method above correctly retrieves
                
                //    TextEditor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.None, stream);
                
                //    await stream.FlushAsync();
                //}
            }
            catch 
            {
                var dialog = new Windows.UI.Popups.MessageDialog("Unable to save the file", "Access Denied");
                dialog.ShowAsync();
            }
        }

        private async void _SaveFileAs()
        {
            var savePicker = new FileSavePicker();
            savePicker.DefaultFileExtension = ".txt";
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>{".txt"});
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            var file = await savePicker.PickSaveFileAsync();
            
            try
            {
                using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                {
                    TextEditor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.None, stream);
                }

                App.ViewModel.SelectedDocument = new DocumentViewModel
                {
                    StorageFile = file,
                    IsEditing = true
                };

                _UpdateStatusTextForFile(file);
            }
            catch
            {
                var dialog = new Windows.UI.Popups.MessageDialog("Unable to save the file", "Access Denied");
                dialog.ShowAsync();
            }
        }

        private static void _UpdateStatusTextForFile(Windows.Storage.IStorageFile file)
        {
            var displayName = (file as Windows.Storage.StorageFile).DisplayName;
            App.ViewModel.StatusText = App.ViewModel.SelectedDocument.IsEditing ? string.Format("Editing {0}", displayName) : string.Format("Viewing {0}", displayName);
        }

        private async void _OpenFile()
        {
            var openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".txt");
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            
            var file = await openPicker.PickSingleFileAsync();
           
            if (file != null)
            {                
                App.ViewModel.SelectedDocument = new ViewModel.DocumentViewModel
                {
                    IsEditing = false,
                    StorageFile = file,
                    IsReadOnly = file.Attributes == Windows.Storage.FileAttributes.ReadOnly
                };

                using (var fileStream = await file.OpenReadAsync())
                {
                    using (var streamReader = new StreamReader(fileStream.AsStreamForRead()))
                    {
                        var text = await streamReader.ReadToEndAsync();

                        App.ViewModel.SelectedDocument.IsEditing = true;
                        TextEditor.Document.SetText(Windows.UI.Text.TextSetOptions.ApplyRtfDocumentDefaults, text);
                        
                        _BaseText = _GetCurrentDocumentText();
                        
                        App.ViewModel.SelectedDocument.IsEditing = false;
                        App.ViewModel.StatusText = string.Format("Viewing {0}", file.DisplayName);
                    }
                }                
            }
            else
            {
                
            }
        }

        private void TextEditor_TextChanged(object sender, RoutedEventArgs e)
        {
            var currentText = _GetCurrentDocumentText();
            App.ViewModel.SelectedDocument.HasUnsavedChanges = _BaseText != currentText;
        }

        private string _GetCurrentDocumentText()
        {
            var currentText = string.Empty;

            TextEditor.Document.GetText(Windows.UI.Text.TextGetOptions.None, out currentText);
            return currentText;
        }
    }
}
