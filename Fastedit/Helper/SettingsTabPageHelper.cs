using Fastedit.Core.Tab;
using Fastedit.Models;
using Fastedit.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace Fastedit.Helper;

public delegate void SettingsPageClosedEvent();

public class SettingsTabPageHelper
{
    public static TabViewItem settingsPage = null;
    public static bool SettingsSelected { get; set; } = false;
    public static bool SettingsPageOpen = false;

    public static event SettingsPageClosedEvent SettingsTabClosed;

    public static void InitialiseTab(MainPage mainPage, TabView tabView, string page = null)
    {
        if (settingsPage == null)
            settingsPage = new TabViewItem
            {
                Header = "Settings",
                IconSource = new SymbolIconSource { Symbol = Symbol.Setting },
                Content = new SettingsPage(new SettingsNavigationParameter(mainPage, tabView, page)),
            };
    }

    public static void HideControls()
    {
        if (TabPageHelper.mainPage == null)
            return;

        foreach(var control in TabPageHelper.mainPage.ControlsToHideInSettings)
        {
            control.Visibility = Visibility.Collapsed;
        }
    }

    public static void OpenSettings(MainPage mainPage, TabView tabView, string page = null)
    {
        InitialiseTab(mainPage, tabView, page);

        if (!tabView.TabItems.Contains(settingsPage))
        {
            tabView.TabItems.Add(settingsPage);
        }

        SettingsPageOpen = true;
        tabView.SelectedItem = settingsPage;
    }
    public static void CloseSettings(TabView tabView)
    {
        //this check prevents a crash, happing when closing the settings tab while it is selected
        //I think it is a problem with tabview!
        if (tabView.SelectedIndex == 0 && IsSettingsPage(tabView.SelectedItem))
        {
            tabView.SelectedIndex = 1;
        }
        tabView.TabItems.Remove(settingsPage);

        SettingsPageOpen = SettingsSelected = false;
        SettingsTabClosed?.Invoke();
    }
    public static bool IsSettingsPage(object item)
    {
        return item is TabViewItem tab && tab.Content is SettingsPage;
    }
}
