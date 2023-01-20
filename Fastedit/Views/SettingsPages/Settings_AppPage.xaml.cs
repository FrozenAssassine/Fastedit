using Fastedit.Settings;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Fastedit.Views.SettingsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings_AppPage : Page
    {
        public Settings_AppPage()
        {
            this.InitializeComponent();

            //load:
            ShowMenubarToggleSwitch.IsOn = AppSettings.GetSettingsAsBool(AppSettingsValues.Settings_ShowMenubar, true);
            ShowStatusbarToggleSwitch.IsOn = AppSettings.GetSettingsAsBool(AppSettingsValues.Settings_ShowStatusbar, true);
            LanguageCombobox.SelectedItem = LanguageCombobox.Items.Where(x => (x as ComboBoxItem).Tag.ToString() == AppSettings.GetSettings(AppSettingsValues.Settings_Language));
        }

        private void ShowStatusbar_Toggled(object sender, RoutedEventArgs e)
        {
            AppSettings.SaveSettings(AppSettingsValues.Settings_ShowStatusbar, ShowStatusbarToggleSwitch.IsOn);
        }

        private void ShowMenubar_Toggled(object sender, RoutedEventArgs e)
        {
            AppSettings.SaveSettings(AppSettingsValues.Settings_ShowMenubar, ShowMenubarToggleSwitch.IsOn);
        }

        private void LanguageCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageCombobox.SelectedItem is ComboBoxItem item)
            {
                AppSettings.SaveSettings(AppSettingsValues.Settings_Language, item.Tag);
            }
        }
    }
}
