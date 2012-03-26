using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MetroPad.Common;
using MetroPad.Input;
using Windows.Storage;

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

        #endregion

        #region Commands

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
        
        #endregion

        public ApplicationViewModel()
        {
            this.Documents = new ObservableCollection<DocumentViewModel>();
            this.SelectedDocument = new DocumentViewModel();
            
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
                _SaveCommand == null)
            {
                return;
            }

            (_NewCommand as CommandBase).OnCanExecuteChanged();
            (_EditCommand as CommandBase).OnCanExecuteChanged();
            (_OpenCommand as CommandBase).OnCanExecuteChanged();
            (_SaveCommand as CommandBase).OnCanExecuteChanged();
        }
    }
}