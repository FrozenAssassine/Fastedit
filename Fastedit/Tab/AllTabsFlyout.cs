using Fastedit.Helper;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Globalization.DateTimeFormatting;
using Windows.Networking.NetworkOperators;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Tab
{
    public class AllTabsFlyout
    {
        private static string typedString = "";
        private static DispatcherTimer timer;

        public static void UpdateFlyout(TabView tabView, ListView listView)
        {
            List<TabFlyoutItem> items = new List<TabFlyoutItem>(tabView.TabItems.Count - (SettingsTabPageHelper.SettingsPageOpen ? 1 : 0));
            foreach (var tab in tabView.TabItems)
            {
                if (!SettingsTabPageHelper.IsSettingsPage(tab))
                {
                    items.Add(new TabFlyoutItem { Tab = tab as TabPageItem });
                }
            }

            listView.ItemsSource = items;
        }
        //type to search
        public static void Flyout_CharacterReceived(char character, ListView listView)
        {
            if (timer == null)
            {
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 0, 800);
                timer.Start();
                timer.Tick += Timer_Tick;
            }

            if (!timer.IsEnabled)
                timer.Start();
            typedString += character;

            var matches = listView.Items.Where(e => (e as TabFlyoutItem).Matches(typedString));
            if (matches.Count() > 0)
            {
                //set the tag to NOT null so in the event it can be identified to not select any tab
                listView.Tag = "";
                listView.SelectedItem = matches.First();
            }
        }

        private static void Timer_Tick(object sender, object e)
        {
            timer.Stop();
            typedString = "";
        }
    }

    public class TabFlyoutItem
    {
        public bool Matches(string FileName) => Tab.DatabaseItem.FileName.Contains(FileName, StringComparison.OrdinalIgnoreCase);

        public TabPageItem Tab { get; set; }
    }
}
