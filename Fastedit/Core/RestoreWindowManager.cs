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
    private WindowStateManager windowStateManager;

    public RestoreWindowManager(Window window, WindowStateManager windowStateManager)
    {
        this.window = window;
        this.windowStateManager = windowStateManager;
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

        window.AppWindow.MoveAndResize(restoreBounds);
        WindowStateHelper.SetWindowState(window, AppSettings.WindowState);
    }

    private void SaveSettings()
    {
         var windowPosSize = windowStateManager.GetWindowSizePosStateIndependent();

        AppSettings.WindowWidth = windowPosSize.size.Width;
        AppSettings.WindowHeight = windowPosSize.size.Height;
        AppSettings.WindowLeft = windowPosSize.position.X;
        AppSettings.WindowTop = windowPosSize.position.Y;
        AppSettings.WindowState = windowPosSize.state;
    }
}
