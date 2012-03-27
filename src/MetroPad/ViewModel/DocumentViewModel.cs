using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroPad.Common;
using Windows.Storage;

namespace MetroPad.ViewModel
{
    public class DocumentViewModel: BindableBase
    {
        private bool _HasUnsavedChanges;

        public bool HasUnsavedChanges
        {
            get { return _HasUnsavedChanges; }
            set { base.SetProperty(ref _HasUnsavedChanges, value); }
        }

        private bool _IsReadOnly;

        public bool IsReadOnly
        {
            get { return _IsReadOnly; }
            set { base.SetProperty(ref _IsReadOnly, value); }
        }

        private bool _IsEditing;

        public bool IsEditing
        {
            get { return _IsEditing; }
            set { base.SetProperty(ref _IsEditing, value); }
        }

        private IStorageFile _StorageFile;

        public IStorageFile StorageFile
        {
            get { return _StorageFile; }
            set { base.SetProperty(ref _StorageFile, value); }
        }

        private string _Text;

        public string Text
        {
            get { return _Text; }
            set { base.SetProperty(ref _Text, value); }
        }

        public DocumentViewModel()
        {
            this.Text = string.Empty;
        }
    }
}
