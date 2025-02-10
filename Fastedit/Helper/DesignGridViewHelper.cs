using Fastedit.Models;
using Fastedit.Settings;
using Fastedit.Tab;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
using System.IO;

namespace Fastedit.Helper;

public class DesignGridViewHelper
{
    //Manage the GridView with design items
    public static DesignGridViewItem CreateItem(FasteditDesign design, string path)
    {
        return new DesignGridViewItem
        {
            TextColor = new SolidColorBrush(ConvertHelper.ToColor(design.TextColor)),
            AppBackground = design.BackgroundType == BackgroundType.Transparent ? null : new SolidColorBrush(design.BackgroundType == BackgroundType.Mica ? ConvertHelper.GetColorFromTheme(design.Theme) : ConvertHelper.ToColor(design.BackgroundColor)),
            TabPageBackground = new SolidColorBrush(ConvertHelper.ToColor(design.SelectedTabPageHeaderBackground)),
            FileName = DesignHelper.GetFileNameFromPath(path),
            DisplayName = DesignHelper.GetDesignNameFromPath(path),
            LineNumberBackground = new SolidColorBrush(ConvertHelper.ToColor(design.LineNumberBackground)),
            LineNumberColor = new SolidColorBrush(ConvertHelper.ToColor(design.LineNumberColor)),
            TextBoxBackground = new SolidColorBrush(ConvertHelper.ToColor(design.TextBoxBackground)),
            Width = 200,
            Height = 130
        };
    }
    public static void LoadItems(GridView designGridView)
    {
        int index = 0;
        int toSelectIndex = 0;
        string currentDesign = AppSettings.CurrentDesign;

        foreach(var designFile in DesignHelper.GetDesignsFilesFromFolder())
        {
            index++;
            var design = DesignHelper.GetDesignFromFile(designFile);
            if (design == null)
                continue;

            string fileName = DesignHelper.GetFileNameFromPath(designFile);
            designGridView.Items.Add(CreateItem(design, fileName));

            if (Path.GetFileName(currentDesign).Equals(fileName))
            {
                toSelectIndex = index - 1;
            }
        }
        designGridView.SelectedIndex = toSelectIndex;
    }
    public static void GridViewClick(ItemClickEventArgs e)
    {
        if (e.ClickedItem is DesignGridViewItem item && item != null)
        {
            AppSettings.CurrentDesign = item.FileName;
            TabPageHelper.mainPage.ApplySettings();
        }
    }
    public static void UpdateItems(GridView designGridView)
    {
        designGridView.Items.Clear();
        LoadItems(designGridView);
    }
}
