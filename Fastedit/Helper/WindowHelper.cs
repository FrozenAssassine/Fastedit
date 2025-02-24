using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Fastedit.Helper;

internal class WindowHelper
{
    public static void ToggleCompactOverlay(Window window)
    {
        bool isCompactOverlay = window.AppWindow.Presenter.Kind == AppWindowPresenterKind.CompactOverlay;
        window.AppWindow.SetPresenter(isCompactOverlay ? AppWindowPresenterKind.Default : AppWindowPresenterKind.CompactOverlay);
    }

    public static void ToggleFullscreen(Window window)
    {
        bool isFullscreen = window.AppWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen;
        window.AppWindow.SetPresenter(isFullscreen ? AppWindowPresenterKind.Default : AppWindowPresenterKind.FullScreen);
    }

    public static void ToggleTopMost(Window window)
    {
        if (window == null)
            return;

        var presenter = window.AppWindow.Presenter as OverlappedPresenter;
        if (presenter == null)
            return;

        presenter.IsAlwaysOnTop = !presenter.IsAlwaysOnTop;
    }
}
