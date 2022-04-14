using Fastedit.Core;
using Fastedit.Extensions;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Fastedit.Views.SettingsPage
{
    public class TabPageIcon
    {
        public string Glyph { get; set; }
        public string Name { get; set; }
    }

    public sealed partial class Page4 : Page
    {
        private AppSettings appsettings = new AppSettings();


        public Page4()
        {
            this.InitializeComponent();
            RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsString("ThemeIndex", "0"));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            TabModeCombobox.SelectedIndex = appsettings.GetSettingsAsInt("TabSizeModeIndex", DefaultValues.defaultTabSizeMode);
            ShowUnderTabControlLine.IsOn = appsettings.GetSettingsAsBool("ShowUnderTabLine", true);
            NewTabTitleName.Text = appsettings.GetSettingsAsString("NewTabTitleName", DefaultValues.NewDocName);

            TabIconCombobox.SelectedIndex = IndexFromTabIcon(appsettings.GetSettingsAsString("TabIconId", DefaultValues.DefaultTabIconId));

            if (appsettings.GetSettingsAsBool("LoadRecentTabs", true) == true)
            {
                CreateNewTabButton.IsChecked = false;
                RecoverOldTabsButton.IsChecked = true;
            }
            else
            {
                CreateNewTabButton.IsChecked = true;
                RecoverOldTabsButton.IsChecked = false;
            }
        }

        private int IndexFromTabIcon(string TabIcon)
        {
            return TabIcons.IndexOf(TabIcon);
        }

        public List<string> TabIcons = new List<string>
        {
            "\uE8A5", //"Document"
            "\uE7C3", //Page
            "\uF56E", //PageMirrored
            "\uE729", //PageSolid
            "\uE70B", //QuickNote
            "\uE70F", //Edit
            "\uE719", //Shop
            "\uE71D", //AllApps
            "\uE723", //Attach
            "\uE728", //FavoriteList
            "\uE736", //ReadingMode
            "\uE73A", //CheckboxComposite
            "\uE74C", //OEM
            "\uE74E", //Save
            "\uE756", //CommandPrompt
            "\uE75A", //SIPUndock
            "\uE790", //Color
            "\uE80F", //Home
            "\uE811", //ParkingLocation
            "\uE81E", //MapLayers
            "\uE838", //FolderOpen
            "\uE875", //MobSIMLock
            "\uE876", //MobSIMMissing
            "\uE879", //RoamingDomestic
            "\uE8A1", //PreviewLink
            "\uE8B7", //Folder
            "\uE8E9", //FontSize
            "\uE95E" //Health
            //"\u", //

        };


        //TabSizeMode --> Page4
        private void TabModeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            appsettings.SaveSettings("TabSizeModeIndex", TabModeCombobox.SelectedIndex);
        }
        //Recover or Create new tab on start
        private void RecoverOldTabsButton_Clicked(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("LoadRecentTabs", true);
        }
        private void CreateNewTabButton_Clicked(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("LoadRecentTabs", false);
        }

        private void NewTabTitleName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (StringBuilder.IsValidFilename(NewTabTitleName.Text))
            {
                appsettings.SaveSettings("NewTabTitleName", NewTabTitleName.Text);
                NewTabTitleName.BorderBrush = DefaultValues.CorrectInput_Color;
            }
            else
            {
                NewTabTitleName.BorderBrush = DefaultValues.WrongInput_Color;
            }
        }

        private void TabIconCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabIconCombobox.SelectedIndex != -1)
            {
                appsettings.SaveSettings("TabIconId", TabIcons[TabIconCombobox.SelectedIndex]);
            }
        }

        private void ShowUnderTabControlLine_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("ShowUnderTabLine", ShowUnderTabControlLine.IsOn.ToString());
        }
    }
}
