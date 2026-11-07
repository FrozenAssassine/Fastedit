using Fastedit.Core;
using Fastedit.Helper;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace Fastedit;

public partial class App : Application
{
    public static MainWindow m_window;
    private readonly SingleInstanceDesktopApp _singleInstanceApp;

    public App()
    {
        LocalizationManager.Instance.Initialize();
        this.InitializeComponent();

        _singleInstanceApp = new SingleInstanceDesktopApp("fastedit.juliuskirsch");
        _singleInstanceApp.Launched += _singleInstanceApp_Launched;
    }

    private async void _singleInstanceApp_Launched(object sender, SingleInstanceLaunchEventArgs e)
    {
        if (e.IsFirstLaunch)
        {
            await LocalizationManager.Instance.InitializeLocalizerAsync();
            m_window = new MainWindow();
            m_window.Activate();
        }

        //file activation
        if (e.Arguments != null && e.Arguments.Length > 0)
        {
            AppActivationHelper.appActivationArguments = e.Arguments;

            if (!e.IsFirstLaunch)
                m_window?.SendLaunchArguments();
        }

        m_window?.BringToForeground();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _singleInstanceApp.Launch(args.Arguments);
    }
}
