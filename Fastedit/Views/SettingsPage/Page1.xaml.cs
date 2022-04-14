using Fastedit.Core;
using Fastedit.Extensions;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Fastedit.Views.SettingsPage
{
    public sealed partial class Page1 : Page
    {
        private readonly AppSettings appsettings = new AppSettings();

        public List<string> Fonts
        {
            get
            {
                return CanvasTextFormat.GetSystemFontFamilies().OrderBy(f => f).ToList();
            }
        }
        public List<string> FontSizes = new List<string>()
        {
            "11",
            "12",
            "14",
            "16",
            "18",
            "20",
            "24",
            "28",
            "34",
            "38"
        };

        public Page1()
        {
            this.InitializeComponent();
            RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsString("ThemeIndex", "0"));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadOnNavigate();
        }

        private void LoadOnNavigate()
        {
            RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsString("ThemeIndex", "0"));

            //retrieve values
            FontSizeCombobox.SelectedIndex = FontSizes.IndexOf(appsettings.GetSettingsAsString("FontSize", DefaultValues.DefaultFontsize.ToString()));
            FontCombobox.SelectedIndex = Fonts.IndexOf(appsettings.GetSettingsAsString("FontFamily", DefaultValues.DefaultFontFamily));
            ShowLineNumbersButton.IsOn = appsettings.GetSettingsAsBool("ShowLineNumbers", true);
            HandwritingEnabled.IsOn = appsettings.GetSettingsAsBool("HandwritingEnabled", false);
            ShowSelectionFlyout.IsOn = appsettings.GetSettingsAsBool("TextboxShowSelectionFlyout", false);
            ShowLineHighlighter.IsOn = appsettings.GetSettingsAsBool("LineHighlighter", true);
        }

        //FontFamily
        private void FontCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            appsettings.SaveSettings("FontFamily", FontCombobox.SelectedItem);
        }
        //FontSize
        private void FontSizeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            appsettings.SaveSettings("FontSize", FontSizeCombobox.SelectedItem);
        }

        private void ShowSelectionFlyout_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("TextboxShowSelectionFlyout", ShowSelectionFlyout.IsOn);
        }

        private void HandwritingEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("HandwritingEnabled", HandwritingEnabled.IsOn);
        }

        private void ShowLineHighlighter_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("LineHighlighter", ShowLineHighlighter.IsOn);
        }

        private void ShowLineNumbersButton_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("ShowLineNumbers", ShowLineNumbersButton.IsOn);
        }
    }
}