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
            Title = "Warning",
            Content = dialogPage,
            PrimaryButtonText = "Apply",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
        };

        var dlgRes = await SaveDialog.ShowAsync();

        return (dlgRes == ContentDialogResult.Primary, dialogPage.SelectedLineEnding);
    }
}
