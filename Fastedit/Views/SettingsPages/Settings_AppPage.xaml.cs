using Fastedit.Helper;
using Fastedit.Settings;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
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
        int[] StatusbarItemSorting = new int[] { 0, 0, 0, 0, 0 };
        bool lockSave = false;

        public Settings_AppPage()
        {
            this.InitializeComponent();

            //load:
            ShowMenubarToggleSwitch.IsOn = AppSettings.GetSettingsAsBool(AppSettingsValues.Settings_ShowMenubar, true);
            ShowStatusbarToggleSwitch.IsOn = AppSettings.GetSettingsAsBool(AppSettingsValues.Settings_ShowStatusbar, true);

            //Load the statusbar sorting:
            lockSave = true;
            var splitted = AppSettings.GetSettings(AppSettingsValues.Settings_StatusbarSorting).Split('|');
            for (int i = 0; i < StatusbarItemSorting.Length; i++)
            {
                StatusbarItemSorting[i] = splitted[i].Equals("1") ? 1 : 0;

                var res = StatusbarItemGrid.Children.FirstOrDefault(x => (x is ToggleSwitch tsw) && ConvertHelper.ToInt(tsw.Tag, -1) == i);
                if (res == default(UIElement))
                    continue;

                if (res is ToggleSwitch sw)
                    sw.IsOn = StatusbarItemSorting[i] == 1;
            }
            lockSave = false;
        }

        private void ShowStatusbar_Toggled(object sender, RoutedEventArgs e)
        {
            AppSettings.SaveSettings(AppSettingsValues.Settings_ShowStatusbar, ShowStatusbarToggleSwitch.IsOn);
        }

        private void ShowMenubar_Toggled(object sender, RoutedEventArgs e)
        {
            AppSettings.SaveSettings(AppSettingsValues.Settings_ShowMenubar, ShowMenubarToggleSwitch.IsOn);
        }

        private void Statusbar_ShowItem_Toggled(object sender, RoutedEventArgs e)
        {
            if (lockSave)
                return;

            if (sender is ToggleSwitch swt && swt.Tag != null)
            {
                int itemIndex = ConvertHelper.ToInt(swt.Tag, -1);
                if (itemIndex < 0)
                    return;

                StatusbarItemSorting[itemIndex] = swt.IsOn ? 1 : 0;

                string arrayStr = "";
                for (int i = 0; i < StatusbarItemSorting.Length; i++)
                {
                    arrayStr += StatusbarItemSorting[i] + "|";
                }

                AppSettings.SaveSettings(AppSettingsValues.Settings_StatusbarSorting, arrayStr);
            }
        }
    }
}
