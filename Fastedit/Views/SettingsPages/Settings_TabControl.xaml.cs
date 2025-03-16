using Fastedit.Dialogs;
using System.IO;
using Microsoft.UI.Xaml.Controls;
using Fastedit.Extensions;

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

            if (data.ContainsInvalidPathChars())
            {
                InfoMessages.FileNameInvalidCharacters();
                return;
            }

            AppSettings.NewTabExtension = Path.GetExtension(data.Trim());
            AppSettings.NewTabTitle = Path.GetFileNameWithoutExtension(data.Trim());
        }
    }
}
