using Microsoft.UI.Xaml.Controls;
using Fastedit.Core.Tab;
using Microsoft.UI.Windowing;
using System.Threading.Tasks;

namespace Fastedit.Helper;

public class AppActivationHelper
{
    public static string appActivationArguments = null;

    public static async Task<bool> HandleAppActivation(TabView tabView)
    {
        if (appActivationArguments != null)
            return await HandleFileActivation(tabView);
        return false;
    }

    private static async Task<bool> HandleFileActivation(TabView tabView)
    {
        var presenter = App.m_window.AppWindow.Presenter as OverlappedPresenter;
        presenter.Minimize();
        presenter.Restore();

        var file = appActivationArguments;
        if (file == null || file.Length == 0)
            return false;

        appActivationArguments = null;

        return await TabPageHelper.OpenAndShowFile(tabView, file, true);
    }
}
