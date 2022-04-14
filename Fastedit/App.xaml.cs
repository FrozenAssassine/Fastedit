using Fastedit.Core.Tab;
using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Fastedit
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();

            SetSystemGroupAsync();
            Suspending += App_Suspending;
            //this.UnhandledException += (sender, e) =>
            //{
            //    e.Handled = true;
            //    System.Diagnostics.Debug.WriteLine(e.Exception);
            //};
        }

        public int MainViewId = -1;

        private async void SetSystemGroupAsync()
        {
            var jumpList = await Windows.UI.StartScreen.JumpList.LoadCurrentAsync();
            jumpList.SystemGroupKind = Windows.UI.StartScreen.JumpListSystemGroupKind.Recent;
            await jumpList.SaveAsync();
        }

        private Frame CreateRootFrame()
        {
            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = new Frame
                {
                    //Language = Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride
                };
                rootFrame.NavigationFailed += OnNavigationFailed;
                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
                MainViewId = ApplicationView.GetForCurrentView().Id;
            }
            return rootFrame;
        }

        private void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            Frame rootFrame = CreateRootFrame();

            if (args.Kind == ActivationKind.CommandLineLaunch)
            {
                if (args is CommandLineActivatedEventArgs commandLine)
                {
                    rootFrame.Navigate(typeof(MainPage), new CommandLineLaunchNavigationParameter
                    {
                        Arguments = commandLine.Operation.Arguments,
                        CurrentDirectoryPath = commandLine.Operation.CurrentDirectoryPath
                    });
                }
                else
                {
                    rootFrame.Navigate(typeof(MainPage));
                }
            }

            Window.Current.Activate();
        }
        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            Frame rootFrame = CreateRootFrame();

            rootFrame.Navigate(typeof(MainPage), args);
            Window.Current.Activate();
            base.OnFileActivated(args);
        }
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            AppSettings appsettings = new AppSettings();
            Frame rootFrame = CreateRootFrame();
            rootFrame.Navigate(typeof(MainPage));
            Window.Current.Activate();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
