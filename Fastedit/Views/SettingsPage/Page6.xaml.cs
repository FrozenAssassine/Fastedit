using Fastedit.Core;
using Fastedit.Dialogs;
using Fastedit.ExternalData;
using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit.Views.SettingsPage
{
    public sealed partial class Page6 : Page
    {
        private AppSettings appsettings = new AppSettings();
        private ExportImportSettings exportimportsettings = new ExportImportSettings();

        public Page6()
        {
            this.InitializeComponent();
            RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsString("ThemeIndex", "0"));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        //Show infobar with content
        private void ShowInfobar(string Content, muxc.InfoBarSeverity Severity)
        {
            SettingsInfoBar.Severity = Severity;
            SettingsInfoBar.Title = Content;
            SettingsInfoBar.IsOpen = true;
        }

        //Export appsettings
        private async void ExportSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (await exportimportsettings.ExportSettings() == true)
            {
                ShowInfobar("Settings saved successfully", muxc.InfoBarSeverity.Success);
            }
            else
            {
                ShowInfobar("Couldn't save settings", muxc.InfoBarSeverity.Error);
            }
        }
        //Import appsettings
        private async void ImportSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (await exportimportsettings.ImportSettings() == true)
            {
                ShowInfobar("Settings loaded successfully", muxc.InfoBarSeverity.Success);
            }
            else
            {
                ShowInfobar("Couldn't load settings successfully", muxc.InfoBarSeverity.Error);
            }
        }

        //Clear the recyclebin
        private async void ClearRecyclebin_Click(object sender, RoutedEventArgs e)
        {
            var res =await RecyclebinWindow.ClearRecycleBin();
            if (res == ClearRecycleBinResult.Success)
                ShowInfobar("Successfully cleared the Recyclebin", muxc.InfoBarSeverity.Success);
            else if (res == ClearRecycleBinResult.NotAllFilesDeleted)
                ShowInfobar("Not all files could be deleted, please try again", muxc.InfoBarSeverity.Warning);
            else if (res == ClearRecycleBinResult.NullError)
                ShowInfobar("Could not clear the Recyclebin, something returned null", muxc.InfoBarSeverity.Error);
            else if (res == ClearRecycleBinResult.Exception)
                ShowInfobar("Could not clear the Recylcebin, an exception occured", muxc.InfoBarSeverity.Error);
        }
    }
}
