using Fastedit.Controls;
using Fastedit.Tab;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
    public class InfoMessages
    {
        public static StackPanel InfoMessagePanel = null;

        public static void NoAccesToSaveFile() => new InfoBar().Show("No access", "No access to write to file", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        public static void NoAccesToReadFile() => new InfoBar().Show("No access", "No access to read from file", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        public static void UnhandledException(string message) => new InfoBar().Show("Exception", "Unhandled exception: \n" + message, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        public static void ClearRecyclebinError() => new InfoBar().Show("Clear recyclebin", "Error occured while clearing the recycle bin", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        public static void DeleteFromRecyclebinError() => new InfoBar().Show("Delete from recyclebin", "Error occured while deleting the file from recycle bin", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        public static void RecyclebinClearSucceed() => new InfoBar().Show("Clear recyclebin", "Successfully cleared recycle bin", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success);
        public static void MoveToRecyclebinError() => new InfoBar().Show("Move to recyclebin", "Error occured while moving the file to recyclebin", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        public static void OpenFromRecyclebinError() => new InfoBar().Show("Open from recyclebin", "Error occured while opening the file from the recyclebin", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        public static void FileNameInvalidCharacters() => new InfoBar().Show("Invalid character", "The text entered contains invalid characters for a file name", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        public static void DesignLoadError(string designName, Exception ex) => new InfoBar().Show("Design load failed", "Could not load the design " + designName + "\n" + ex.Message, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        public static void DetectEncodingError(Exception ex) => new InfoBar().Show("Detect encoding", "Could not detect the encoding\n" + ex.Message, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        public static void SettingsExportSucceed() => new InfoBar().Show("Settings export", "Settings successfully exported", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success);
        public static void SettingsImportSucceed() => new InfoBar().Show("Settings import", "Settings successfully imported", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success);
        public static void SettingsExportFailed() => new InfoBar().Show("Settings export", "Failed while exporting settings", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        public static void SettingsImportFailed() => new InfoBar().Show("Settings import", "Faild while importing settings", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        public static void ClearTemporaryFilesFailed() => new InfoBar().Show("Temporary files", "Failed while clearing temporary files", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        public static void ClearTemporaryFilesSucceed() => new InfoBar().Show("Temporary files", "Successfully cleared temporary files", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success);

        public static void WelcomeMessage()
        {
            var btn = new Button { Content = "Designs" };
            btn.Click += delegate
            {
                TabPageHelper.mainPage.ShowSettings("DesignPage");
            };
            new InfoBar().Show("Welcome to Fastedit", "To customize your experience,\npress the button below", btn, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success, 8);
        }

    }
}
