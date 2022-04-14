using Microsoft.Services.Store.Engagement;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Fastedit.Views
{
    public sealed partial class AboutPage : Page
    {
        private AppSettings appsettings = new AppSettings();


        public AboutPage()
        {
            this.InitializeComponent();

            if (StoreServicesFeedbackLauncher.IsSupported())
            {
                ReportFeedbackButton.Visibility = Visibility.Visible;
            }

            RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsString("ThemeIndex", "0"));
            FillInData();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void FillInData()
        {
            DeveloperDisplay.Text = $"{AppSettings.GetResourceStringStatic("AboutPage_DevelopedBy/Text")} {Package.Current.PublisherDisplayName}";
            VersionDisplay.Text = AppSettings.GetResourceStringStatic("AboutPage_Version/Text") + " " +
                    Package.Current.Id.Version.Major + "." +
                    Package.Current.Id.Version.Minor + "." +
                    Package.Current.Id.Version.Build;
        }


        private async void ReportBugSubmitFeature_Click(object sender, RoutedEventArgs e)
        {
            await StoreServicesFeedbackLauncher.GetDefault().LaunchAsync();
        }

        private void PrivacyPolicies_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PrivacyPoliciesPage));
        }

        private void CopyEmailAddress_Click(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText("fasteditsoftware@gmail.com");
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
        }
    }
}