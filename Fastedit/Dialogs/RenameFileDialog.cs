using Fastedit.Storage;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Fastedit.Core.Tab;
using Fastedit.Extensions;

namespace Fastedit.Dialogs;

internal class RenameFileDialog
{
    public static async Task<bool> Show(TabPageItem tab, XamlRoot root = null)
    {
        if (tab == null || tab.textbox == null)
            return false;

        var renameTextbox = new TextBox { Text = tab.DatabaseItem.FileName };

        var renameDialog = new ContentDialog
        {
            XamlRoot = root ?? App.m_window.Content.XamlRoot,
            Background = DialogHelper.ContentDialogBackground(),
            Foreground = DialogHelper.ContentDialogForeground(),
            RequestedTheme = DialogHelper.DialogDesign,
            Title = "Rename " + tab.DatabaseItem.FileName,
            Content = renameTextbox,
            PrimaryButtonText = "Rename",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
        };

        renameTextbox.TextChanged += (sender, e) =>
        {
            renameDialog.IsPrimaryButtonEnabled = renameTextbox.Text.Length > 0 && !renameTextbox.Text.ContainsInvalidPathChars();
        };

        int dotIndex = tab.DatabaseItem.FileName.LastIndexOf(".");
        if (dotIndex > 0)
            renameTextbox.Select(0, dotIndex);
        else
            renameTextbox.SelectAll();

        var res = await renameDialog.ShowAsync();
        if (res == ContentDialogResult.Primary)
            return RenameFileHelper.RenameFile(tab, renameTextbox.Text);
        else if (res == ContentDialogResult.Secondary)
            return true;
        return false;
    }
}
