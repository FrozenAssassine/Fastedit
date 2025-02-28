using System.Collections.Generic;
using System.IO;
using Fastedit.Core.Settings;
using Fastedit.Dialogs;
using Fastedit.Views;
using Microsoft.UI.Xaml;

namespace Fastedit.Helper;

internal class DesignWindowHelper
{
    static List<(Window, Microsoft.UI.Windowing.AppWindow)> OpenWindows = new();

    public static void EditDesign(string designName)
    {
        var design = DesignHelper.GetDesignFromFile(Path.Combine(DefaultValues.DesignPath, designName));

        if (design == null)
            return;

        ShowWindow(design, designName);
    }

    public static bool IsWindowOpen()
    {
        return OpenWindows.Count > 0;
    }
    
    private static Window ShowWindow(FasteditDesign design, string designName)
    {
        var window = new Window();
        window.ExtendsContentIntoTitleBar = true;
        window.AppWindow.Closing += AppWindow_Closing;
        window.Content = new DesignEditor(design, designName);
        window.Title = "Edit Design " + designName;

        window.Activate();

        OpenWindows.Add((window, window.AppWindow));
        return window;
    }

    private static async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        int index = OpenWindows.FindIndex(x => x.Item2 == sender);
        if (index == -1)
        {
            return; //window not found
        }

        Window window = OpenWindows[index].Item1;
        var designEditor = window.Content as DesignEditor;

        if (designEditor == null || !designEditor.NeedSave)
        {
            OpenWindows.RemoveAt(index);
            return; //close the window
        }

        args.Cancel = true; //prompt save

        var res = await AskSaveDesignDialog.Show(designEditor);
        switch (res)
        {
            case Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary:
                if (designEditor.SaveDesign())
                {
                    InfoMessages.SaveDesignSucceeded();
                    OpenWindows.RemoveAt(index);
                    args.Cancel = false;
                    window.Close();
                }
                else
                {
                    InfoMessages.SaveDesignError();
                }
                break;
            case Microsoft.UI.Xaml.Controls.ContentDialogResult.Secondary:
                OpenWindows.RemoveAt(index);
                args.Cancel = false;
                window.Close();
                break;
            default:
                break;
        }
    }

}
