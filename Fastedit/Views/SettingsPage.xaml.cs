using Fastedit.Core;
using Fastedit.Views;
using Fastedit.Views.SettingsPage;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Convert = Fastedit.Extensions.Convert;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit
{
    public sealed partial class SettingsPage : Page
    {
        private AppSettings appsettings = new AppSettings();
        private muxc.TabView TextTabControl;
        private MainPage mainpage = null;

        public SettingsPage()
        {
            this.InitializeComponent();
            NavView_Navigate(appsettings.GetSettingsAsString("SettingsRecentPage", "Page1"), null);
            RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsString("ThemeIndex", "0"));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ApplySettingsButton.Foreground = new SolidColorBrush(Convert.WhiteOrBlackFromColorBrightness(DefaultValues.SystemAccentColor));
            if (e.Parameter is SettingsNavigationParameter navparam)
            {
                mainpage = navparam.Mainpage;
                TextTabControl = navparam.Tabcontrol;

                if (navparam.PageToNavigateTo != "")
                    NavView_Navigate(navparam.PageToNavigateTo, null);
                else
                    NavView_Navigate(appsettings.GetSettingsAsString("SettingsRecentPage", "Page1"), null);
            }
        }

        private readonly List<(string Tag, Type Page, string HeaderContent)> _pages = new List<(string Tag, Type Page, string HeaderContent)>
        {
            ("Page1", typeof(Page1), AppSettings.GetResourceStringStatic("SettingsPage_Item_Editor/Content")), //Editor
            ("Page2", typeof(Page2), AppSettings.GetResourceStringStatic("SettingsPage_Item_App/Content")), //App
            //("Page3", typeof(Page3), "Menu"), //Menu
            ("Page4", typeof(Page4), AppSettings.GetResourceStringStatic("SettingsPage_Item_TabControl/Content")), //Tabcontrol
            ("Page5", typeof(Page5), AppSettings.GetResourceStringStatic("SettingsPage_Item_Dialogs/Content")), //Dialogs
            ("Page6", typeof(Page6), AppSettings.GetResourceStringStatic("SettingsPage_Item_Data/Content")), //Data
            ("Page7", typeof(Page7), AppSettings.GetResourceStringStatic("SettingsPage_Item_Designs/Content")), //Designs
            ("Page8", typeof(Page8), AppSettings.GetResourceStringStatic("SettingsPage_Item_Statusbar/Content")), //Statusbar
            ("KeyPage", typeof(KeyPage), AppSettings.GetResourceStringStatic("SettingsPage_Item_Shortcuts/Content")), //Shortcuts
            ("AboutPage", typeof(AboutPage), AppSettings.GetResourceStringStatic("SettingsPage_Item_About/Content")), //AboutPage
            ("PrivacyPolicy", typeof(PrivacyPoliciesPage), AppSettings.GetResourceStringStatic("SettingsPage_Item_PrivacyPolicy/Content")), //PrivacyPolicy
            ("Changelog", typeof(ChangelogPage), AppSettings.GetResourceStringStatic("SettingsPage_Item_ChangeLog/Content")), //Changelog
        };

        private void NavigationView_ItemInvoked(muxc.NavigationView sender, muxc.NavigationViewItemInvokedEventArgs args)
        {
            var navItemTag = args.InvokedItemContainer.Tag.ToString();
            appsettings.SaveSettings("SettingsRecentPage", navItemTag);

            NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
        }

        private void NavView_Navigate(string navItemTag, Windows.UI.Xaml.Media.Animation.NavigationTransitionInfo transitionInfo)
        {
            Type _page = null;

            var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            _page = item.Page;

            var preNavPageType = ContentFrame.CurrentSourcePageType;
            if (!(_page is null) && !Type.Equals(preNavPageType, _page))
            {
                if (TextTabControl != null)
                {
                    ContentFrame.Navigate(_page, new SettingsNavigationParameter
                    {
                        Mainpage = mainpage,
                        Tabcontrol = TextTabControl
                    }, transitionInfo);

                    CurrentPageHeader.Text = item.HeaderContent;
                }
            }
        }

        private async void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            if (mainpage != null)
            {
                await mainpage.SetSettings();
            }
        }
    }
}
