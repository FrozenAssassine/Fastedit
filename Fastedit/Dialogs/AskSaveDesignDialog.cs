using Fastedit.Views;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace Fastedit.Dialogs;

public class AskSaveDesignDialog
{
    public static async Task<ContentDialogResult> Show(DesignEditor editor)
    {
        var SaveDialog = new ContentDialog
        {
            XamlRoot = editor.XamlRoot,
            Background = DialogHelper.ContentDialogBackground(),
            Foreground = DialogHelper.ContentDialogForeground(),
            RequestedTheme = DialogHelper.DialogDesign,
            Title = Multilingual.L("T_SaveDesignTitle"),
            Content = Multilingual.L("T_SaveDesignContent", Path.GetFileNameWithoutExtension(editor.CurrentDesignName)),
            PrimaryButtonText = Multilingual.L("T_Save"),
            SecondaryButtonText = Multilingual.L("T_DonTSave"),
            CloseButtonText = Multilingual.L("T_Cancel"),
            DefaultButton = ContentDialogButton.Primary,
        };
        return await SaveDialog.ShowAsync();
    }
}
