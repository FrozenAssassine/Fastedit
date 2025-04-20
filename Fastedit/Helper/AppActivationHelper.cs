using Microsoft.UI.Xaml.Controls;
using Fastedit.Core.Tab;
using System;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace Fastedit.Helper;

public class AppActivationHelper
{
    public static string appActivationArguments = null;

    public static bool HandleAppActivation(TabView tabView)
    {
        if (appActivationArguments != null)
            return HandleFileActivation(tabView);

        var args = Environment.GetCommandLineArgs();
        if (args.Length == 0)
            return false;
        return HandleCommandLineActivation(tabView, args);
    }

    private static bool HandleCommandLineActivation(TabView tabView, string[] args)
    {       
        //no file, just open the app
        if (args.Length == 1)
            return true;

        int successfullyOpened = 0;
        if (args.Length >= 2)
        {
            string[] files = args.Skip(1).ToArray();
            foreach (var file in files)
            {
                if (TabPageHelper.OpenAndShowFile(tabView, file, false))
                    successfullyOpened++;
            }
        }
        return successfullyOpened != 0;
    }

    private static bool HandleFileActivation(TabView tabView)
    {
        var file = appActivationArguments;
        if (file == null || file.Length == 0)
            return false;

        appActivationArguments = null;

        return TabPageHelper.OpenAndShowFile(tabView, file, true);
    }
}
