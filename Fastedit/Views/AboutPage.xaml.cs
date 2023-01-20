using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Fastedit.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        public string ChangelogText => File.ReadAllText(@"Assets\changelog.txt");
        public string AppVersion =>
            Package.Current.Id.Version.Major + "." +
            Package.Current.Id.Version.Minor + "." +
            Package.Current.Id.Version.Build;

        public string DeveloperName => Package.Current.PublisherDisplayName;

        public AboutPage()
        {
            this.InitializeComponent();
        }

        private async void NavigateToLink_Click(Controls.SettingsControl sender)
        {
            if (sender.Tag == null)
                return;

            await Windows.System.Launcher.LaunchUriAsync(new Uri(sender.Tag.ToString()));
        }
    }
}
