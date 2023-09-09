using Fastedit.Storage;
using Fastedit.Tab;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
    internal class RenameFileDialog
    {
        public static async Task<bool> Show(TabPageItem tab)
        {
            var renameTextbox = new TextBox { Text = tab.DatabaseItem.FileName };
            var renameDialog = new ContentDialog
            {
                Background = DialogHelper.ContentDialogBackground(),
                Foreground = DialogHelper.ContentDialogForeground(),
                RequestedTheme = DialogHelper.DialogDesign,
                Title = "Rename: " + tab.DatabaseItem.FileName,
                Content = renameTextbox,
                PrimaryButtonText = "Rename",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
            };

            renameTextbox.Select(0, tab.DatabaseItem.FileName.LastIndexOf("."));

            var res = await renameDialog.ShowAsync();
            if (res == ContentDialogResult.Primary)
                return await RenameFileHelper.RenameFile(tab, renameTextbox.Text);
            else if (res == ContentDialogResult.Secondary)
                return true;
            return false;
        }
    }
}
