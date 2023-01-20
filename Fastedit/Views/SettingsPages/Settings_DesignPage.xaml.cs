using Fastedit.Helper;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
    }
}
