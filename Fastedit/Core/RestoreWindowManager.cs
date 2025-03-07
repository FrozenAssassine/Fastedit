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

    public static bool IsWindowPositionValid(Window window, RectInt32 restoreBounds)
    {
        var displayArea = DisplayArea.GetFromRect(restoreBounds, DisplayAreaFallback.Nearest);

        if (displayArea != null)
        {
            var screenBounds = displayArea.WorkArea;

            return restoreBounds.X >= screenBounds.X &&
                   restoreBounds.Y >= screenBounds.Y &&
                   restoreBounds.X + restoreBounds.Width <= screenBounds.X + screenBounds.Width &&
                   restoreBounds.Y + restoreBounds.Height <= screenBounds.Y + screenBounds.Height;
        }
        return false;
    }

    public void RestoreSettings()
    {
        var state = WindowStateHelper.SetWindowState(window, AppSettings.WindowState);

        var width = AppSettings.WindowWidth;
        var height = AppSettings.WindowHeight;
        var left = AppSettings.WindowLeft;
        var top = AppSettings.WindowTop;

        if (width < 200)
            width = 1100;
        if (height < 100)
            height = 700;

        if (state != OverlappedPresenterState.Maximized)
        {
            RectInt32 restoreBounds = new RectInt32(left, top, width, height);
            if (IsWindowPositionValid(window, restoreBounds))
                window.AppWindow.MoveAndResize(restoreBounds);
            else
                window.AppWindow.Resize(new SizeInt32(width, height));
        }
        Debug.WriteLine(left + ":" + top + ":" + width + ":" + height);
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
