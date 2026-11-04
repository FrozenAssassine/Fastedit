using Fastedit.Helper;
using Fastedit.Models;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using Fastedit.Views.SettingsPages;
using System;
using System.Collections.Generic;

namespace Fastedit.Views;

public sealed partial class SettingsPage : Page
{
    private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
    {
        ("AppPage", typeof(Settings_AppPage)),
        ("TextboxPage", typeof(Settings_DocumentPage)),
        ("TabviewPage", typeof(Settings_TabControl)),
        ("DesignPage", typeof(Settings_DesignPage)),
        ("DataPage", typeof(Settings_Data)),
        ("AboutPage", typeof(AboutPage)),
    };

    public SettingsNavigationParameter SettingsParameter { get; set; }

    public SettingsPage(SettingsNavigationParameter param)
    {
        this.InitializeComponent();

        SettingsParameter = param;
        ReloadLanguage(false);
        NavView_Navigate(SettingsParameter.Page ?? _pages[0].Tag);
        this.Loaded += (s, e) => ReloadLanguage(false);
    }
    private string GetHeaderTitle(string tag)
    {
        return tag switch
        {
            "AppPage" => Multilingual.L("SettingsPage_NavigationViewItem_1.Content"),
            "TextboxPage" => Multilingual.L("SettingsPage_NavigationViewItem_2.Content"),
            "TabviewPage" => Multilingual.L("SettingsPage_NavigationViewItem_3.Content"),
            "DesignPage" => Multilingual.L("SettingsPage_NavigationViewItem_4.Content"),
            "DataPage" => Multilingual.L("SettingsPage_NavigationViewItem_5.Content"),
            "AboutPage" => Multilingual.L("SettingsPage_NavigationViewItem_6.Content"),
            _ => tag
        };
    }

    private void NavView_Navigate(string navItemTag)
    {
        Type _page = null;
        if (navItemTag == "settings")
        {
            // _page = typeof(SettingsPage);
        }

        var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
        _page = item.Page;

        applySettingsButton.Visibility = ConvertHelper.BoolToVisibility(item.Page != typeof(AboutPage));

        var preNavPageType = navigationFrame.CurrentSourcePageType;
        if (!(_page is null) && !Type.Equals(preNavPageType, _page))
        {
            pageNameDisplay.Text = GetHeaderTitle(item.Tag);
            navigationFrame.Navigate(_page, SettingsParameter);
        }
    }

    public void ReloadLanguage(bool reloadCurrentSubPage = true)
    {
        if (navigationView?.MenuItems != null)
        {
            foreach (var navItem in navigationView.MenuItems.OfType<NavigationViewItem>())
            {
                switch (navItem.Tag?.ToString())
                {
                    case "AppPage": navItem.Content = Multilingual.L("SettingsPage_NavigationViewItem_1.Content"); break;
                    case "TextboxPage": navItem.Content = Multilingual.L("SettingsPage_NavigationViewItem_2.Content"); break;
                    case "TabviewPage": navItem.Content = Multilingual.L("SettingsPage_NavigationViewItem_3.Content"); break;
                    case "DesignPage": navItem.Content = Multilingual.L("SettingsPage_NavigationViewItem_4.Content"); break;
                    case "DataPage": navItem.Content = Multilingual.L("SettingsPage_NavigationViewItem_5.Content"); break;
                }
            }
        }
        if (navigationView?.FooterMenuItems != null)
        {
            foreach (var navItem in navigationView.FooterMenuItems.OfType<NavigationViewItem>())
            {
                if (navItem.Tag?.ToString() == "AboutPage")
                    navItem.Content = Multilingual.L("SettingsPage_NavigationViewItem_6.Content");
            }
        }

        if (applySettingsButton != null)
            applySettingsButton.Content = Multilingual.L("SettingsPage_Button_1.Content");

        if (SettingsTabPageHelper.settingsPage != null)
            SettingsTabPageHelper.settingsPage.Header = Multilingual.L("MainPage_MenuFlyoutItem_7.Text");

        var curType = navigationFrame.CurrentSourcePageType;
        if (curType != null)
        {
            var item = _pages.FirstOrDefault(p => p.Page == curType);
            if (item.Page != null)
            {
                pageNameDisplay.Text = GetHeaderTitle(item.Tag);
                if (reloadCurrentSubPage)
                {
                    navigationFrame.Content = null;
                    navigationFrame.Navigate(item.Page, SettingsParameter);
                }
            }
        }
    }

    private void NavigationView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
    {
        var navItemTag = args.InvokedItemContainer.Tag.ToString();

        NavView_Navigate(navItemTag);
    }

    private void ApplySettings_Click(object sender, RoutedEventArgs e)
    {
        SettingsParameter?.MainPage.ApplySettings();
        ReloadLanguage();

        this.ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
    }

    private void Page_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        this.ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
    }
}
