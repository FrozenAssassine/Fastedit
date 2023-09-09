using Fastedit.Storage;
using Fastedit.Tab;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
    public class AskSaveDialog
    {
        public static async Task<bool> Show(TabPageItem tab, XamlRoot root = null)
        {
            var SaveDialog = new ContentDialog
            {
                XamlRoot = root,
                Background = DialogHelper.ContentDialogBackground(),
                Foreground = DialogHelper.ContentDialogForeground(),
                RequestedTheme = DialogHelper.DialogDesign,
                Title = "Save file?",
                Content = "Would you like to save the file " + tab.DatabaseItem.FileName + "?",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Don't save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
            };
            var res = await SaveDialog.ShowAsync();
            if (res == ContentDialogResult.Primary)
                return await SaveFileHelper.Save(tab);
            else return res == ContentDialogResult.Secondary;
        }
    }
}
