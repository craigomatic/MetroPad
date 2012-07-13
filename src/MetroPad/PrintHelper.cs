using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Printing;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Printing;

namespace MetroPad
{
    public class PrintHelper : IDisposable
    {
        private PrintDocument _PrintDocument;
        private IPrintDocumentSource _PrintDocumentSource;
        private IList<UIElement> _Pages;
        private PrintManager _PrintManager;
        private string _Title;
        private ManualResetEventSlim _ResetEvent;
        private PrintTaskCompletion _PrintTaskCompletionValue;

        public PrintHelper()
        {
            _PrintDocument = new PrintDocument();
            _PrintDocumentSource = _PrintDocument.DocumentSource;
            _PrintDocument.AddPages += _PrintDocument_AddPages;
            _PrintDocument.Paginate += _PrintDocument_Paginate;
            _PrintDocument.GetPreviewPage += _PrintDocument_GetPreviewPage;

            _Pages = new List<UIElement>();
        }

        public async Task<PrintTaskCompletion> PrintAsync(string title, IList<UIElement> pages)
        {
            _ResetEvent = new ManualResetEventSlim();

            var completionTask = Task<PrintTaskCompletion>.Run(() =>
            {
                _ResetEvent.Wait();

                return _PrintTaskCompletionValue;
            });

            _Title = title;
            _Pages = pages;

            _PrintManager = PrintManager.GetForCurrentView();
            _PrintManager.PrintTaskRequested += _PrintManager_PrintTaskRequested;
            
            await PrintManager.ShowPrintUIAsync();
            
            return await completionTask;
        }

        void _PrintDocument_GetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            _PrintDocument.SetPreviewPage(e.PageNumber, _Pages.FirstOrDefault());
        }

        void _PrintDocument_Paginate(object sender, PaginateEventArgs e)
        {
            _PrintDocument.SetPreviewPageCount(_Pages.Count, PreviewPageCountType.Intermediate);
        }

        void _PrintDocument_AddPages(object sender, AddPagesEventArgs e)
        {
            foreach (var page in _Pages)
            {
                _PrintDocument.AddPage(page);
            }

            _PrintDocument.AddPagesComplete();
        }

        void _PrintManager_PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            var printTask = args.Request.CreatePrintTask(_Title, e => e.SetSource(_PrintDocumentSource));
            printTask.Completed += printTask_Completed;
        }

        void printTask_Completed(PrintTask sender, PrintTaskCompletedEventArgs args)
        {
            _PrintTaskCompletionValue = args.Completion;

            _ResetEvent.Set();
        }

        public void Dispose()
        {
            _PrintDocument.AddPages -= _PrintDocument_AddPages;
            _PrintDocument.Paginate -= _PrintDocument_Paginate;
            _PrintDocument.GetPreviewPage -= _PrintDocument_GetPreviewPage;
            _PrintManager.PrintTaskRequested -= _PrintManager_PrintTaskRequested;

            if (_ResetEvent.IsSet)
            {
                _ResetEvent.Set();
            }
        }
    }
}
