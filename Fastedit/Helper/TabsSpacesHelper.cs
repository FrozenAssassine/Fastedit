using Fastedit.Models;
using Fastedit.Core.Tab;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using TextControlBoxNS;

namespace Fastedit.Helper;

internal class TabsSpacesHelper
{
    public static void SelectRadioItemFromMenu(MenuFlyout tabsSpacesflyout, TabPageItem tabPage)
    {
        string tag = (tabPage.textbox.UseSpacesInsteadTabs ? tabPage.textbox.NumberOfSpacesForTab : -1).ToString();

        IterateItems(tabsSpacesflyout.Items, tag);
    }

    private static void IterateItems(IList<MenuFlyoutItemBase> items, string tag)
    {
        foreach (MenuFlyoutItemBase item in items)
        {
            if (item is ToggleMenuFlyoutItem radioItem)
            {
                radioItem.IsChecked = radioItem.Tag.ToString().Equals(tag);
            }
        }
    }

    public static void SelectToggleMenuItemsFromMenu(MenuFlyoutSubItem tabsSpacesflyout, TabPageItem tabPage)
    {
        if (tabPage == null || tabPage.textbox == null)
            return;

        string tag = (tabPage.textbox.UseSpacesInsteadTabs ? tabPage.textbox.NumberOfSpacesForTab : -1).ToString();
        IterateItems(tabsSpacesflyout.Items, tag);
    }

    public static void SetTabsSpaces(TabPageItem tabPage, object sender)
    {
        if (tabPage == null)
            return;

        int spaces = -1;

        if (sender is MenuFlyoutItem item)
            spaces = ConvertHelper.ToInt(item.Tag, -1);
        else if (sender is QuickAccessWindowItem qawi)
            spaces = ConvertHelper.ToInt(qawi.Tag, -1);

        tabPage.SetTabsSpaces(spaces);

    }

    public static void RewriteTabsSpaces(TabPageItem tabPage, object sender)
    {
        if (tabPage == null)
            return;

        int spaces = -1;

        if (sender is MenuFlyoutItem item)
            spaces = ConvertHelper.ToInt(item.Tag, -1);
        else if (sender is QuickAccessWindowItem qawi)
            spaces = ConvertHelper.ToInt(qawi.Tag, -1);


        tabPage.RewriteTabsSpaces(spaces);
    }
}
