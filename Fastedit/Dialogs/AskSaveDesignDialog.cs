using Fastedit.Helper;
using Fastedit.Storage;
using Fastedit.Tab;
using Fastedit.Views;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
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
                Title = "Save design?",
                Content = "Would you like to save the changes on " + Path.GetFileNameWithoutExtension(editor.CurrentDesignName) + "?",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Don't save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
            };
            return await SaveDialog.ShowAsync();
        }
    }
}
