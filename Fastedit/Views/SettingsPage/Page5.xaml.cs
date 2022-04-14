using Fastedit.Core;
using Fastedit.Extensions;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Fastedit.Views.SettingsPage
{
    public sealed partial class Page5 : Page
    {
        private AppSettings appsettings = new AppSettings();

        public Page5()
        {
            this.InitializeComponent();
            RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsString("ThemeIndex", "0"));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SearchGoToLineDialogAlignment.SelectedIndex = appsettings.GetSettingsAsBool("SearchPanelCenterAlign", true) ? 1 : 0;

            //Go to line Dialog:
            HideGoToLineDialogAfterGoingToLineButton.IsChecked = appsettings.GetSettingsAsBool("HideGoToLineDialogAfterEntering", true);
            SelectLineAfterGoingToItButton.IsChecked = appsettings.GetSettingsAsBool("SelectLineAfterGoingToIt", true);
        }

        //Go to line dialog
        private void HideGoToDialogAfterEntering_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox)
            {
                CheckBox cb = sender as CheckBox;
                appsettings.SaveSettings("HideGoToLineDialogAfterEntering", cb.IsChecked);
            }
        }
        private void SelectLineAfterGoingToIt_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox)
            {
                CheckBox cb = sender as CheckBox;
                appsettings.SaveSettings("SelectLineAfterGoingToIt", cb.IsChecked);
            }
        }

        private void SearchGoToLineDialogAlignment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SearchGoToLineDialogAlignment.SelectedIndex > -1)
            {
                appsettings.SaveSettings("SearchPanelCenterAlign", SearchGoToLineDialogAlignment.SelectedIndex == 1 ? true : false);
            }
        }
    }
}