using Fastedit.Helper;
using Fastedit.Tab;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
    internal class SelectTabPageDialog
    {
        public static async Task<TabPageItem> Show(TabView tabView, TabPageItem firstTab)
        {
            ListView tabList = new ListView();
            foreach(TabPageItem tab in tabView.TabItems)
            {

                if (SettingsTabPageHelper.settingsPage == tab || tab == firstTab)
                    continue;

                tabList.Items.Add(new ListViewItem { Content = tab.Header, Tag = tab });
            }

            var dialog = new ContentDialog
            {
                Background = DialogHelper.ContentDialogBackground(),
                Foreground = DialogHelper.ContentDialogForeground(),
                RequestedTheme = DialogHelper.DialogDesign,
                Title = "Select Document",
                Content = tabList,
                PrimaryButtonText = "Done",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
            };
            var res = await dialog.ShowAsync();
            if (res == ContentDialogResult.Primary && tabList.SelectedIndex >= 0)
            {
                return (tabList.SelectedItem as ListViewItem).Tag as TabPageItem;
            }
            return null;
        }
    }
}
