using Microsoft.UI.Xaml.Controls;
using Fastedit.Core.Tab;

namespace Fastedit.Helper;

public class AppActivationHelper
{
    public static string appActivationArguments = null;

    public static bool HandleAppActivation(TabView tabView)
    {
        if (appActivationArguments == null)
            return false;

        //TODO! var args = Environment.GetCommandLineArgs();

        return HandleFileActivation(tabView);
    }

    private static bool HandleFileActivation(TabView tabView)
    {
        var file = appActivationArguments;
        if (file == null || file.Length == 0)
            return false;

        return TabPageHelper.OpenAndShowFile(tabView, file, true);
    }
}
