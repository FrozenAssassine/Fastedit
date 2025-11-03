using Fastedit.Core.Settings;
using Fastedit.Core.Tab;
using Fastedit.Dialogs;
using Fastedit.Helper;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.IO;

namespace Fastedit.Controls;

public sealed partial class DesignGridView : UserControl
{
    public DesignGridView()
    {
        this.InitializeComponent();
    }

    public void LoadItems()
    {
        DesignGridViewHelper.LoadItems(designGridView);
    }

    public void UpdateDesigns()
    {
        DesignGridViewHelper.UpdateItems(designGridView);
    }

    public bool DeleteDesign(string name)
    {
        return DesignHelper.DeleteDesign(name, designGridView);
    }

    private void BasicGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        DesignGridViewHelper.GridViewClick(e);
    }


    private void EditDesign_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item)
        {
            DesignWindowHelper.EditDesign(ConvertHelper.ToString(item.Tag));
        }
    }


    private async void EditDesignJson_Click(object sender, RoutedEventArgs e)
    {
        string fileName = (sender as MenuFlyoutItem).Tag.ToString();
        await TabPageHelper.OpenAndShowFile(TabPageHelper.mainPage.tabView, Path.Combine(DefaultValues.DesignPath, fileName), true);
    }

    private void DuplicateDesign_Click(object sender, RoutedEventArgs e)
    {
        string fileName = (sender as MenuFlyoutItem).Tag.ToString();
        var duplicateFileName = DesignHelper.DuplicateDesign(fileName);
        if (duplicateFileName != null)
            this.UpdateDesigns();
    }

    private void DeleteDesign_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag != null)
        {
            if (this.DeleteDesign(item.Tag.ToString()))
                this.UpdateDesigns();
            else
                InfoMessages.DeleteDesignError();
        }
    }

    private async void ExportDesign_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag != null)
        {
            if (!await DesignHelper.ExportDesign(item.Tag.ToString()))
                InfoMessages.ExportDesignError();
        }
    }

}
