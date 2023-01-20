using Fastedit.Tab;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace Fastedit.Helper
{
    public class ShareFileHelper
    {
        private static TabPageItem TabPage = null;

        public static void ShowShareUI(TabPageItem tab)
        {
            TabPage = tab;
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private static async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("ShareTemp.txt", CreationCollisionOption.OpenIfExists);
            if (file != null)
            {
                await FileIO.WriteTextAsync(file, TabPage.textbox.GetText());
                DataRequest request = args.Request;
                request.Data.SetStorageItems(new List<IStorageFile> { file });
                request.Data.Properties.Title = TabPage.DatabaseItem.FileName;
            }
        }
    }
}
