using Fastedit.Helper;
using Fastedit.Settings;
using Fastedit.Views;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace Fastedit.Tab
{
    internal class TabWindowHelper
    {
        private static TabView tabView = null;
        private static bool closeWithoutChanging = true;

        public static Dictionary<AppWindow, TabPageItem> AppWindows { get; set; } = new Dictionary<AppWindow, TabPageItem>();

        public static async Task<bool> ShowInNewWindow(TabView tabView, TabPageItem tab)
        {
            TabWindowHelper.tabView = tabView;

            if (tab == null)
                return false;

            tabView.TabItems.Remove(tab);

            tab.RemoveTextbox();
            tab.DatabaseItem.HasOwnWindow = true;
            var window = await CreateWindow();
            window.Title = tab.DatabaseItem.FileName;
            window.TitleBar.ExtendsContentIntoTitleBar = true;
            window.TitleBar.ButtonBackgroundColor = Colors.Transparent;

            ElementCompositionPreview.SetAppWindowContent(window, new TabWindowPage(tab, window));

            window.Closed += Window_Closed;
            if (await window.TryShowAsync())
            {
                AppWindows.Add(window, tab);
                UpdateSettings();
                tab.textbox.ClearSelection();

                //Delay to prevent holding down the key
                await Task.Delay(400);

                return true;
            }
            return false;
        }

        public static async Task CloseAllWindows(bool closeForSave = true)
        {
            closeWithoutChanging = !closeForSave;

            int count = AppWindows.Count;
            for (int i = 0; i < count; i++)
            {
                await AppWindows.Keys.ElementAt(0).CloseAsync();
            }
        }

        /*public static async Task<bool> CloseWindow(TabPageItem tab)
        {
            Debug.WriteLine("CLose Window: "  +  tab.DatabaseItem.FileName);
            //Find the AppWindow that matches to the tab
            var res = AppWindows.Where(x => x.Value == tab);
            if(res.Count() <= 0) return false;

            await res.ElementAt(0).Key.CloseAsync();
            return true;
        }*/

        public static void UpdateSettings()
        {
            var gridBackground = new SolidColorBrush(ConvertHelper.ToColor(DesignHelper.CurrentDesign.SelectedTabPageHeaderBackground));
            foreach (var item in TabWindowHelper.AppWindows)
            {
                var window = item.Key;
                if (ElementCompositionPreview.GetAppWindowContent(window) is TabWindowPage page)
                {
                    //Fix for mica not working in appwindow:
                    if (DesignHelper.CurrentDesign.BackgroundType == BackgroundType.Mica)
                    {
                        Color bg = ThemeHelper.CurrentTheme == ElementTheme.Dark ? Color.FromArgb(255, 25, 25, 25) : Color.FromArgb(255, 255, 255, 255);
                        page.Background = new SolidColorBrush(bg);
                    }
                    else
                        SettingsUpdater.SetMainPageSettings(page, DesignHelper.CurrentDesign);

                    //apply settings to textbox without updating the margin:
                    SettingsUpdater.UpdateTab(item.Value, false);

                    //apply to tab:
                    page.MainGrid.Background = gridBackground;
                }
            }
        }

        private static void Window_Closed(AppWindow sender, AppWindowClosedEventArgs args)
        {
            AppWindows.TryGetValue(sender, out var tab);
            AppWindows.Remove(sender);

            //remove the textbox from the window and add it back to the tab:
            if (ElementCompositionPreview.GetAppWindowContent(sender) is TabWindowPage page)
            {
                page.Close();
                tab.AddTextbox();

                //Add the tab back
                tabView.TabItems.Add(tab);

                if (closeWithoutChanging)
                {
                    tab.DatabaseItem.HasOwnWindow = false;
                    closeWithoutChanging = true;
                }

                //apply settings to tab
                SettingsUpdater.UpdateTab(tab, false);
            }
        }

        private static async Task<AppWindow> CreateWindow()
        {
            return await AppWindow.TryCreateAsync();
        }
    }
}
