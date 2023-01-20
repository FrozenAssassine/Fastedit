using Fastedit.Controls;
using Fastedit.Tab;
using System;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
    public class InfoMessages
    {
        public static StackPanel InfoMessagePanel = null;

        public static void Add(InfobarMessage message)
        {
            InfoMessagePanel.Children.Add(message);
        }
        public static void NoAccesToSaveFile() => Add(new InfobarMessage("No access", "No access to write to file", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error));
        public static void NoAccesToReadFile() => Add(new InfobarMessage("No access", "No access to read from file", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error));
        public static void UnhandledException(string message) => Add(new InfobarMessage("Exception", "Unhandled exception: \n" + message, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error));
        public static void ClearRecyclebinError() => Add(new InfobarMessage("Clear recyclebin", "Error occured while clearing the recycle bin", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error));
        public static void DeleteFromRecyclebinError() => Add(new InfobarMessage("Delete from recyclebin", "Error occured while deleting the file from recycle bin", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error));
        public static void RecyclebinClearSucceed() => Add(new InfobarMessage("Clear recyclebin", "Successfully cleared recycle bin", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success));
        public static void MoveToRecyclebinError() => Add(new InfobarMessage("Move to recyclebin", "Error occured while moving the file to recyclebin", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error));
        public static void OpenFromRecyclebinError() => Add(new InfobarMessage("Open from recyclebin", "Error occured while opening the file from the recyclebin", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error));
        public static void FileNameInvalidCharacters() => Add(new InfobarMessage("Invalid character", "The text entered contains invalid characters for a file name", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error));
        public static void DesignLoadError(string designName, Exception ex) => Add(new InfobarMessage("Design load failed", "Could not load the design " + designName + "\n" + ex.Message, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error));
        public static void DetectEncodingError(Exception ex) => Add(new InfobarMessage("Detect encoding", "Could not detect the encoding\n" + ex.Message, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error));
        public static void SettingsExportSucceed() => Add(new InfobarMessage("Settings export", "Settings successfully exported", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success));
        public static void SettingsImportSucceed() => Add(new InfobarMessage("Settings import", "Settings successfully imported", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success));
        public static void SettingsExportFailed() => Add(new InfobarMessage("Settings export", "Failed while exporting settings", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error));
        public static void SettingsImportFailed() => Add(new InfobarMessage("Settings import", "Faild while importing settings", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error));
        public static void ClearTemporaryFilesFailed() => Add(new InfobarMessage("Temporary files", "Failed while clearing temporary files", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error));
        public static void ClearTemporaryFilesSucceed() => Add(new InfobarMessage("Temporary files", "Successfully cleared temporary files", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success));

        public static void WelcomeMessage()
        {
            var btn = new Button { Content = "Designs", Margin = new Windows.UI.Xaml.Thickness(0, 0, 0, 10) };
            btn.Click += delegate
            {
                TabPageHelper.mainPage.ShowSettings("DesignPage");
            };
            Add(new InfobarMessage("Welcome to Fastedit", "To customize your experience,\npress the button below", btn, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success, 8));
        }

    }
}
