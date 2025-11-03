using Fastedit.Core;
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
        public readonly BackdropWindowManager backdropManager;
        public readonly RestoreWindowManager restoreWindowManager;

        public MainWindow()
        {
            this.InitializeComponent();
            backdropManager = new BackdropWindowManager(this);
            restoreWindowManager = new RestoreWindowManager(this);
            restoreWindowManager.RestoreSettings();

            this.WindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);

            XamlRoot = this.Content.XamlRoot;
            UIDispatcherQueue = DispatcherQueue.GetForCurrentThread();
            InfoMessagesPanel = this.infoMessagesPanel;
            this.ExtendsContentIntoTitleBar = AppSettings.HideTitlebar;

            this.mainPage.TitleBarGrid.LayoutUpdated += TitleBarGrid_LayoutUpdated;
            this.Closed += MainWindow_Closed;


            this.Title = "Fastedit";
            this.AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets\\AppIcon\\Icon.ico"));
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            this.mainPage.TitleBarGrid.LayoutUpdated -= TitleBarGrid_LayoutUpdated;
        }

        private void TitleBarGrid_LayoutUpdated(object sender, object e)
        {
            SetTitleBar(mainPage.TitleBarGrid);
        }

        public void SendLaunchArguments()
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                await mainPage.TriggerAppActivationAfterStart();
            });
        }
    }
}
