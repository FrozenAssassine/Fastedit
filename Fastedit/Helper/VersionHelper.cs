using Fastedit.Dialogs;
using System;
using Windows.ApplicationModel;

namespace Fastedit.Helper;

public class VersionHelper
{
    private static bool IsOnNewVersion(string version)
    {
        string lastSavedVersion = AppSettings.AppVersion;
        AppSettings.AppVersion = version;

        //no version saved -> first start
        if (lastSavedVersion.Length == 0)
            return false;

        if (!version.Equals(lastSavedVersion, StringComparison.Ordinal))
            return true;

        return false;
    }

    public static string GetCurrentVersion()
    {
        return Package.Current.Id.Version.Major + "." +
            Package.Current.Id.Version.Minor + "." +
            Package.Current.Id.Version.Build;
    }

    public static void CheckNewVersion()
    {
        var version = GetCurrentVersion();
        if (IsOnNewVersion(version))
        {
            InfoMessages.NewVersionInfo(version);
        }
    }
}
