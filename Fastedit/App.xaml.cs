using Fastedit.Helper;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;

namespace Fastedit;

public partial class App : Application
{
    public static MainWindow m_window;
    private readonly SingleInstanceDesktopApp _singleInstanceApp;

    public App()
    {
        this.InitializeComponent();

        //_singleInstanceApp = new SingleInstanceDesktopApp("fastedit.juliuskirsch");
        //_singleInstanceApp.Launched += _singleInstanceApp_Launched;
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var appInstance = AppInstance.GetCurrent();
        var activatedArgs = appInstance.GetActivatedEventArgs();

        var mainInstance = AppInstance.FindOrRegisterForKey("fastedit.juliuskirsch");

        if (!mainInstance.IsCurrent)
        {
            await mainInstance.RedirectActivationToAsync(activatedArgs);
            Environment.Exit(0);
            return;
        }

        // Register event to handle future activations
        appInstance.Activated += AppInstance_Activated; ;

        // Create and show main window
        m_window = new MainWindow();
        m_window.Activate();

        HandleActivation(activatedArgs);
    }

    private void AppInstance_Activated(object sender, AppActivationArguments e)
    {
        if (m_window == null)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        HandleActivation(e);
    }

    private void HandleActivation(AppActivationArguments args)
    {
        AppActivationHelper.activatedEventArgs = args;
        m_window?.SendLaunchArguments();
    }
}
