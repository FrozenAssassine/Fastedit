using Microsoft.UI.Xaml.Controls;
using Fastedit.Core.Tab;
using System;
using System.Linq;
using Microsoft.Windows.AppLifecycle;
using Windows.Storage;
using System.Diagnostics;
using Windows.ApplicationModel.Activation;
using System.Collections.Generic;

namespace Fastedit.Helper;

public class AppActivationHelper
{
    public static AppActivationArguments activatedEventArgs;

    public static bool HandleAppActivation(TabView tabView)
    {
        if (activatedEventArgs == null)
            return false;

        if (activatedEventArgs.Kind == ExtendedActivationKind.Launch)
        {
            activatedEventArgs = null;
            return false;
        }

        if (activatedEventArgs.Kind == ExtendedActivationKind.File)
        {
            var files = (activatedEventArgs.Data as FileActivatedEventArgs).Files;
            activatedEventArgs = null;

            return HandleFileActivation(tabView, files);
        }

        if(activatedEventArgs.Kind == ExtendedActivationKind.CommandLineLaunch)
        {
            Debug.WriteLine(activatedEventArgs.Data.GetType());
        }

        activatedEventArgs = null;
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

    private static bool HandleFileActivation(TabView tabView, IReadOnlyList<IStorageItem> files)
    {
        if (files == null)
            return false;

        bool oneSuccess = false;
        foreach (var file in files)
        {
            if(TabPageHelper.OpenAndShowFile(tabView, file.Path, true))
            {
                oneSuccess = true;
            }
        }
        return oneSuccess;
    }
}
