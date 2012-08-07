using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel;
using MetroPad.Input;
using MetroPad.ViewModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Text;
using Windows.UI;
using System.Reflection;
using MetroPad.Model;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace MetroPad
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainView : MetroPad.Common.LayoutAwarePage
    {
        private string _BaseText;

        public List<int> Integers = new List<int>();


        public MainView()
        {
            this.InitializeComponent();

            (App.ViewModel.BoldCommand as CommandBase).ExecuteDelegate = e => _BoldText();
            (App.ViewModel.ItalicsCommand as CommandBase).ExecuteDelegate = e => _ItalicizeText();
            (App.ViewModel.UnderlineCommand as CommandBase).ExecuteDelegate = e => _UnderlineText();
            (App.ViewModel.FontColourCommand as CommandBase).ExecuteDelegate = e => _FontColour();
            (App.ViewModel.FontSelectionCommand as CommandBase).ExecuteDelegate = e => _FontSelection();
            (App.ViewModel.NewCommand as CommandBase).ExecuteDelegate = e => _NewFile();
            (App.ViewModel.OpenCommand as CommandBase).ExecuteDelegate = e => _OpenFile();
            (App.ViewModel.SaveCommand as CommandBase).ExecuteDelegate = e => _SaveFile();
            (App.ViewModel.EditCommand as CommandBase).ExecuteDelegate = e => _EditFile();
            (App.ViewModel.PrintCommand as CommandBase).ExecuteDelegate = e => _PrintFile();

            this.DataContext = App.ViewModel;

            DataTransferManager.GetForCurrentView().DataRequested += BlankPage_DataRequested;
        }

        void BlankPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var request = args.Request;
            request.Data.Properties.Title = App.ViewModel.SelectedDocument.GetDisplayName();
            request.Data.Properties.Description = string.Format("Created {0}", App.ViewModel.SelectedDocument.GetCreatedDate().ToString("g"));
            request.Data.SetText(_GetCurrentDocumentText());
        }

        private async void _PrintFile()
        {
            var title = App.ViewModel.SelectedDocument.StorageFile != null ? (App.ViewModel.SelectedDocument.StorageFile as StorageFile).DisplayName : "Untitled";

            using (var printHelper = new PrintHelper())
            {
                await printHelper.PrintAsync(title, new List<UIElement> { TextEditor });
            }
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

                var lines = currentText.Split('\r');

                if (lines.Length > 0)
                {
                    using (var sw = new StreamWriter(await App.ViewModel.SelectedDocument.StorageFile.OpenStreamForWriteAsync()))
                    {
                        foreach (var line in lines)
                        {
                            await sw.WriteLineAsync(line);
                        }
                    }
                }
                else
                {
                    await Windows.Storage.FileIO.WriteTextAsync(App.ViewModel.SelectedDocument.StorageFile, currentText);
                }

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
            savePicker.FileTypeChoices.Add("Plain Text", new List<string> { ".txt" });
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

        public async void OpenFile(StorageFile storageFile)
        {
            await _ReadTextFromFile(storageFile);
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
                await _ReadTextFromFile(file);
            }
            else
            {

            }
        }

        private async System.Threading.Tasks.Task _ReadTextFromFile(StorageFile file)
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

        private void TextEditor_TextChanged(object sender, RoutedEventArgs e)
        {
            App.ViewModel.SelectedDocument.Text = _GetCurrentDocumentText();
            App.ViewModel.SelectedDocument.HasUnsavedChanges = _BaseText != App.ViewModel.SelectedDocument.Text;
        }

        private string _GetCurrentDocumentText()
        {
            var currentText = string.Empty;

            TextEditor.Document.GetText(Windows.UI.Text.TextGetOptions.None, out currentText);
            return currentText;
        }

        private void _BoldText()
        {
            ITextCharacterFormat format = TextEditor.Document.Selection.CharacterFormat;
            format.Bold = FormatEffect.Toggle;
        }

        private void _ItalicizeText()
        {
            ITextCharacterFormat format = TextEditor.Document.Selection.CharacterFormat;
            format.Italic = FormatEffect.Toggle;
        }

        private void _UnderlineText()
        {
            ITextCharacterFormat format = TextEditor.Document.Selection.CharacterFormat;
            if (format.Underline == UnderlineType.None)
            {
                format.Underline = UnderlineType.Single;
            }
            else
            {
                format.Underline = UnderlineType.None;
            }
        }

        private void _FontColour()
        {
            TextEditor.Document.Selection.CharacterFormat.ForegroundColor = Colors.Red;
        }

        private void _FontSelection()
        {
            TextEditor.FontFamily = new FontFamily("Times New Roman");
        }


        private static void _LoadColors()
        {
            

            var t = typeof(Colors);
            var ti = t.GetTypeInfo();
            var dp = ti.DeclaredProperties;
            _Colors = new List<PropertyInfo>();

            foreach (var item in dp)
            {
                _Colors.Add(item);
            }
        }

        private static List<PropertyInfo> _Colors;

        public List<PropertyInfo> Colors1
        {
            get
            {
                if (_Colors == null)
                    _LoadColors();
                return _Colors;
            }
        }

        private void FontSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            try
            {
                TextEditor.FontFamily = new FontFamily(e.AddedItems[0] as string);
            }
            catch { }
        }


        private void FontColorChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            TextEditor.Document.Selection.CharacterFormat.ForegroundColor = (e.AddedItems[0] as FontColour).Value;
        }
    }
}
