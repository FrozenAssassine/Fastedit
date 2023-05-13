using Fastedit.Settings;
using Microsoft.Graphics.Canvas.Text;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Fastedit.Views.SettingsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings_DocumentPage : Page
    {
        public string[] Fonts
        {
            get
            {
                return CanvasTextFormat.GetSystemFontFamilies().OrderBy(f => f).ToArray();
            }
        }

        public Settings_DocumentPage()
        {
            this.InitializeComponent();

            SpacesOrTabsSwitch.IsOn = AppSettings.GetSettingsAsBool(AppSettingsValues.Settings_UseSpacesInsteadTabs, DefaultValues.UseSpacesInsteadTabs);

            var value = AppSettings.GetSettingsAsInt(AppSettingsValues.Settings_SpacesPerTab, DefaultValues.NumberOfSpacesPerTab);
            SpacesPerTabSlider.Value = value == -1 ? DefaultValues.NumberOfSpacesPerTab : value;
            //load
            FontFamilyCombobox.SelectedItem = AppSettings.GetSettings(AppSettingsValues.Settings_FontFamily, DefaultValues.FontFamily);
            FontSizeNumberBox.Value = AppSettings.GetSettingsAsInt(AppSettingsValues.Settings_FontSize, DefaultValues.FontSize);

            ShowLinenumbersSwitch.IsOn = AppSettings.GetSettingsAsBool(AppSettingsValues.Settings_ShowLineNumbers, DefaultValues.ShowLinenumbers);
            EnableSyntaxhighlightingSwitch.IsOn = AppSettings.GetSettingsAsBool(AppSettingsValues.Settings_Syntaxhighlighting, DefaultValues.SyntaxHighlighting);
            ShowLinehighlighterSwitch.IsOn = AppSettings.GetSettingsAsBool(AppSettingsValues.Settings_ShowLineHighlighter, DefaultValues.ShowLineHighlighter);
        }

        private void SpacesOrTabsSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            AppSettings.SaveSettings(AppSettingsValues.Settings_UseSpacesInsteadTabs, SpacesOrTabsSwitch.IsOn);
        }
        private void SpacesPerTabSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            AppSettings.SaveSettings(AppSettingsValues.Settings_SpacesPerTab, SpacesPerTabSlider.Value);
        }

        private void FontFamilyCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppSettings.SaveSettings(AppSettingsValues.Settings_FontFamily, FontFamilyCombobox.SelectedItem);
        }

        private void FontSizeNumberBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            AppSettings.SaveSettings(AppSettingsValues.Settings_FontSize, FontSizeNumberBox.Value);
        }
        //Removed du to bug (#110)
        private void ShowLinenumbersSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            AppSettings.SaveSettings(AppSettingsValues.Settings_ShowLineNumbers, ShowLinenumbersSwitch.IsOn);
        }

        private void ShowLinehighlighterSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            AppSettings.SaveSettings(AppSettingsValues.Settings_ShowLineHighlighter, ShowLinehighlighterSwitch.IsOn);
        }

        private void EnableSyntaxhighlightingSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            AppSettings.SaveSettings(AppSettingsValues.Settings_Syntaxhighlighting, EnableSyntaxhighlightingSwitch.IsOn);
        }
    }
}
