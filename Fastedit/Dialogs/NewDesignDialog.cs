using Fastedit.Helper;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace Fastedit.Dialogs;

internal class NewDesignDialog
{
    public static async Task<string> Show()
    {
        TextBox designName_Textbox = new TextBox
        {
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch
        };
        var dialog = new ContentDialog
        {
            Background = DialogHelper.ContentDialogBackground(),
            Foreground = DialogHelper.ContentDialogForeground(),
            RequestedTheme = DialogHelper.DialogDesign,
            Title = "New Design",
            Content = designName_Textbox,
            PrimaryButtonText = "Done",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = App.m_window.Content.XamlRoot
        };
        var res = await dialog.ShowAsync();
        if(res == ContentDialogResult.Primary && designName_Textbox.Text.Length > 0)
        {
            if (DesignHelper.IsValidDesignName(designName_Textbox.Text))
            {
                return designName_Textbox.Text + ".json";
            }
            InfoMessages.InvalidDesignName();
        }
        return null;
    }
}
