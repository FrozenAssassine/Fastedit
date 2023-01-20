using Fastedit.Tab;
using Windows.ApplicationModel.DataTransfer;

namespace Fastedit.Dialogs
{
    public class ShareDialog
    {
        private static TabPageItem CurrentTab = null;

        public static void Share(TabPageItem tab)
        {
            if (tab == null)
                return;

            CurrentTab = tab;
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private static void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.SetText(CurrentTab.textbox.GetText());
            request.Data.Properties.Title = CurrentTab.DatabaseItem.FileName;
        }
    }
}
