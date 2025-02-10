using Fastedit.Dialogs;
using Fastedit.Helper;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using System.IO;
using Fastedit.Settings;
using Fastedit.Tab;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Fastedit.Views.SettingsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings_DesignPage : Page
    {
        public Settings_DesignPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DesignGridViewHelper.LoadItems(designGridView);
            base.OnNavigatedTo(e);
        }
        private void BasicGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            DesignGridViewHelper.GridViewClick(e);
        }

        private void UpdateDesigns_Click(object sender, RoutedEventArgs e)
        {
            DesignGridViewHelper.UpdateItems(designGridView);
        }

        private async void ExportDesign_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag != null)
            {
                if (!await DesignHelper.ExportDesign(item.Tag.ToString()))
                    InfoMessages.ExportDesignError();
            }
        }

        private async void ImportDesign_Click(object sender, RoutedEventArgs e)
        {
            if (await DesignHelper.ImportDesign())
                UpdateDesigns_Click(null, null);
            else
                InfoMessages.ImportDesignError();
        }

        private async void DeleteDesign_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag != null)
            {
                if (await DesignHelper.DeleteDesign(item.Tag.ToString(), designGridView))
                    UpdateDesigns_Click(null, null);
                else
                    InfoMessages.DeleteDesignError();
            }
        }

        private void EditDesign_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                DesignWindowHelper.EditDesign(ConvertHelper.ToString(item.Tag));
            }
        }

        private async void NewDesign_Click(object sender, RoutedEventArgs e)
        {
            string designName = await NewDesignDialog.Show();
            if (designName == null)
                return;

            var file = DesignHelper.CreateDesign(designName);

            DesignGridViewHelper.UpdateItems(designGridView);
            DesignWindowHelper.EditDesign(Path.GetFileName(file));
        }

        private void LoadDefaultDesigns_Click(object sender, RoutedEventArgs e)
        {
            foreach (var file in Directory.GetFiles(DefaultValues.DesignPath))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    InfoMessages.DeleteDesignError(file);
                }
            }

            DesignHelper.CopyDefaultDesigns(true);

            DesignGridViewHelper.UpdateItems(designGridView);
        }

        private void EditDesignJson_Click(object sender, RoutedEventArgs e)
        {
            string fileName = (sender as MenuFlyoutItem).Tag.ToString();
            TabPageHelper.OpenAndShowFile(TabPageHelper.mainPage.tabView, Path.Combine(DefaultValues.DesignPath, fileName), true);
        }
    }
}
