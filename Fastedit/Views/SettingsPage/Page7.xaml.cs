using Fastedit.Controls;
using Fastedit.ExternalData;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace Fastedit.Views.SettingsPage
{
    public sealed partial class Page7 : Page
    {
        private AppSettings appsettings = new AppSettings();
        private CustomDesigns customdesigns;
        private MainPage mainpage = null;
        private DesignGridViewItem lastSelectedItem = null;
        private string LastDesignItemContent = "";
        private DesignGridViewItem rightclickedItem = null;

        public Page7()
        {
            this.InitializeComponent();
            customdesigns = new CustomDesigns(DesignGridView, null, SettingsInfoBar);
            RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsString("ThemeIndex", "0"));
        }

        //Loading theme stuff:
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is SettingsNavigationParameter snp)
            {
                mainpage = snp.Mainpage;
            }

            await customdesigns.AddAllDesignsToGridView();
            DesignGridView.SelectedIndex = appsettings.GetSettingsAsInt("SelectedDesign", 0);
            RetriveFromSettings();
        }
        //Designs stuff:
        private void RetriveFromSettings()
        {
            if (DesignGridView.SelectedIndex < 0 || DesignGridView.SelectedIndex >= DesignGridView.Items.Count)
            {
                NoDesignSelectedMessage.IsOpen = true;
                DesignItemsScrollViewer.IsEnabled = false;
                return;
            }
            else
            {
                DesignItemsScrollViewer.IsEnabled = true;
                NoDesignSelectedMessage.IsOpen = false;
            }

            for(int i = 0; i< FontColorButtons.Children.Count; i++)
            {
                if(FontColorButtons.Children[i] is CustomizationControl cc)
                {
                    if(cc != null)
                        cc.RetrieveFromSettings();
                }
            }
            EnableAcrylicDesign_Switch.IsOn = appsettings.GetSettingsAsBool("AcrylicEnabled", true);
            UseMicaInsteadDefaultBackground.IsOn = appsettings.GetSettingsAsBool("UseMica", false);
            AppBackgroundColorButton.IsEnabled = TitlebarBackgroundColorButton.IsEnabled = EnableAcrylicDesign_Switch.IsOn;
        }
        private async Task SaveChangesToDesign()
        {
            if (DesignGridView.SelectedItem is DesignGridViewItem dgvi)
            {
                if (lastSelectedItem != dgvi)
                {
                    await customdesigns.SaveCurrentAsDesign(lastSelectedItem, false, LastDesignItemContent);
                    lastSelectedItem = dgvi;
                }
            }
        }

        //events:
        private void EnableAcrylicDesign_Switch_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("AcrylicEnabled", EnableAcrylicDesign_Switch.IsOn);
            UseMicaInsteadDefaultBackground.IsOn = !EnableAcrylicDesign_Switch.IsOn;
            AppBackgroundColorButton.IsEnabled = TitlebarBackgroundColorButton.IsEnabled = EnableAcrylicDesign_Switch.IsOn;
        }
        private void UseMicaInsteadDefaultBackground_Toggled(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("UseMica", UseMicaInsteadDefaultBackground.IsOn);
            AppBackgroundColorButton.IsEnabled = TitlebarBackgroundColorButton.IsEnabled =
                EnableAcrylicDesign_Switch.IsOn = !UseMicaInsteadDefaultBackground.IsOn;
        }
        private async void NewDesignButton_Click(object sender, RoutedEventArgs e)
        {
            await customdesigns.CreateNewDesign();
        }
        private async void ImportedDesignButton_Click(object sender, RoutedEventArgs e)
        {
            await customdesigns.ImportDesign();
        }
        private void CustomizationControl_ColorChangedEvent()
        {
            if (DesignGridView.SelectedItem is DesignGridViewItem dgvi)
            {
                if (lastSelectedItem != dgvi)
                {
                    LastDesignItemContent = customdesigns.CreateDesignContent();
                    lastSelectedItem = dgvi;
                }
            }
        }
        private void DesignGridView_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            DesignGridViewFlyout.ShowAt(
                sender as DependencyObject,
                new FlyoutShowOptions
                { 
                    Position = e.GetPosition(sender as UIElement)
                });
            rightclickedItem = (sender as Grid)?.DataContext as DesignGridViewItem;
            Debug.WriteLine(rightclickedItem != null);
        }
        private async void DesignGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await SaveChangesToDesign();

            await customdesigns.OnSelectionChange();
            if (mainpage != null)
            {
                await mainpage.SetSettings(true);
            }
            RetriveFromSettings();
        }
        private async void RenameDesign_Click(object sender, RoutedEventArgs e)
        {
            if (rightclickedItem != null)
            {
                await customdesigns.RenameDesign(rightclickedItem);
            }
        }
        private async void ExportDesign_Click(object sender, RoutedEventArgs e)
        {
            if (rightclickedItem != null)
            {
                await customdesigns.ExportDesign(rightclickedItem);
            }
        }
        private async void DeleteDesign_Click(object sender, RoutedEventArgs e)
        {
            if (rightclickedItem != null)
            {
                await customdesigns.DeleteDesign(rightclickedItem);
            }
        }
    }
}