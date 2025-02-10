using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using Windows.ApplicationModel;

namespace Fastedit
{
    public sealed partial class MainWindow : Window
    {
        public static DispatcherQueue UIDispatcherQueue = null;
        public static XamlRoot XamlRoot = null;

        public static StackPanel InfoMessagesPanel;

        public IntPtr WindowHandle;

        public MainWindow()
        {
            this.InitializeComponent();

            this.WindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);

            XamlRoot = this.Content.XamlRoot;
            UIDispatcherQueue = DispatcherQueue.GetForCurrentThread();
            InfoMessagesPanel = this.infoMessagesPanel;

            this.ExtendsContentIntoTitleBar = AppSettings.HideTitlebar;

            SetTitleBar(mainPage.TitleBarGrid);

            this.Title = "Fastedit";
            this.AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets\\AppIcon\\Icon.ico"));
        }

        public void SendLaunchArguments()
        {
            mainPage.TriggerAppActivationAfterStart();
        }
    }
}
