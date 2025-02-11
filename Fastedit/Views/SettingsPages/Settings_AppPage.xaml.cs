using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Text;

namespace Fastedit.Views.SettingsPages
{
    public sealed partial class Settings_AppPage : Page
    {
        private bool lockSave = false;
        private Dictionary<ToggleSwitch, string> StatusbarToggleSwitches;
        private Dictionary<string, ToggleSwitch> StatusbarStrings;

        public Settings_AppPage()
        {
            this.InitializeComponent();

            ShowMenubarToggleSwitch.IsOn = AppSettings.ShowMenubar;
            ShowStatusbarToggleSwitch.IsOn = AppSettings.ShowStatusbar;
            menubarAlignmentCombobox.SelectedIndex = AppSettings.MenubarAlignment;
            HideTitlebarToggle.IsOn = AppSettings.HideTitlebar;

            //Load the statusbar sorting:
            lockSave = true;
            LoadStatusbarSorting();
            lockSave = false;
        }

        private void LoadStatusbarSorting()
        {
            StatusbarToggleSwitches = new Dictionary<ToggleSwitch, string>
            {
                { showZoomItem, "Zoom" },
                { showLineColumnItem, "LineColumn" },
                { showEncodingItem, "Encoding" },
                { showFileItem, "FileName" },
                { showWordChars, "WordChar" }
            };

            StatusbarStrings = new Dictionary<string, ToggleSwitch>
           {
                {"Zoom", showZoomItem},
                {"LineColumn",showLineColumnItem},
                {"Encoding", showEncodingItem},
                {"FileName", showFileItem},
                {"WordChar", showWordChars }
            };

            //turn them all on (fallback)
            foreach (var item in StatusbarStrings.Values)
                item.IsOn = true;

            var sorting = AppSettings.StatusbarSorting.Split("|", System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in sorting)
            {
                var splitted = item.Split(":");
                if (splitted.Length != 2)
                    continue;

                if (StatusbarStrings.TryGetValue(splitted[0].Trim(), out ToggleSwitch toggleSwitch))
                    toggleSwitch.IsOn = splitted[1] == "1";
            }


        }

        private void ShowStatusbar_Toggled(object sender, RoutedEventArgs e)
        {
            AppSettings.ShowStatusbar = ShowStatusbarToggleSwitch.IsOn;
        }

        private void ShowMenubar_Toggled(object sender, RoutedEventArgs e)
        {
            AppSettings.ShowMenubar = ShowMenubarToggleSwitch.IsOn;
        }

        private void Statusbar_ShowItem_Toggled(object sender, RoutedEventArgs e)
        {
            if (lockSave)
                return;

            //save the statusbar sorting
            StringBuilder sb = new StringBuilder();
            foreach (var item in StatusbarToggleSwitches)
            {
                sb.Append(item.Value);
                sb.Append(":");
                sb.Append(item.Key.IsOn ? 1 : 0);
                sb.Append("|");
            }
            string sorting = sb.ToString();
            AppSettings.StatusbarSorting = sorting;
        }

        private void menubarAlignmentCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppSettings.MenubarAlignment = menubarAlignmentCombobox.SelectedIndex;
        }

        private void HideTitlebar_Toggled(object sender, RoutedEventArgs e)
        {
            AppSettings.HideTitlebar = HideTitlebarToggle.IsOn;
        }
    }
}
