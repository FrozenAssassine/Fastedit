using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Linq;
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
            //remove items that are not getting used
            while (listView.Items.Count > tabView.TabItems.Count)
            {
                listView.Items.RemoveAt(listView.Items.Count - 1);
            }

            //only update the names on same length
            if (listView.Items.Count == tabView.TabItems.Count)
            {
                for (int i = 0; i < tabView.TabItems.Count; i++)
                {
                    if (tabView.TabItems[i] is TabPageItem tab && listView.Items[i] is TabFlyoutItem tabFlyoutItem)
                    {
                        tabFlyoutItem.Tab = tab;
                    }
                }
            }
            else if (listView.Items.Count < tabView.TabItems.Count)
            {
                //replace all possible ones
                for (int i = 0; i < listView.Items.Count; i++)
                {
                    if (tabView.TabItems[i] is TabPageItem tab && listView.Items[i] is TabFlyoutItem tabFlyoutItem)
                    {
                        tabFlyoutItem.Tab = tab;
                    }
                }

                //create new ones
                for (int i = listView.Items.Count; i < tabView.TabItems.Count; i++)
                {
                    if (tabView.TabItems[i] is TabPageItem tab)
                    {
                        listView.Items.Add(new TabFlyoutItem
                        {
                            Tab = tab
                        });
                    }
                }
            }
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

            Debug.WriteLine(typedString);

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
