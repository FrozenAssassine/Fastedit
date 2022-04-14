using System;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Fastedit.Extensions
{
    public static class Utilities
    {
        public static void SetCursor(CoreCursorType cursor)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(cursor, 0);
        }

        /// <summary>
        /// Set the fullscreen mode
        /// </summary>
        /// <param name="Fullscreen"></param>
        /// <param name="appsettings"></param>
        /// <returns>whether the app entered Fullscreen or not</returns>
        public static bool FullScreen(bool Fullscreen, AppSettings appsettings = null)
        {
            try
            {
                bool isinfullscreen = false;
                if (!Fullscreen)
                {
                    ApplicationView.GetForCurrentView().ExitFullScreenMode();
                    isinfullscreen = false;
                }
                else
                {
                    isinfullscreen = ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                }
                if (appsettings != null)
                    appsettings.SaveSettings("FullScreen", isinfullscreen);
                return isinfullscreen;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in MainPage --> Fullscreen:" + "\n" + e.Message);
                return false;
            }
        }
        public static bool ToggleFullscreen(AppSettings appsettings = null)
        {
            return FullScreen(!ApplicationView.GetForCurrentView().IsFullScreenMode, appsettings);
        }
    }
}
