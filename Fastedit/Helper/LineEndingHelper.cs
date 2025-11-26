using Fastedit.Core.Tab;
using Fastedit.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TextControlBoxNS;

namespace Fastedit.Helper;

internal class LineEndingHelper
{
    public static string GetLineEndingString(LineEnding lineEnding)
    {
        return lineEnding switch
        {
            LineEnding.CRLF => "\r\n",
            LineEnding.LF => "\n",
            LineEnding.CR => "\r",
            _ => Environment.NewLine
        };
    }

    public static LineEnding[] GetLineEndings()
    {
        return (LineEnding[])Enum.GetValues(typeof(LineEnding));
    }

    private static IEnumerable<MenuFlyoutItem> MakeItems(Action<LineEnding> lineEndingSelected)
    {
        foreach (var lineEnding in GetLineEndings())
        {
            var item = new MenuFlyoutItem { Text = lineEnding.ToString(), Tag = lineEnding };
            item.Click += (sender, e) =>
            {
                lineEndingSelected?.Invoke(lineEnding);
            };

            yield return item;
        }
    }
    private static IEnumerable<RadioMenuFlyoutItem> MakeRadioItems(Action<LineEnding> lineEndingSelected)
    {
        foreach (var lineEnding in GetLineEndings())
        {
            var item = new RadioMenuFlyoutItem { Text = lineEnding.ToString(), Tag = lineEnding };
            item.Click += (sender, e) =>
            {
                lineEndingSelected?.Invoke(lineEnding);
            };

            yield return item;
        }
    }

    private static IEnumerable<QuickAccessWindowItem> MakeItemsQuickAccess(Action<LineEnding> lineEndingSelected)
    {
        foreach (var lineEnding in GetLineEndings())
        {
            var item = new QuickAccessWindowItem { Command = lineEnding.ToString() };
            item.RunCommandWindowItemClicked += (sender, e) =>
            {
                lineEndingSelected?.Invoke(lineEnding);
            };

            yield return item;
        }
    }

    public static void MakeAndAddLineEndingItems(QuickAccessWindowSubItem quickAccessItem, Action<LineEnding> lineEndingSelected)
    {
        foreach (var item in MakeItemsQuickAccess(lineEndingSelected))
            quickAccessItem.Items.Add(item);
    }


    public static void MakeAndAddLineEndingItems(MenuFlyout menuFlyout, Action<LineEnding> lineEndingSelected)
    {
        foreach (var item in MakeItems(lineEndingSelected))
            menuFlyout.Items.Add(item);
    }

    public static void MakeAndAddLineEndingItems(MenuFlyoutSubItem subItem, Action<LineEnding> lineEndingSelected)
    {
        foreach (var item in MakeRadioItems(lineEndingSelected))
            subItem.Items.Add(item);
    }

    public static void SelectItems(IList<MenuFlyoutItemBase> items, TabPageItem tabPage)
    {
        if (tabPage == null || tabPage.textbox == null)
            return;

        foreach (var item in items)
        {
            if(item is RadioMenuFlyoutItem radioItem && 
               radioItem.Tag is LineEnding lineEnding)
            {
                radioItem.IsChecked = tabPage.LineEnding == lineEnding;
                return;
            }
        }
    }
}
