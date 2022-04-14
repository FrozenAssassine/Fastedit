using Fastedit.Controls.Textbox;
using Windows.ApplicationModel.DataTransfer;

namespace Fastedit.Dialogs
{
    public class ShareFile
    {
        private TextControlBox textBox = null;

        public ShareFile(TextControlBox tb)
        {
            textBox = tb;
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.SetText(textBox.GetText());
            request.Data.Properties.Title = textBox.Header;
        }

        public static void ShareText(string text)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += delegate (DataTransferManager sender, DataRequestedEventArgs args)
            {
                DataRequest request = args.Request;
                request.Data.SetText(text);
                request.Data.Properties.Title = "Share text";
            };
            DataTransferManager.ShowShareUI();

        }
    }
}
