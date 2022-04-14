using System;
using Windows.UI.Xaml;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
    public class ErrorDialogs
    {
        public static muxc.InfoBar CreateInfobar(string Content = "", string Title = "", muxc.InfoBarSeverity severity = muxc.InfoBarSeverity.Warning, int VisibilityTime = 5, object content = null)
        {
            return new muxc.InfoBar
            {
                Severity = severity,
                Title = Title,
                Message = Content,
                IsOpen = true,
                Content = content,
                Margin = new Thickness(0, 0, 0, 5)
            };
            
        }
        
        public static muxc.InfoBar FileNoAccessExceptionErrorDialog(Exception e, string FilePath = "")
        {
            return CreateInfobar("No access to the file\n" + e.Message + "\nFile: " + FilePath, "", muxc.InfoBarSeverity.Error);
        }

        public static muxc.InfoBar FileNotFoundExceptionErrorDialog(Exception e, string FilePath = "")
        {
            return CreateInfobar("The file could not be found\n" + e.Message + "\nFile: " + FilePath, "", muxc.InfoBarSeverity.Error);
        }

        public static muxc.InfoBar LoadSettingsError(Exception e)
        {
            return CreateInfobar("Couldn't load app-settings!\n" + e.Message,"", muxc.InfoBarSeverity.Error);
        }

        public static muxc.InfoBar SaveErrorDialog()
        {
            return CreateInfobar(AppSettings.GetResourceStringStatic("ErrorDialogs_SaveErrorDialog/Text"),"", muxc.InfoBarSeverity.Error);
        }

        public static muxc.InfoBar SaveFileError(string DataBaseName)
        {
            return CreateInfobar(AppSettings.GetResourceStringStatic("ErrorDialogs_SaveFileError/Text") + DataBaseName, "", muxc.InfoBarSeverity.Error);
        }

        public static muxc.InfoBar OpenErrorDialog(string filepath = "")
        {
            return CreateInfobar(AppSettings.GetResourceStringStatic("ErrorDialogs_OpenErrorDialog/Text") + "\nFile: " +
                filepath, "", muxc.InfoBarSeverity.Error);
        }

        public static muxc.InfoBar CreateTabErrorDialog()
        {
            return CreateInfobar(AppSettings.GetResourceStringStatic("ErrorDialogs_CreateTabErrorDialog/Text"),"", muxc.InfoBarSeverity.Error);
        }

        public static muxc.InfoBar GetTabErrorDialog()
        {
            return CreateInfobar(AppSettings.GetResourceStringStatic("ErrorDialogs_GetTabError/Text"),"", muxc.InfoBarSeverity.Error);
        }

        public static muxc.InfoBar CloseTabError()
        {
            return CreateInfobar(AppSettings.GetResourceStringStatic("ErrorDialogs_CloseTabError/Text"), "", muxc.InfoBarSeverity.Error);
        }

        public static muxc.InfoBar LoadTabsError()
        {
            return CreateInfobar(AppSettings.GetResourceStringStatic("ErrorDialogs_LoadTabsError/Text"),"", muxc.InfoBarSeverity.Error);
        }

        public static muxc.InfoBar LoadTempTabError()
        {
            return CreateInfobar(AppSettings.GetResourceStringStatic("ErrorDialogs.LoadTempTabError/Text"), "", muxc.InfoBarSeverity.Error);
        }
        public static muxc.InfoBar LoadDesignError()
        {
            return CreateInfobar("Could not load the design", "", muxc.InfoBarSeverity.Error);
        }
    }
}
