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
                InfoMessages.SettingsExportSucceeded();
            else if (res == SettingsImportExportResult.Failed)
                InfoMessages.SettingsExportFailed();
        }
        private async void ImportSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var res = await SettingsImportExport.Import();
            if (res == SettingsImportExportResult.Success)
                InfoMessages.SettingsImportSucceeded();
            else if(res == SettingsImportExportResult.Failed)
                InfoMessages.SettingsImportFailed();
        }

        private void ClearRecyclebin_Click(object sender, RoutedEventArgs e)
        {
            if (RecycleBinManager.ClearRecycleBin() == ClearRecycleBinResult.Success)
                InfoMessages.RecycleBinClearSucceeded();

            InitSize();
        }
        private async void ClearTemporaryFiles_Click(object sender, RoutedEventArgs e)
        {
            if (await TemporaryFilesHandler.Clear())
                InfoMessages.ClearTemporaryFilesSucceeded();
            else
                InfoMessages.ClearTemporaryFilesFailed();

            InitSize();
        }
    }
}
