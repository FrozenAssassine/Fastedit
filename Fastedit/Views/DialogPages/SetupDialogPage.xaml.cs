using Microsoft.UI.Xaml.Controls;

namespace Fastedit.Views.DialogPages;
public sealed partial class SetupDialogPage : Page
{
    public SetupDialogPage()
    {
        this.InitializeComponent();
    }

    private void designGridView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        designGridView.LoadItems();
    }
}
