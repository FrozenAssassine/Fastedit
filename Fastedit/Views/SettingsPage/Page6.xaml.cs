using Fastedit.Core;
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
        private muxc.TabView TextTabControl = null;
        private ExportImportSettings exportimportsettings = new ExportImportSettings();

        public Page6()
        {
            this.InitializeComponent();
            RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsString("ThemeIndex", "0"));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is SettingsNavigationParameter snp)
            {
                TextTabControl = snp.Tabcontrol;
            }
            //AutoBackupDatabaseNumberBox.Value = appsettings.GetSettingsAsInt("AutoBackupDatabaseTime", DefaultValues.AutoBackupDataBaseMinutes);
            //AutoSaveFilesNumberBox.Value = appsettings.GetSettingsAsInt("AutoSaveFileTime", DefaultValues.AutoSaveTempFileMinutes);
        }

        //Show infobar with content
        private void ShowInfobar(string Content, muxc.InfoBarSeverity Severity)
        {
            SettingsInfoBar.Severity = Severity;
            SettingsInfoBar.Title = Content;
            SettingsInfoBar.IsOpen = true;
        }

        /*
//Function to ask for backup and backup
private async Task<bool> ImportBackup(StorageFolder folder = null)
{
    var SaveDialog = new ContentDialog
    {
        Background = DefaultValues.ContentDialogBackgroundColor(),
        CornerRadius = DefaultValues.DefaultDialogCornerRadius,
        Title = "Load backup",
        Content = "Do your really want to load the backup, you will lose all currently opened tabs?",
        PrimaryButtonText = "Load",
        CloseButtonText = "Cancel"
    };
    var dialogres = await SaveDialog.ShowAsync();

    if (dialogres == ContentDialogResult.Primary)
    {
        if (TextTabControl == null)
            return false;

        TabActions tabActions = new TabActions(TextTabControl);
        var res = await tabActions.LoadTabs(true, folder);
        await tabActions.SaveAllTabChanges();
        return res;
    }
    return false;
}


//Load Backup from default appfolder
private async void LoadLastBackupButton_Click(object sender, RoutedEventArgs e)
{
    await ImportBackup();
}
//Backup to default appfolder
private async void BackupNowButton_Click(object sender, RoutedEventArgs e)
{
    TabActions tabActions = new TabActions(TextTabControl);
    if (await tabActions.SaveAllTabChangesToBackup())
    {
        ShowInfobar("Backup succeed", muxc.InfoBarSeverity.Success);
    }
    else
    {
        ShowInfobar("Backup failed", muxc.InfoBarSeverity.Error);
    }
}
//Export current database to a picked folder
private async void ExportBackup_Click(object sender, RoutedEventArgs e)
{
    try
    {
        FolderPicker picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

        StorageFolder folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            TabActions tabActions = new TabActions(TextTabControl);
            if (await tabActions.SaveAllTabChanges(folder))
            {
                ShowInfobar("Backup successfully created in " + folder.Path, muxc.InfoBarSeverity.Success);
            }
            else
            {
                ShowInfobar("Backup creation failed", muxc.InfoBarSeverity.Error);
            }
        }
    }
    catch(Exception ex)
    {
        ShowInfobar("Backup creation failed\n" + ex.Message, muxc.InfoBarSeverity.Error);
    }

}      
//Load Backup from custom folder
private async void ImportBackup_Click(object sender, RoutedEventArgs e)
{
    try
    {
        FolderPicker picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
        StorageFolder folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            if (await ImportBackup(folder))
            {
                ShowInfobar("Backup successfully loaded from" + folder.Path, muxc.InfoBarSeverity.Success);
            }
            else
            {
                ShowInfobar("Backup loading failed", muxc.InfoBarSeverity.Error);
            }
        }
    }
    catch(Exception ex)
    {
        ShowInfobar("Backup loading failed\n" + ex.Message, muxc.InfoBarSeverity.Error);
    }
}
*/
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

        //Clear the recylcebin
        private async void ClearRecyclebin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(DefaultValues.RecycleBin_FolderName, CreationCollisionOption.OpenIfExists);
                if (folder == null) return;

                var files = await folder.GetFilesAsync();
                for (int i = 0; i < files.Count; i++)
                {
                    if (files[i] == null) return;
                    await files[i].DeleteAsync();
                }

                var files_2 = await folder.GetFilesAsync();
                if (files_2.Count == 0)
                {
                    ShowInfobar("Successfully cleared recyclebin", muxc.InfoBarSeverity.Success);
                }
                else
                {
                    throw new Exception("Not all files could be deleted!");
                }
            }
            catch (Exception ex)
            {
                ShowInfobar("Error occured while clearing recyclebin\n" + ex.Message, muxc.InfoBarSeverity.Error);
            }
        }

        /*
        private void AutoBackupDatabaseNumberBox_ValueChanged(muxc.NumberBox sender, muxc.NumberBoxValueChangedEventArgs args)
        {
            appsettings.SaveSettings("AutoBackupDatabaseTime", sender.Value.ToString());
        }
        private void AutoSaveFilesNumberBox_ValueChanged(muxc.NumberBox sender, muxc.NumberBoxValueChangedEventArgs args)
        {
            appsettings.SaveSettings("AutoSaveFileTime", sender.Value.ToString());
        }*/
    }
}
