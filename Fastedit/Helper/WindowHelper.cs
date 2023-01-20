using System;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;

namespace Fastedit.Helper
{
    internal class WindowHelper
    {
        private static bool FullScreen(bool Fullscreen)
        {
            try
            {
                if (Fullscreen)
                    return ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                else
                    ApplicationView.GetForCurrentView().ExitFullScreenMode();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool ToggleFullscreen()
        {
            return FullScreen(!ApplicationView.GetForCurrentView().IsFullScreen);
        }

        public static async void ToggleCompactOverlay()
        {
            await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(
                ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.Default ? ApplicationViewMode.CompactOverlay : ApplicationViewMode.Default);
        }

        public static void ToggleCompactOverlayForAppWindow(AppWindow window)
        {
            if (window.Presenter.GetConfiguration().Kind != AppWindowPresentationKind.CompactOverlay)
            {
                window.Presenter.RequestPresentation(AppWindowPresentationKind.CompactOverlay);
            }
            else
            {
                window.Presenter.RequestPresentation(AppWindowPresentationKind.Default);
            }
        }

        public static void ToggleFullscreenForAppWindow(AppWindow window)
        {
            if (window.Presenter.GetConfiguration().Kind != AppWindowPresentationKind.FullScreen)
            {
                window.Presenter.RequestPresentation(AppWindowPresentationKind.FullScreen);
            }
            else
            {
                window.Presenter.RequestPresentation(AppWindowPresentationKind.Default);
            }
        }
    }
}
