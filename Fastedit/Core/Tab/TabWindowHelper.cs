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
    public static Dictionary<Window, TabPageItem> OpenWindows { get; } = new();

    public static bool ShowInNewWindow(TabView tabView, TabPageItem tab)
    {
        TabWindowHelper.tabView = tabView;

        if (tab == null)
            return false;

        //load the tab otherwise it has no text
        if (!tab.DataIsLoaded)
            TabPageHelper.LoadUnloadedTab(tab);

        tabView.TabItems.Remove(tab);

        tab.RemoveTextbox();
        tab.DatabaseItem.HasOwnWindow = true;

        var window = new Window();
        window.Content = new TabWindowPage(tab, window, new BackdropWindowManager(window));
        window.AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets\\AppIcon\\Icon.ico"));
        window.Title = tab.DatabaseItem.FileName;
        window.ExtendsContentIntoTitleBar = AppSettings.HideTitlebar;
        window.Closed += Window_Closed;
        window.AppWindow.Closing += AppWindow_Closing;
        window.Activate();

        OpenWindows.Add(window, tab);
        UpdateSettings();
        tab.textbox.ClearSelection();

        return false;
    }

    //fired when the user closes the window, while it is still fully alive:
    //detach the textbox here, moving it during the teardown in Closed crashes WinUI natively
    private static void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        var window = OpenWindows.Keys.FirstOrDefault(x => x.AppWindow == sender);
        if (window?.Content is TabWindowPage page)
            page.Close();
    }

    private static void Window_Closed(object sender, WindowEventArgs args)
    {
        Window window = sender as Window;

        if (window == null)
            return;

        window.Closed -= Window_Closed;
        window.AppWindow.Closing -= AppWindow_Closing;

        if (!OpenWindows.TryGetValue(window, out var tab))
            return;
        OpenWindows.Remove(window);

        if (window.Content is not TabWindowPage page)
            return;

        page.Close();

        bool keepOwnWindow = !closeWithoutChanging;
        void MoveTabBack()
        {
            tab.AddTextbox();

            tabView.TabItems.Add(tab);
            tabView.SelectedItem = tab;
            tab.DatabaseItem.HasOwnWindow = keepOwnWindow;

            SettingsUpdater.UpdateTab(tab, false);
        }

        //when closing the app, the tab has to be back in the tabview before the database is saved
        if (keepOwnWindow)
            MoveTabBack();
        else
            tabView.DispatcherQueue.TryEnqueue(MoveTabBack);
    }

    public static void CloseAllWindows(bool closeForSave = true)
    {
        closeWithoutChanging = !closeForSave;

        foreach (var window in OpenWindows.Keys.ToArray())
        {
            window.Close();
        }

        closeWithoutChanging = true;
    }

    public static void UpdateSettings()
    {
        var design = DesignHelper.CurrentDesign;

        foreach (var item in OpenWindows)
        {
            var window = item.Key;

            if (window.Content is TabWindowPage page)
            {
                SettingsUpdater.SetWindowBackground(page.backDropWindowManager, DesignHelper.CurrentDesign);

                SettingsUpdater.UpdateTab(item.Value, false);

                SettingsUpdater.SetSettingsToStatusbar(page.Statusbar, DesignHelper.CurrentDesign);

                page.Statusbar.IsVisible = AppSettings.ShowStatusbar;
                ThemeHelper.SetWindowTheme(window, design.Theme);

                page.TitleText.Foreground = new SolidColorBrush(ConvertHelper.ToColor(design.SelectedTabPageHeaderTextColor));
            }
        }
    }
}
