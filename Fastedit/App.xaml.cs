using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Fastedit
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            SetSystemGroupAsync();
        }

        //Enable recent files in the taskbar rightclick menu
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
            }
            return rootFrame;
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            CreateRootFrame().Navigate(typeof(MainPage), args);
            Window.Current.Activate();
        }

        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            CreateRootFrame().Navigate(typeof(MainPage), args);
            Window.Current.Activate();
            base.OnFileActivated(args);
        }
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            CreateRootFrame().Navigate(typeof(MainPage));
            Window.Current.Activate();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
        private void OnSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
