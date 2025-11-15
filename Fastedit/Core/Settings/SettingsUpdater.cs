using Fastedit.Controls;
using Fastedit.Helper;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using TextControlBoxNS;
using Fastedit.Core.Tab;
using Microsoft.UI;
using Windows.UI;

namespace Fastedit.Core.Settings
{
    public class SettingsUpdater
    {
        private static TextControlBoxDesign textboxDesign = null;

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

            tabView.TabWidthMode = (TabViewWidthMode)AppSettings.TabViewWidthMode;
        }
        private static TextControlBoxDesign CreateTextboxDesign(FasteditDesign currentDesign)
        {
            return new TextControlBoxDesign(
                DesignHelper.CreateBackgroundBrush(ConvertHelper.ToColor(currentDesign.TextBoxBackground), currentDesign.TextboxBackgroundType),
                ConvertHelper.ToColor(currentDesign.TextColor),
                ConvertHelper.ToColor(currentDesign.SelectionColor),
                ConvertHelper.ToColor(currentDesign.CursorColor),
                ConvertHelper.ToColor(currentDesign.LineHighlighterBackground),
                ConvertHelper.ToColor(currentDesign.LineNumberColor),
                ConvertHelper.ToColor(currentDesign.LineNumberBackground),
                ConvertHelper.ToColor(currentDesign.SearchHighlightColor),
                ConvertHelper.ToColor(currentDesign.WhitespaceCharacterColor)
            );
        }

        private static void UpdateTabSettings(TabPageItem tab, TextControlBoxDesign textboxDesign, ElementTheme theme, bool setMargin = true)
        {
            //setting the design before the textbox loaded does not work, because xaml overwrites it
            if (!tab.textbox.IsLoaded)
            {
                tab.textbox.Loaded += (sender) =>
                {
                    tab.textbox.RequestedTheme = theme;
                };
            }
            else
                tab.textbox.RequestedTheme = theme;

            tab.textbox.Design = textboxDesign;

            tab.textbox.FontSize = AppSettings.FontSize;
            tab.textbox.FontFamily = new FontFamily(AppSettings.FontFamily);

            tab.textbox.ShowLineHighlighter = AppSettings.ShowLineHighlighter;
            tab.textbox.ShowLineNumbers = AppSettings.ShowLineNumbers;

            tab.textbox.EnableSyntaxHighlighting = AppSettings.SyntaxHighlighting;
            
            tab.textbox.ShowWhitespaceCharacters = tab.GetEffectiveWhitespaceSetting();

            tab.textbox.HighlightLinks = AppSettings.EnableClickableLinks;

            if (setMargin)
                tab.textbox.Margin = TabPageHelper.TabMargin;
        }

        public static void UpdateTab(TabPageItem tab, bool setMargin = true)
        {
            UpdateTabSettings(tab, textboxDesign, DesignHelper.CurrentDesign.Theme, setMargin);
        }
        public static void UpdateTabs(TabView tabView, bool setMargin = true)
        {
            UpdateTabPages(tabView, DesignHelper.CurrentDesign);
        }

        public static void UpdateTabPages(TabView tabView, FasteditDesign currentDesign)
        {
            for (int i = 0; i < tabView.TabItems.Count; i++)
            {
                if (tabView.TabItems[i] is TabPageItem tab && tab != null)
                {
                    UpdateTabSettings(tab, textboxDesign, currentDesign.Theme);
                }
                else if (SettingsTabPageHelper.IsSettingsPage(tabView.TabItems[i]))
                {
                    (tabView.TabItems[i] as TabViewItem).Background = DesignHelper.CreateBackgroundBrush(ConvertHelper.ToColor(currentDesign.TextBoxBackground), currentDesign.TextboxBackgroundType);
                }
            }

            //Load tabs or spaces
            object tag = AppSettings.UseSpacesInsteadTabs ? AppSettings.SpacesPerTab : "-1";
            TabPageHelper.TabsOrSpacesForAll(tabView, tag);
        }
        public static void SetSettingsToStatusbar(TextStatusBar statusbar, FasteditDesign design)
        {
            statusbar.Background = DesignHelper.CreateBackgroundBrush(ConvertHelper.ToColor(design.StatusbarBackgroundColor), design.StatusbarBackgroundType);
            var foreground = new SolidColorBrush(ConvertHelper.ToColor(design.StatusbarTextColor));

            statusbar.ToggleItemVisibility(foreground);

            statusbar.UpdateAll();
        }
        private static void SetMenubarAlignment(MenuBar menubar)
        {
            menubar.HorizontalAlignment = (HorizontalAlignment)AppSettings.MenubarAlignment;
        }
        public static void SetControlsVisibility(TabView tabView, MenuBar menuBar, TextStatusBar textStatusBar)
        {
            if (SettingsTabPageHelper.SettingsSelected)
            {
                SettingsTabPageHelper.HideControls();
                return;
            }

            menuBar.Visibility = ConvertHelper.BoolToVisibility(AppSettings.ShowMenubar);
            textStatusBar.IsVisible = AppSettings.ShowStatusbar;

            TabPageHelper.TabMargin.Top = GetHeightWithVisibility(menuBar);
            TabPageHelper.TabMargin.Bottom = GetHeightWithVisibility(textStatusBar);

            //Update tab margin:
            for (int i = 0; i < tabView.TabItems.Count; i++)
            {
                if (tabView.TabItems[i] is TabPageItem tab)
                {
                    tab.textbox.Margin = TabPageHelper.TabMargin;
                }
            }
        }

        public static void SetWindowBackground(BackdropWindowManager backdropManager, FasteditDesign currentDesign)
        {
            backdropManager.SetBackdrop(DesignHelper.CurrentDesign.BackgroundType, DesignHelper.CurrentDesign);
        }

        public static void SetTitlebarSettings(FasteditDesign design)
        {
            App.m_window.ExtendsContentIntoTitleBar = AppSettings.HideTitlebar;
        }

        public static void UpdateSettings(MainPage mainPage, TabView tabView, MenuBar menuBar, TextStatusBar textStatusBar, FasteditDesign currentDesign)
        {
            //Load the desing from file
            if (currentDesign == null)
                DesignHelper.LoadDesign();

            //update the design
            currentDesign = DesignHelper.CurrentDesign;
            textboxDesign = CreateTextboxDesign(currentDesign);

            ThemeHelper.CurrentTheme = currentDesign.Theme;
            //Controls
            SetControlsVisibility(tabView, menuBar, textStatusBar);

            //menubar
            SetMenubarAlignment(menuBar);
            //TabPages
            UpdateTabPages(tabView, currentDesign);

            //TabControl
            SetTabViewSettings(tabView, currentDesign);

            //MainWindow
            SetWindowBackground(App.m_window.backdropManager, currentDesign);

            SetTitlebarSettings(currentDesign);

            //Statusbar
            SetSettingsToStatusbar(textStatusBar, currentDesign);

            mainPage.RunCommandWindow.UpdateColors();

            //TabWindows (tabs as own windows):
            TabWindowHelper.UpdateSettings();
        }
    }
}
