using Microsoft.UI.Xaml.Controls;
using Fastedit.Core.Tab;
using Microsoft.UI.Windowing;

namespace Fastedit.Helper;

public class AppActivationHelper
{
    public static string appActivationArguments = null;

    public static bool HandleAppActivation(TabView tabView)
    {
        if (appActivationArguments != null)
            return HandleFileActivation(tabView);
        return false;
    }

    private static bool HandleFileActivation(TabView tabView)
    {
        var presenter = App.m_window.AppWindow.Presenter as OverlappedPresenter;
        presenter.Minimize();
        presenter.Restore();

        var file = appActivationArguments;
        if (file == null || file.Length == 0)
            return false;

        appActivationArguments = null;

        return TabPageHelper.OpenAndShowFile(tabView, file, true);
    }
}
