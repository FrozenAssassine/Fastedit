using Fastedit.Tab;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
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
                /*else if (args.Kind == ActivationKind.CommandLineLaunch)
                {

                    return false;
                    var handler = args as CommandLineActivatedEventArgs;
                    if (handler != null)
                    {
                        await new MessageDialog(handler.Operation.Arguments + ":" + handler.Operation.CurrentDirectoryPath, "").ShowAsync();

                        //var file = await HandleCommandLineLaunch(handler.Operation);
                        //if (file == null)
                        //    return false;

                        return false;
                        //return await TabPageHelper.OpenFile(tabView, file);
                    }
                }*/
                else if (args.Kind == ActivationKind.File)
                {
                    var handler = args as FileActivatedEventArgs;
                    if (handler != null)
                    {
                        return await TabPageHelper.OpenFiles(tabView, handler.Files);
                    }
                }
            }
            return false;
        }

        //Handle the commandline parameters
        public static async Task<StorageFile> HandleCommandLineLaunch(CommandLineActivationOperation operation)
        {
            try
            {
                if (operation.Arguments.Length < 1)
                    return null;

                string path = GetAbsolutePath(operation.Arguments, operation.CurrentDirectoryPath);
                if (path == null || path.Length < 1)
                    return null;

                return await StorageFile.GetFileFromPathAsync(path);
            }
            catch (FileNotFoundException)
            {
                //ShowInfobar(InfoBarMessages.FileNotFound, InfoBarMessages.FileNotFoundTitle, muxc.InfoBarSeverity.Error);
            }
            catch (UnauthorizedAccessException)
            {
                //ShowInfobar(InfoBarMessages.FileNoAccess, InfoBarMessages.FileNoAccessTitle, muxc.InfoBarSeverity.Error);
            }
            catch (ArgumentException)
            {
                //ShowInfobar(InfoBarMessages.FileInvalidPath, InfoBarMessages.FileInvalidPathTitle, muxc.InfoBarSeverity.Error);
            }
            return null;
        }

        //Get the path from the commandline parameters
        public static string GetAbsolutePath(string path, string dir)
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
