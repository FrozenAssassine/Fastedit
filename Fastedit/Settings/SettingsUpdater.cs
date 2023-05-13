using Fastedit.Controls;
using Fastedit.Helper;
using Fastedit.Tab;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Fastedit.Settings
{
    public class SettingsUpdater
    {
        private static double GetHeightWithVisibility(FrameworkElement control)
        {
            return control.Visibility == Visibility.Visible ? control.ActualHeight : 0;
        }
        private static void SetTabViewSettings(TabView tabView, FasteditDesign design)
        {
            (tabView.Resources["TabViewItemHeaderBackground"] as SolidColorBrush).Color = ConvertHelper.ToColor(design.UnselectedTabPageHeaderBackground);
            (tabView.Resources["TabViewItemHeaderBackgroundSelected"] as SolidColorBrush).Color = ConvertHelper.ToColor(design.SelectedTabPageHeaderBackground);
            (tabView.Resources["TabViewItemHeaderForeground"] as SolidColorBrush).Color = ConvertHelper.ToColor(design.UnSelectedTabPageHeaderTextColor);
            (tabView.Resources["TabViewItemHeaderForegroundSelected"] as SolidColorBrush).Color = ConvertHelper.ToColor(design.SelectedTabPageHeaderTextColor);

            tabView.TabWidthMode = (TabViewWidthMode)Enum.Parse(typeof(TabViewWidthMode), AppSettings.GetSettings(AppSettingsValues.Settings_TabViewWidthMode, "0"));
        }
        private static TextControlBox.TextControlBoxDesign CreateTextboxDesign(FasteditDesign currentDesign)
        {
            return new TextControlBox.TextControlBoxDesign(
                DesignHelper.CreateBackgroundBrush(ConvertHelper.ToColor(currentDesign.TextBoxBackground), currentDesign.TextboxBackgroundType),
                ConvertHelper.ToColor(currentDesign.TextColor),
                ConvertHelper.ToColor(currentDesign.SelectionColor),
                ConvertHelper.ToColor(currentDesign.CursorColor),
                ConvertHelper.ToColor(currentDesign.LineHighlighterBackground),
                ConvertHelper.ToColor(currentDesign.LineNumberColor),
                ConvertHelper.ToColor(currentDesign.LineNumberBackground),
                ConvertHelper.ToColor(currentDesign.SearchHighlightColor)
            );
        }

        public static void UpdateTab(TabPageItem tab, bool setMargin = true)
        {
            UpdateTabSettings(tab, CreateTextboxDesign(DesignHelper.CurrentDesign), DesignHelper.CurrentDesign.Theme, setMargin);
        }

        private static void UpdateTabSettings(TabPageItem tab, TextControlBox.TextControlBoxDesign textboxDesign, ElementTheme theme, bool setMargin = true)
        {
            tab.textbox.Design = textboxDesign;

            tab.textbox.FontSize = AppSettings.GetSettingsAsInt(AppSettingsValues.Settings_FontSize, DefaultValues.FontSize);
            tab.textbox.FontFamily = new FontFamily(AppSettings.GetSettings(AppSettingsValues.Settings_FontFamily, DefaultValues.FontFamily));

            tab.textbox.ShowLineHighlighter = AppSettings.GetSettingsAsBool(AppSettingsValues.Settings_ShowLineHighlighter, DefaultValues.ShowLineHighlighter);

            tab.textbox.ShowLineNumbers = AppSettings.GetSettingsAsBool(AppSettingsValues.Settings_ShowLineNumbers, DefaultValues.ShowLinenumbers);

            tab.textbox.SyntaxHighlighting = AppSettings.GetSettingsAsBool(AppSettingsValues.Settings_Syntaxhighlighting, DefaultValues.SyntaxHighlighting);

            //tab.textbox.UseSpacesInsteadTabs = AppSettings.GetSettingsAsBool(AppSettingsValues.Settings_UseSpacesInsteadTabs, DefaultValues.UseSpacesInsteadTabs);
            //tab.textbox.NumberOfSpacesForTab = AppSettings.GetSettingsAsInt(AppSettingsValues.Settings_SpacesPerTab, DefaultValues.NumberOfSpacesPerTab);
            tab.textbox.RequestedTheme = theme;

            if (setMargin)
                tab.textbox.Margin = TabPageHelper.TabMargin;
        }
        private static void UpdateTabPages(TabView tabView, FasteditDesign currentDesign)
        {
            var textboxDesign = CreateTextboxDesign(currentDesign);
            for (int i = 0; i < tabView.TabItems.Count; i++)
            {
                if (tabView.TabItems[i] is TabPageItem tab && tab != null)
                {
                    UpdateTabSettings(tab, textboxDesign, currentDesign.Theme);
                }
                else if (SettingsTabPageHelper.IsSettingsPage(tabView.TabItems[i]))
                {
                    DesignHelper.SetBackground(tabView.TabItems[i] as TabViewItem, ConvertHelper.ToColor(currentDesign.BackgroundColor), currentDesign.BackgroundType);
                }
            }

            //Load tabs or spaces
            object tag;
            if (!AppSettings.GetSettingsAsBool(AppSettingsValues.Settings_UseSpacesInsteadTabs, DefaultValues.UseSpacesInsteadTabs))
                tag = "-1";
            else
                tag = AppSettings.GetSettingsAsInt(AppSettingsValues.Settings_SpacesPerTab, DefaultValues.NumberOfSpacesPerTab);

            TabPageHelper.TabsOrSpaces(tabView, tag);
        }
        private static void SetSettingsToStatusbar(Grid statusbar, FasteditDesign design)
        {
            statusbar.Background = DesignHelper.CreateBackgroundBrush(ConvertHelper.ToColor(design.StatusbarBackgroundColor), design.StatusbarBackgroundType);
            var foreground = new SolidColorBrush(ConvertHelper.ToColor(design.StatusbarTextColor));

            var splitted = AppSettings.GetSettings(AppSettingsValues.Settings_StatusbarSorting, DefaultValues.StatusbarSorting).Split('|');
            for (int i = 0; i < statusbar.Children.Count; i++)
            {
                if (statusbar.Children[i] is StatusbarItem item)
                {
                    if (i < splitted.Length)
                        item.Visibility = ConvertHelper.BoolToVisibility(splitted[i].Equals("1"));
                    item.Foreground = foreground;
                }
            }
        }
        private static void SetMenubarAlignment(Microsoft.UI.Xaml.Controls.MenuBar menubar)
        {
            menubar.HorizontalAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), AppSettings.GetSettings(AppSettingsValues.Settings_MenubarAlignment, DefaultValues.MenubarAlignment.ToString()));
        }
        public static void SetMainPageSettings(Page mainPage, FasteditDesign currentDesign)
        {
            DesignHelper.SetBackground(mainPage, ConvertHelper.ToColor(currentDesign.BackgroundColor), currentDesign.BackgroundType);
        }
        public static void SetControlsVisibility(TabView tabView, Microsoft.UI.Xaml.Controls.MenuBar menuBar, Grid statusbar)
        {
            //do not apply -> controls will be visible in settings
            if (SettingsTabPageHelper.SettingsSelected)
                return;

            statusbar.Visibility = ConvertHelper.BoolToVisibility(AppSettings.GetSettingsAsBool(AppSettingsValues.Settings_ShowStatusbar, DefaultValues.ShowStatusbar));
            menuBar.Visibility = ConvertHelper.BoolToVisibility(AppSettings.GetSettingsAsBool(AppSettingsValues.Settings_ShowMenubar, DefaultValues.ShowMenubar));

            TabPageHelper.TabMargin.Top = GetHeightWithVisibility(menuBar);
            TabPageHelper.TabMargin.Bottom = GetHeightWithVisibility(statusbar);

            //Update tab margin:
            for (int i = 0; i < tabView.TabItems.Count; i++)
            {
                if (tabView.TabItems[i] is TabPageItem tab)
                {
                    tab.textbox.Margin = TabPageHelper.TabMargin;
                }
            }
        }

        public static async void UpdateSettings(MainPage mainPage, TabView tabView, Microsoft.UI.Xaml.Controls.MenuBar menuBar, Grid statusbar, FasteditDesign currentDesign)
        {
            //Load the desing from file
            if (currentDesign == null)
                await DesignHelper.LoadDesign();

            //update the design
            currentDesign = DesignHelper.CurrentDesign;

            //Apply the theme:
            ThemeHelper.CurrentTheme = currentDesign.Theme;

            //Controls
            SetControlsVisibility(tabView, menuBar, statusbar);

            //menubar
            SetMenubarAlignment(menuBar);

            //TabPages
            UpdateTabPages(tabView, currentDesign);

            //TabControl
            SetTabViewSettings(tabView, currentDesign);

            //MainPage
            SetMainPageSettings(mainPage, currentDesign);

            //Statusbar
            SetSettingsToStatusbar(statusbar, currentDesign);

            mainPage.RunCommandWindow.UpdateColors();

            //TabWindows (tabs as own windows):
            TabWindowHelper.UpdateSettings();
        }
    }
}
