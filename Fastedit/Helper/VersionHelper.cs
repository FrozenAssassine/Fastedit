using Fastedit.Controls;
using Fastedit.Settings;
using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Helper
{
    public class VersionHelper
    {
        private static bool IsOnNewVersion(string version)
        {
            string lastSavedVersion = AppSettings.GetSettings(AppSettingsValues.App_Version);
            AppSettings.SaveSettings(AppSettingsValues.App_Version, version);

            //no version saved -> first start
            if (lastSavedVersion.Length == 0)
                return false;

            if (!version.Equals(lastSavedVersion, StringComparison.Ordinal))
                return true;

            return false;
        }

        public static void CheckNewVersion(StackPanel infobarDisplay)
        {
            string version = Package.Current.Id.Version.Major + "." +
                Package.Current.Id.Version.Minor + "." +
                Package.Current.Id.Version.Build;

            if (IsOnNewVersion(version))
            {
                ShowNewVersionInfo(infobarDisplay, version);
            }
        }

        private static void ShowNewVersionInfo(StackPanel infobarDisplay, string version)
        {
            infobarDisplay.Children.Add(new InfobarMessage("New version", "Welcome to Fastedit version " + version, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success));
        }
    }
}
