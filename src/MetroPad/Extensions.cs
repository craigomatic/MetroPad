using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroPad.ViewModel;
using Windows.Storage;

namespace MetroPad
{
    public static class Extensions
    {
        public static string GetDisplayName(this DocumentViewModel viewModel)
        {
            return viewModel.StorageFile == null ? "Untitled" : (viewModel.StorageFile as StorageFile).DisplayName;
        }

        public static DateTimeOffset GetCreatedDate(this DocumentViewModel viewModel)
        {
            return viewModel.StorageFile == null ? DateTimeOffset.Now : viewModel.StorageFile.DateCreated.ToLocalTime();
        }
    }
}
