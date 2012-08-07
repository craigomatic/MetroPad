using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MetroPad.Common;
using MetroPad.Input;
using Windows.UI;
using MetroPad.Model;

namespace MetroPad.ViewModel
{
    public class ApplicationViewModel : BindableBase
    {
        #region Properties

        private DocumentViewModel _SelectedDocument;

        public DocumentViewModel SelectedDocument
        {
            get { return _SelectedDocument; }
            set 
            {
                var oldDoc = _SelectedDocument;

                if (base.SetProperty(ref _SelectedDocument, value))
                {
                    if(oldDoc != null)
                    {
                        oldDoc.PropertyChanged-= SelectedDocument_PropertyChanged;
                    }

                    //hook the listener to the new document
                    _SelectedDocument.PropertyChanged += SelectedDocument_PropertyChanged;

                    _UpdateCanExecuteStates();
                }
            }
        }

        private string _StatusText;

        public string StatusText
        {
            get { return _StatusText; }
            set { base.SetProperty(ref _StatusText, value); }
        }

        public ObservableCollection<DocumentViewModel> Documents { get; private set; }

        public IList<FontSelection> SystemFonts { get; private set; }

        public IList<FontColour> SystemColours { get; private set; }

        #endregion

        #region Commands

        private readonly ICommand _BoldCommand;

        public ICommand BoldCommand
        {
            get { return _BoldCommand; }
        }

        private readonly ICommand _ItalicsCommand;

        public ICommand ItalicsCommand
        {
            get { return _ItalicsCommand; }
        }

        private readonly ICommand _UnderlineCommand;

        public ICommand UnderlineCommand
        {
            get { return _UnderlineCommand; }
        }

        private readonly ICommand _FontColourCommand;

        public ICommand FontColourCommand
        {
            get { return _FontColourCommand; }
        }

        private readonly ICommand _FontSelectionCommand;

        public ICommand FontSelectionCommand
        {
            get { return _FontSelectionCommand; }
        }

        private readonly ICommand _NewCommand;

        public ICommand NewCommand
        {
            get { return _NewCommand; }
        }

        private readonly ICommand _SaveCommand;

        public ICommand SaveCommand
        {
            get { return _SaveCommand; }
        }

        private readonly ICommand _OpenCommand;

        public ICommand OpenCommand
        {
            get { return _OpenCommand; }
        }

        private readonly ICommand _EditCommand;
       
        public ICommand EditCommand
        {
            get { return _EditCommand; }
        }

        private readonly ICommand _PrintCommand;

        public ICommand PrintCommand
        {
            get { return _PrintCommand; }
        }
        
        #endregion

        public ApplicationViewModel()
        {
            //TODO: remove the hardcode, replace with something that gets the system fonts
            // this.SystemFonts = new List<string>() { "Segoe UI", "Times New Roman", "Arial", "Verdana", "Helvetica", "Courier New" };
            var list = new List<string>() { "Segoe UI", "Times New Roman", "Arial", "Verdana", "Helvetica", "Courier New" };
            this.SystemFonts = new List<FontSelection>();

            for(int i=0;i<6;i++)
            {
                this.SystemFonts.Add(new FontSelection
                {
                    Name = list[i]
            });
        }
            this.SystemColours = new List<FontColour>();

            var ti = typeof(Colors).GetTypeInfo();
            var dp = ti.DeclaredProperties;
            
            foreach (var item in dp)
            {
                this.SystemColours.Add(new FontColour 
                { 
                    Name = item.Name,
                    Value = (Color)item.GetValue(null) 
                });
            }

            this.Documents = new ObservableCollection<DocumentViewModel>();
            this.SelectedDocument = new DocumentViewModel();

            _BoldCommand = new CommandBase
            {
                CanExecuteDelegate = c => true
            };

            _ItalicsCommand = new CommandBase
            {
                CanExecuteDelegate = c => true
            };

            _UnderlineCommand = new CommandBase
            {
                CanExecuteDelegate = c => true
            };

            _FontColourCommand = new CommandBase
            {
                CanExecuteDelegate = c => true
            };

            _FontSelectionCommand = new CommandBase
            {
                CanExecuteDelegate = c => true
            };

            _NewCommand = new CommandBase
            {
                CanExecuteDelegate = c => true
            };

            _OpenCommand = new CommandBase
            {
                CanExecuteDelegate = c => true
            };

            _SaveCommand = new CommandBase
            {
                CanExecuteDelegate = c => !this.SelectedDocument.IsReadOnly && this.SelectedDocument.HasUnsavedChanges
            };

            _EditCommand = new CommandBase
            {
                CanExecuteDelegate = c => !this.SelectedDocument.IsReadOnly && this.SelectedDocument.StorageFile != null      
            };

            _PrintCommand = new CommandBase
            {
                CanExecuteDelegate = c => this.SelectedDocument.Text.Length > 0
            };

            this.StatusText = "Editing an unsaved document";
            this.SelectedDocument.IsEditing = true;
        }

        void SelectedDocument_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _UpdateCanExecuteStates();
        }

        private void _UpdateCanExecuteStates()
        {
            if (_NewCommand == null ||
                _EditCommand == null ||
                _OpenCommand == null ||
                _SaveCommand == null ||
                _PrintCommand == null)
            {
                return;
            }

            (_NewCommand as CommandBase).OnCanExecuteChanged();
            (_EditCommand as CommandBase).OnCanExecuteChanged();
            (_OpenCommand as CommandBase).OnCanExecuteChanged();
            (_SaveCommand as CommandBase).OnCanExecuteChanged();
            (_PrintCommand as CommandBase).OnCanExecuteChanged();
        }
    }
}