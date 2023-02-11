using Fastedit.Dialogs;
using Fastedit.Storage;
using Fastedit.Tab;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;

namespace Fastedit.Helper
{
    public class AppActivationHelper
    {
        public static NavigationEventArgs NavigationEvent = null;

        public static async Task<bool> HandleAppActivation(TabView tabView)
        {
            if (NavigationEvent.Parameter == null)
                return false;

            if (NavigationEvent.Parameter is IActivatedEventArgs args)
            {
                if (args.Kind == ActivationKind.Launch)
                {
                    return true;
                }
                else if (args.Kind == ActivationKind.CommandLineLaunch)
                {
                    if (args is CommandLineActivatedEventArgs handler)
                    {
                        var cmd_args = handler.Operation.Arguments.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        string cmd_dir = handler.Operation.CurrentDirectoryPath;

                        await HandleCommandLineLaunch(tabView, cmd_args, cmd_dir);
                    }
                }
                else if (args.Kind == ActivationKind.File)
                {
                    if (args is FileActivatedEventArgs handler)
                    {
                        return await TabPageHelper.OpenFiles(tabView, handler.Files);
                    }
                }
            }
            return false;
        }

        //Handle the commandline parameters
        public static async Task HandleCommandLineLaunch(TabView tabView, string[] args, string curpath)
        {
            try
            {
                if (args.Length == 0)
                    return;

                string fileName = "";

                if (args.Contains("-n") && args.Length >= 1) //argument to create new tab
                {
                    if (args.Length > 1) //when the file has to be renamed
                        fileName = args[1];

                    TabPageHelper.AddNewTab(tabView, true, fileName);
                    return;
                }
                else if (args.Contains("-r") && args.Length == 3) //argument to change the name of the opening file
                {
                    if (args.Length == 3)
                        fileName = args[2];
                }

                string path = GetAbsolutePath(args[0], curpath);
                if (path == null || path.Length == 0)
                    return;

                var file = await StorageFile.GetFileFromPathAsync(path);
                if (file != null)
                {
                    var newTab = await OpenFileHelper.DoOpen(tabView, file);
                    if (fileName.Length > 0)
                    {
                        newTab.SetHeader(fileName);
                    }

                    tabView.SelectedItem = newTab;
                }
            }
            catch (FileNotFoundException)
            {
                InfoMessages.FileNotFound(args.Length > 0 ? args[0] : "");
            }
            catch (Exception ex)
            {
                InfoMessages.OpenFileFromCmdError(ex);
            }
        }

        private static string GetAbsolutePath(string path, string dir)
        {
            if (path.StartsWith("\"") && path.Length > 1)
            {
                var index = path.IndexOf('\"', 1);
                if (index == -1)
                    return null;
                path = path.Substring(1, index - 1);
            }

            path = path.Trim('/').Replace('/', Path.DirectorySeparatorChar);

            if (!String.IsNullOrWhiteSpace(path)
                   && path.IndexOfAny(Path.GetInvalidPathChars().ToArray()) == -1
                   && Path.IsPathRooted(path)
                   && !Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                return path;
            }

            if (path.StartsWith(".\\"))
                path = dir + Path.DirectorySeparatorChar + path.Substring(2, path.Length - 2);
            else if (path.StartsWith("..\\"))
                path = GetAbsolutePath(dir, path);
            else
                path = dir + Path.DirectorySeparatorChar + path;
            return path;
        }
    }
}
