using Fastedit.Dialogs;
using Fastedit.Helper;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using System.IO;
using Fastedit.Core.Settings;
using Fastedit.Core.Tab;
using Fastedit.Controls;

namespace Fastedit.Views.SettingsPages;

public sealed partial class Settings_DesignPage : Page
{
    public Settings_DesignPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        designGridView.LoadItems();
        base.OnNavigatedTo(e);
    }


    private void UpdateDesigns_Click(object sender, RoutedEventArgs e)
    {
        designGridView.UpdateDesigns();
    }

    private async void ImportDesign_Click(object sender, RoutedEventArgs e)
    {
        if (await DesignHelper.ImportDesign())
            UpdateDesigns_Click(null, null);
        else
            InfoMessages.ImportDesignError();
    }


    private async void NewDesign_Click(object sender, RoutedEventArgs e)
    {
        string designName = await NewDesignDialog.Show();
        if (designName == null)
            return;

        var file = DesignHelper.CreateDesign(designName);

        designGridView.UpdateDesigns();
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

        designGridView.UpdateDesigns();
    }

}
