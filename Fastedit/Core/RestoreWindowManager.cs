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

        //no saved size -> first launch, center the window on the primary display
        if (width < 200 || height < 100 || !AppSettings.FirstStart)
        {
            width = 1100;
            height = 700;

            var workArea = DisplayArea.Primary.WorkArea;
            width = System.Math.Min(width, workArea.Width);
            height = System.Math.Min(height, workArea.Height);
            left = workArea.X + (workArea.Width - width) / 2;
            top = workArea.Y + (workArea.Height - height) / 2;
        }

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
