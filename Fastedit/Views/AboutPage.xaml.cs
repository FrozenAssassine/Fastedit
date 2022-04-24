using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Fastedit.Views
{
    public sealed partial class AboutPage : Page
    {
        private AppSettings appsettings = new AppSettings();

        public AboutPage()
        {
            this.InitializeComponent();

            RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsString("ThemeIndex", "0"));
            
            BitmapImage githubURI = new BitmapImage(
                new Uri("ms-appx:///Assets/SocialIcons/" + (RequestedTheme == ElementTheme.Light ? "GitHub-Mark-120px-plus.png" : "GitHub-Mark-Light-120px-plus.png")));
            GithubIconDisplay.Source = githubURI;

            FillInData();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void FillInData()
        {
            DeveloperDisplay.Text = $"{appsettings.GetResourceString("AboutPage_DevelopedBy/Text")} {Package.Current.PublisherDisplayName}";
            VersionDisplay.Text = appsettings.GetResourceString("AboutPage_Version/Text") + " " +
                    Package.Current.Id.Version.Major + "." +
                    Package.Current.Id.Version.Minor + "." +
                    Package.Current.Id.Version.Build;
        }

        private void CopyEmailAddress_Click(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText("fasteditsoftware@gmail.com");
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
        }
    }
}