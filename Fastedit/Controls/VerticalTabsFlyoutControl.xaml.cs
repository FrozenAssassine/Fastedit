using Fastedit.Helper;
using Fastedit.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Fastedit.Core.Tab;

namespace Fastedit.Controls;

public sealed partial class VerticalTabsFlyoutControl : Flyout
{
    private static string typedString = "";
    private static DispatcherTimer timer;
    public TabView tabView = TabPageHelper.mainPage.tabView;

    public VerticalTabsFlyoutControl()
    {
        this.InitializeComponent();
        TabPageHelper.mainPage.verticalTabsFlyout = this;
    }

    public void UpdateFlyoutIfOpen()
    {
        if(this.IsOpen)
           UpdateFlyout();
    }

    public void Show(FrameworkElement placementTarget)
    {
        this.ShowAt(placementTarget);

        //update the index:
        listView.Focus(FocusState.Programmatic);
    }

    private void UpdateFlyout()
    {
        List<TabFlyoutItem> items = new List<TabFlyoutItem>(tabView.TabItems.Count - (SettingsTabPageHelper.SettingsPageOpen ? 1 : 0));
        foreach (var tab in tabView.TabItems)
        {
            if (SettingsTabPageHelper.IsSettingsPage(tab))
                continue;

            items.Add(new TabFlyoutItem
            {
                Tab = tab as TabPageItem
            });
        }

        listView.ItemsSource = items;
        listView.Tag = null;
        listView.Focus(FocusState.Programmatic);
        listView.SelectedIndex = tabView.SelectedIndex > listView.Items.Count ? 0 : -1;
    }

    private void ShowAllTabsFlyout_Opened(object sender, object e)
    {
        UpdateFlyout();
    }
    private void AllTabsFlyout_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (listView.SelectedItem is TabFlyoutItem tabFlyoutItem)
        {
            //Don't hide the flyout when the tag has a value -> for searching and up/down using arrow keys
            if (listView.Tag == null)
            {
                tabView.SelectedItem = tabFlyoutItem.Tab;

                //hide the flyout
                this.Hide();
            }
        }
    }
    private void AllTabsFlyout_CharacterReceived(UIElement sender, CharacterReceivedRoutedEventArgs args)
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
        typedString += args.Character;

        var matches = listView.Items.Where(e => (e as TabFlyoutItem).Matches(typedString));
        if (matches.Count() > 0)
        {
            //set the tag to NOT null so in the event it can be identified to not select any tab
            listView.Tag = "";
            listView.SelectedItem = matches.First();
        }
    }
    private void AllTabsFlyout_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            if (listView.Tag != null && listView.SelectedItem is TabFlyoutItem tabFlyoutItem)
                tabView.SelectedItem = tabFlyoutItem.Tab;

            //hide the flyout
            this.Hide();
        }
        else if (e.Key == VirtualKey.Down || e.Key == VirtualKey.Up)
        {
            if (listView.SelectedIndex < listView.Items.Count - 1 || listView.SelectedIndex > 0)
            {
                listView.Tag = "";
                listView.Focus(FocusState.Programmatic);
            }
        }
    }
    private async void AllTabsFlyout_CloseTab(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is TabPageItem tab)
        {
            await TabPageHelper.CloseTab(tabView, tab);
            UpdateFlyout();
        }
    }
    private void NewFile_Click(object sender, RoutedEventArgs e)
    {
        TabPageHelper.AddNewTab(tabView, true);
        UpdateFlyout();
    }

    //type to search
    private static void Timer_Tick(object sender, object e)
    {
        timer.Stop();
        typedString = "";
    }
}