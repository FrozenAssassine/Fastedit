using Fastedit.Dialogs;
using System;
using System.IO;
using System.Linq;
using Microsoft.UI.Xaml.Controls;

namespace Fastedit.Views.SettingsPages
{
    public sealed partial class Settings_TabControl : Page
    {
        public Settings_TabControl()
        {
            this.InitializeComponent();
            TabSizecombobox.SelectedIndex = AppSettings.TabViewWidthMode;

            newTabTitleTextbox.Text = AppSettings.NewTabTitle + AppSettings.NewTabExtension;
        }

        private void TabSizecombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppSettings.TabViewWidthMode = TabSizecombobox.SelectedIndex;
        }

        private void NewTabTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            //validate data:
            string data = (sender as TextBox).Text;
            if (data.Length == 0)
                return;

            //check for invalid characters
            char[] invalidChars = Path.GetInvalidFileNameChars();
            for (int i = 0; i < data.Length; i++)
            {
                if (invalidChars.Contains(data[i]))
                {
                    InfoMessages.FileNameInvalidCharacters();
                    return;
                }
            }

            AppSettings.NewTabExtension = Path.GetExtension(data.Trim());
            AppSettings.NewTabTitle = Path.GetFileNameWithoutExtension(data.Trim());
        }
    }
}
