using Fastedit.Helper;
using Fastedit.Views.SettingsPages;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Fastedit.Views
{
    public sealed partial class SettingsPage : Page
    {
        SettingsNavigationParameter SettingsParameter = null;

        private readonly List<(string Tag, Type Page, string HeaderContent)> _pages = new List<(string Tag, Type Page, string HeaderContent)>
        {
            ("AppPage", typeof(Settings_AppPage), "App"),
            ("TextboxPage", typeof(Settings_DocumentPage), "Textbox"),
            ("TabviewPage", typeof(Settings_TabControl), "Tabview"),
            ("DesignPage", typeof(Settings_DesignPage), "Design"),
            ("DataPage", typeof(Settings_Data), "Data"),
            ("AboutPage", typeof(AboutPage), "About"),
        };

        public SettingsPage(SettingsNavigationParameter param)
        {
            this.InitializeComponent();
            SettingsParameter = param;
            NavView_Navigate(SettingsParameter.Page ?? _pages[0].Tag);
        }
        private void NavView_Navigate(string navItemTag)
        {
            Type _page = null;

            var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            _page = item.Page;

            applySettingsButton.Visibility = ConvertHelper.BoolToVisibility(item.Page != typeof(AboutPage));

            var preNavPageType = navigationFrame.CurrentSourcePageType;
            if (!(_page is null) && !Type.Equals(preNavPageType, _page))
            {
                pageNameDisplay.Text = item.HeaderContent;
                navigationFrame.Navigate(_page, SettingsParameter);
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
        }
    }
}
