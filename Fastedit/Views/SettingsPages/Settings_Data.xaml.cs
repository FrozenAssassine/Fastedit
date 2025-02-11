using Fastedit.Core;
using Fastedit.Core.Settings;
using Fastedit.Dialogs;
using Fastedit.Models;
using Fastedit.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Fastedit.Views.SettingsPages
{
    public sealed partial class Settings_Data : Page
    {
        public Settings_Data()
        {
            this.InitializeComponent();
        }

        private void InitSize()
        {
            temporaryFileSizeDisplay.Text = TemporaryFilesHandler.GetSize();
            recyclebinFilesizeDisplay.Text = RecycleBinManager.GetSize();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            InitSize();
            base.OnNavigatedTo(e);
        }

        //Import/Export settings
        private async void ExportSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var res = await SettingsImportExport.Export();
            if (res == SettingsImportExportResult.Success)
                InfoMessages.SettingsExportSucceed();
            else if (res == SettingsImportExportResult.Failed)
                InfoMessages.SettingsExportFailed();
        }
        private async void ImportSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var res = await SettingsImportExport.Import();
            if (res == SettingsImportExportResult.Success)
                InfoMessages.SettingsImportSucceed();
            else if(res == SettingsImportExportResult.Failed)
                InfoMessages.SettingsImportFailed();
        }

        //Clear recylcebin
        private void ClearRecyclebin_Click(object sender, RoutedEventArgs e)
        {
            if (RecycleBinManager.ClearRecycleBin() == ClearRecycleBinResult.Success)
                InfoMessages.RecyclebinClearSucceed();

            InitSize();
        }
        private async void ClearTemporaryFiles_Click(object sender, RoutedEventArgs e)
        {
            if (await TemporaryFilesHandler.Clear())
                InfoMessages.ClearTemporaryFilesSucceed();
            else
                InfoMessages.ClearTemporaryFilesFailed();

            InitSize();
        }
    }
}
