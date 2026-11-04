using Fastedit.Views.DialogPages;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using TextControlBoxNS;

namespace Fastedit.Dialogs;

internal class MixedLineEndingsWarningDialog
{
    public static async Task<(bool confirmed, LineEnding lineEnding)> Show()
    {
        var dialogPage = new MixedLineEndingWarningDialogPage();

        var SaveDialog = new ContentDialog
        {
            XamlRoot = App.m_window.Content.XamlRoot,
            Background = DialogHelper.ContentDialogBackground(),
            Foreground = DialogHelper.ContentDialogForeground(),
            RequestedTheme = DialogHelper.DialogDesign,
            Title = Multilingual.L("T_Warning"),
            Content = dialogPage,
            PrimaryButtonText = Multilingual.L("T_Apply"),
            CloseButtonText = Multilingual.L("T_Cancel"),
            DefaultButton = ContentDialogButton.Primary,
        };

        var dlgRes = await SaveDialog.ShowAsync();

        return (dlgRes == ContentDialogResult.Primary, dialogPage.SelectedLineEnding);
    }
}
