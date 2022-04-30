using Fastedit.Core;
using Fastedit.Extensions;
using Fastedit.ExternalData;
using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit.Views.SettingsPage
{
    public sealed partial class Page2 : Page
    {
        private readonly AppSettings appsettings = new AppSettings();
        private bool SaveColorsAfterComboboxIndexChanged = false;

        public Page2()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsString("ThemeIndex", "0"));
            SaveColorsAfterComboboxIndexChanged = false;

            ThemeCombobox.SelectedIndex = appsettings.GetSettingsAsInt("ThemeIndex", 0);
            ShowHideDropDownMenuButton.IsOn = appsettings.GetSettingsAsBool("ShowDropdown", true);
            NavigateToPreviousTab.IsOn = appsettings.GetSettingsAsBool("ShowNavigateToPreviousTab", false);
            NavigateToNextTab.IsOn = appsettings.GetSettingsAsBool("ShowNavigateToNextTab", false);
            PageSettings.SelectComboBoxItemByTag(Languagecombobox, appsettings.GetSettingsAsString("AppLanguage", "en-US"));
            ToggleThemeChange.IsOn = appsettings.GetSettingsAsBool("AutomaticThemeChange", false);
            ShowMenuBarToggleButton.IsOn = appsettings.GetSettingsAsBool("ShowMenubar", true);
            MenubarAlignment.SelectedIndex = appsettings.GetSettingsAsInt("MenuBarAlignment", 1);
            SaveColorsAfterComboboxIndexChanged = true;

            var designs = await CustomDesigns.GetAllInstalledDesigns();
            for (int i = 0; i < designs.Count; i++)
            {
                ThemeCombobox1.Items.Add(
                    new ComboBoxItem
                    {
                        Name = designs[i].Name + "1",
                        Content = designs[i].Name.Replace(DefaultValues.Extension_FasteditDesign, ""),
                        Tag = designs[i]
                    });
                ThemeCombobox2.Items.Add(
                    new ComboBoxItem
                    {
                        Name = designs[i].Name + "2",
                        Content = designs[i].Name.Replace(DefaultValues.Extension_FasteditDesign, ""),
                        Tag = designs[i]
                    });
                ThemeCombobox1.SelectedItem = ThemeCombobox1.FindName(appsettings.GetSettingsAsString("DesignForLightMode", DefaultValues.DefaultThemeName) + "1");
                ThemeCombobox2.SelectedItem = ThemeCombobox2.FindName(appsettings.GetSettingsAsString("DesignForDarkMode", DefaultValues.DefaultThemeName) + "2");
            }
        }

        //Language --> Page2
        private async void Language_SelectionChange(object sender, SelectionChangedEventArgs e)
        {
            var OldLanguage = appsettings.GetSettingsAsString("AppLanguage", "");
            var LanguageTag = ((ComboBoxItem)Languagecombobox.SelectedItem).Tag.ToString();
            appsettings.SaveSettings("AppLanguage", LanguageTag);

            if (OldLanguage != LanguageTag && SaveColorsAfterComboboxIndexChanged == true)
            {
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = LanguageTag;
                await new Dialogs.InfoBox("To let all changes take effect, please restart the application!", "").ShowAsync();
            }
        }
        //Theme --> Page2
        private void ThemeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            appsettings.SaveSettings("ThemeIndex", ThemeCombobox.SelectedIndex);
            this.RequestedTheme = ThemeHelper.RootTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), ThemeCombobox.SelectedIndex.ToString());
        }

         //Show/Hide UI
        private void ShowDropDownMenu_Toggled(object sender, RoutedEventArgs e)
        {
            if (!ShowHideDropDownMenuButton.IsOn && SaveColorsAfterComboboxIndexChanged && !ShowMenuBarToggleButton.IsOn)
            {
                GetBackToSettingsTeachingTip.IsOpen = true;
            }
            appsettings.SaveSettings("ShowDropdown", ShowHideDropDownMenuButton.IsOn);
        }

        private void ShowNavigateToPreviousTab_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("ShowNavigateToPreviousTab", NavigateToPreviousTab.IsOn);
        }
        private void ShowNavigateToNextTab_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("ShowNavigateToNextTab", NavigateToNextTab.IsOn);
        }

        private void GetBackToSettingsTeachingTip_CloseButtonClick(muxc.TeachingTip sender, object args)
        {
            GetBackToSettingsTeachingTip.IsOpen = false;
        }

        private void ThemeCombobox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeCombobox1.SelectedItem is ComboBoxItem cbitem)
            {
                if (cbitem.Tag is StorageFile file)
                {
                    appsettings.SaveSettings("DesignForLightMode", file.Name);
                }
            }
        }
        private void ThemeCombobox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeCombobox2.SelectedItem is ComboBoxItem cbitem)
            {
                if (cbitem.Tag is StorageFile file)
                {
                    appsettings.SaveSettings("DesignForDarkMode", file.Name);
                }
            }
        }
        private void ToggleThemeChange_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("AutomaticThemeChange", ToggleThemeChange.IsOn);
        }

        private void ShowMenubar_Toggled(object sender, RoutedEventArgs e)
        {
            if (!ShowHideDropDownMenuButton.IsOn && SaveColorsAfterComboboxIndexChanged && !ShowHideDropDownMenuButton.IsOn)
            {
                GetBackToSettingsTeachingTip.IsOpen = true;
            }
            appsettings.SaveSettings("ShowMenubar", ShowMenuBarToggleButton.IsOn);
        }

        private void MenubarAlignment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            appsettings.SaveSettings("MenuBarAlignment", MenubarAlignment.SelectedIndex);
        }
    }
}
