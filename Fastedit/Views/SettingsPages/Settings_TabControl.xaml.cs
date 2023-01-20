using Fastedit.Dialogs;
using Fastedit.Settings;
using System;
using System.IO;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Views.SettingsPages
{
    public sealed partial class Settings_TabControl : Page
    {
        public Settings_TabControl()
        {
            this.InitializeComponent();
            TabSizecombobox.SelectedIndex = AppSettings.GetSettingsAsInt(AppSettingsValues.Settings_TabViewWidthMode);

            newTabTitleTextbox.Text = AppSettings.GetSettings(AppSettingsValues.Settings_NewTabTitle, DefaultValues.NewTabTitle) + AppSettings.GetSettings(AppSettingsValues.Settings_NewTabExtension, DefaultValues.NewTabExtension);
        }

        private void TabSizecombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppSettings.SaveSettings(AppSettingsValues.Settings_TabViewWidthMode, TabSizecombobox.SelectedIndex);
        }

        private void NewTabTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textbox)
            {
                //validate data:
                string data = textbox.Text;
                if (data.Length == 0)
                    return;

                //check for invalid characters
                char[] invalidChars = Path.GetInvalidFileNameChars();
                for (int i = 0; i < data.Length; i++)
                {
                    if (invalidChars.Contains(data[0]))
                    {
                        InfoMessages.FileNameInvalidCharacters();
                        return;
                    }
                }

                AppSettings.SaveSettings(AppSettingsValues.Settings_NewTabTitle, Path.GetFileNameWithoutExtension(data.Trim()));
                AppSettings.SaveSettings(AppSettingsValues.Settings_NewTabExtension, Path.GetExtension(data.Trim()));
            }
        }
    }
}
