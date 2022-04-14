using Fastedit.Core;
using Fastedit.Extensions;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Fastedit.Views.SettingsPage
{
    public sealed partial class Page8 : Page
    {
        private AppSettings appsettings = new AppSettings();

        public Page8()
        {
            this.InitializeComponent();
            RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsString("ThemeIndex", "0"));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsString("ThemeIndex", "0"));

            ShowHideStatusbarSwitch.IsOn = appsettings.GetSettingsAsBool("ShowStatusbar", true);
            ShowStatusbarFontInBold.IsChecked = appsettings.GetSettingsAsBool("StatusbarInBoldFont", false);

            LineNumberDisplay.IsOn = appsettings.GetSettingsAsBool("ShowLinenumberButtonOn_SBar", true);
            RenameDisplay.IsOn = appsettings.GetSettingsAsBool("ShowRenameButtonOn_SBar", true);
            ZoomDisplay.IsOn = appsettings.GetSettingsAsBool("ShowZoomButtonOn_SBar", true);
            EncodingDisplay.IsOn = appsettings.GetSettingsAsBool("ShowEncodingButtonOn_SBar", true);
            SaveStatusDisplay.IsOn = appsettings.GetSettingsAsBool("ShowSaveStatusButtonOn_SBar", true);
            WordCountDisplay.IsOn = appsettings.GetSettingsAsBool("ShowWordCountButtonOn_SBar", false);
            ShowHideStatusbarSwitch_Toggled(null, null);
        }
        private void ShowHideStatusbarSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("ShowStatusbar", ShowHideStatusbarSwitch.IsOn);
            bool IsEnabled = ShowHideStatusbarSwitch.IsOn;
            LineNumberDisplay.IsEnabled = IsEnabled;
            RenameDisplay.IsEnabled = IsEnabled;
            ZoomDisplay.IsEnabled = IsEnabled;
            EncodingDisplay.IsEnabled = IsEnabled;
            SaveStatusDisplay.IsEnabled = IsEnabled;
            ShowStatusbarFontInBold.IsEnabled = IsEnabled;
        }

        private void BoldFontCheckBox_CheckedUnChecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox)
            {
                var cb = sender as CheckBox;
                appsettings.SaveSettings("StatusbarInBoldFont", cb.IsChecked);
            }
        }

        private void EncodingDisplay_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("ShowEncodingButtonOn_SBar", EncodingDisplay.IsOn);
        }
        private void ZoomDisplay_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("ShowZoomButtonOn_SBar", ZoomDisplay.IsOn);
        }
        private void LineNumberDisplay_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("ShowLinenumberButtonOn_SBar", LineNumberDisplay.IsOn);
        }
        private void RenameDisplay_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("ShowRenameButtonOn_SBar", RenameDisplay.IsOn);
        }

        private void SaveStatusDisplay_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("ShowSaveStatusButtonOn_SBar", SaveStatusDisplay.IsOn);
        }

        private void WordCountDisplay_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("ShowWordCountButtonOn_SBar", WordCountDisplay.IsOn);
        }
    }
}
