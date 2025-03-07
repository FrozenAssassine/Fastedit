using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Fastedit.Helper
{
    internal class WindowStateHelper
    {
        public static OverlappedPresenterState GetWindowState(Window window)
        {
            return (window.AppWindow.Presenter as OverlappedPresenter).State;
        }

        public static OverlappedPresenterState SetWindowState(Window window, OverlappedPresenterState state)
        {
            var presenter = window.AppWindow.Presenter as OverlappedPresenter;
            if (state == OverlappedPresenterState.Maximized)
                presenter.Maximize();
            else if (state == OverlappedPresenterState.Minimized)
                presenter.Minimize();

            return state;
        }
    }
}
