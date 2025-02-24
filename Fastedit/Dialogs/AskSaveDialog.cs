using Fastedit.Storage;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Fastedit.Core.Tab;

namespace Fastedit.Dialogs
{
    public class AskSaveDialog
    {
        public static async Task<bool> Show(TabPageItem tab, XamlRoot root = null)
        {
            var SaveDialog = new ContentDialog
            {
                Background = DialogHelper.ContentDialogBackground(),
                Foreground = DialogHelper.ContentDialogForeground(),
                RequestedTheme = DialogHelper.DialogDesign,
                Title = "Save File?",
                Content = "Would you like to save the file " + tab.DatabaseItem.FileName + "?",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Don't save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = root ?? App.m_window.Content.XamlRoot,
            };
            var res = await SaveDialog.ShowAsync();
            if (res == ContentDialogResult.Primary)
                return await SaveFileHelper.Save(tab);
            else return res == ContentDialogResult.Secondary;
        }
    }
}
