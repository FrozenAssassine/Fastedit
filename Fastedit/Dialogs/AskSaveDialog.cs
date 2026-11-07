using Fastedit.Storage;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Fastedit.Core.Tab;

namespace Fastedit.Dialogs;

public class AskSaveDialog
{
    public static async Task<bool> Show(TabPageItem tab, XamlRoot root = null)
    {
        var SaveDialog = new ContentDialog
        {
            Background = DialogHelper.ContentDialogBackground(),
            Foreground = DialogHelper.ContentDialogForeground(),
            RequestedTheme = DialogHelper.DialogDesign,
            Title = Multilingual.L("T_SaveFileTitle"),
            Content = Multilingual.L("T_SaveFileContent", tab.DatabaseItem.FileName),
            PrimaryButtonText = Multilingual.L("T_Save"),
            SecondaryButtonText = Multilingual.L("T_DonTSave"),
            CloseButtonText = Multilingual.L("T_Cancel"),
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = root ?? App.m_window.Content.XamlRoot,
        };
        var res = await SaveDialog.ShowAsync();
        if (res == ContentDialogResult.Primary)
            return await SaveFileHelper.Save(tab);
        else return res == ContentDialogResult.Secondary;
    }
}
