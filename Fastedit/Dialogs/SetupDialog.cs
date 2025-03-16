using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Fastedit.Views.DialogPages;

namespace Fastedit.Dialogs;

internal class SetupDialog
{
    public static async Task<string> Show()
    {
        var dialog = new ContentDialog
        {
            Background = DialogHelper.ContentDialogBackground(),
            Foreground = DialogHelper.ContentDialogForeground(),
            RequestedTheme = DialogHelper.DialogDesign,
            Title = "Setup",
            Content = new SetupDialogPage(),
            PrimaryButtonText = "Done",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = App.m_window.Content.XamlRoot,
        };
        var res = await dialog.ShowAsync();
        return null;
    }
}
