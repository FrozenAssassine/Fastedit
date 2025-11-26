using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Fastedit.Helper
{
    internal class WindowStateHelper
    {
        public static OverlappedPresenterState GetWindowState(Window window)
        {
            var presenter = (window.AppWindow.Presenter as OverlappedPresenter);
            if (presenter == null)
                return OverlappedPresenterState.Restored;

            return presenter.State;
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
