using Fastedit.Helper;
using Fastedit.Views;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel;
using System.IO;
using Fastedit.Core.Settings;

namespace Fastedit.Core.Tab;

internal class TabWindowHelper
{
    private static TabView tabView = null;
    private static bool closeWithoutChanging = true;
    private static BackdropWindowManager backdropManager;
    public static Dictionary<Window, TabPageItem> OpenWindows { get; } = new();

    public static async Task<bool> ShowInNewWindow(TabView tabView, TabPageItem tab)
    {
        TabWindowHelper.tabView = tabView;

        if (tab == null)
            return false;

        tabView.TabItems.Remove(tab);

        tab.RemoveTextbox();
        tab.DatabaseItem.HasOwnWindow = true;

        var window = new Window();
        window.Content = new TabWindowPage(tab, window);
        window.AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets\\AppIcon\\Icon.ico"));
        window.Title = tab.DatabaseItem.FileName;
        window.ExtendsContentIntoTitleBar = AppSettings.HideTitlebar;
        window.Closed += Window_Closed;
        window.Activate();

        backdropManager = new BackdropWindowManager(window);

        OpenWindows.Add(window, tab);
        UpdateSettings();
        tab.textbox.ClearSelection();

        //Delay to prevent holding down the key
        await Task.Delay(400);
        return false;
    }

    private static void Window_Closed(object sender, WindowEventArgs args)
    {
        Window window = sender as Window;

        if (window == null)
            return;

        OpenWindows.TryGetValue(window, out var tab);
        OpenWindows.Remove(window);

        //remove the textbox from the window and add it back to the tab:
        if (window.Content is TabWindowPage page)
        {
            page.Close();
            tab.AddTextbox();

            tabView.TabItems.Add(tab);
            tabView.SelectedItem = tab;
            if (closeWithoutChanging)
            {
                tab.DatabaseItem.HasOwnWindow = false;
                closeWithoutChanging = true;
            }

            SettingsUpdater.UpdateTab(tab, false);
        }
    }

    public static void CloseAllWindows(bool closeForSave = true)
    {
        closeWithoutChanging = !closeForSave;

        int count = OpenWindows.Count;
        for (int i = 0; i < count; i++)
        {
            OpenWindows.Keys.ElementAt(0).Close();
        }
    }

    public static void UpdateSettings()
    {
        var gridBackground = new SolidColorBrush(ConvertHelper.ToColor(DesignHelper.CurrentDesign.SelectedTabPageHeaderBackground));
        foreach (var item in OpenWindows)
        {
            var window = item.Key;

            if (window.Content is TabWindowPage page)
            {
                SettingsUpdater.SetWindowBackground(backdropManager, DesignHelper.CurrentDesign);

                SettingsUpdater.UpdateTab(item.Value, false);

                SettingsUpdater.SetSettingsToStatusbar(page.Statusbar, DesignHelper.CurrentDesign);

                page.Statusbar.IsVisible = AppSettings.ShowStatusbar;

                page.MainGrid.Background = gridBackground;
            }
        }
    }
}
