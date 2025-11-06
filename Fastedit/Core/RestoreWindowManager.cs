using Fastedit.Core.Settings;
using Fastedit.Helper;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using Windows.Graphics;

namespace Fastedit.Core;

public class RestoreWindowManager
{
    private Window window;
    public RestoreWindowManager(Window window)
    {
        this.window = window;

        window.AppWindow.Closing += AppWindow_Closing;
    }

    private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        SaveSettings();
    }

    public void RestoreSettings()
    {
        var width = AppSettings.WindowWidth;
        var height = AppSettings.WindowHeight;
        var left = AppSettings.WindowLeft;
        var top = AppSettings.WindowTop;

        if (width < 200)
            width = 1100;
        if (height < 100)
            height = 700;

        RectInt32 restoreBounds = new RectInt32(left, top, width, height);

        //minimized windows have weird positions
        if (left != -32000 && top != -32000)
            window.AppWindow.MoveAndResize(restoreBounds);

        window.AppWindow.Resize(new SizeInt32(width, height));

        WindowStateHelper.SetWindowState(window, AppSettings.WindowState);
    }

    private void SaveSettings()
    {
        AppSettings.WindowWidth = window.AppWindow.Size.Width;
        AppSettings.WindowHeight = window.AppWindow.Size.Height;
        AppSettings.WindowLeft = window.AppWindow.Position.X;
        AppSettings.WindowTop = window.AppWindow.Position.Y;
        var state = WindowStateHelper.GetWindowState(window);
        AppSettings.WindowState = state;
    }
}
