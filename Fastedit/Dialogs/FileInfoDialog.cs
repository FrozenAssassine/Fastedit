using Fastedit.Helper;
using Fastedit.Storage;
using Fastedit.Tab;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
    public class FileInfoDialog
    {
        public static async Task Show(TabPageItem tab)
        {
            if (tab == null)
                return;

            StringBuilder content = new StringBuilder();

            //File extension
            var extension = Path.GetExtension(tab.DatabaseItem.FileName);
            var fileExtension = FileExtensions.FindByExtension(extension);
            if (fileExtension != null)
                content.AppendLine("Extension: " + extension + " (" + fileExtension.ExtensionName + ")"); ;

            //only if the tab is based on a file
            if (tab.DatabaseItem.FileToken.Length > 0)
            {
                try
                {
                    var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(tab.DatabaseItem.FileToken);

                    BasicProperties fileProperties = await file.GetBasicPropertiesAsync();
                    //calculate the filesize and extension

                    content.AppendLine("Path: " + tab.DatabaseItem.FilePath);
                    content.AppendLine("Created: " + file.DateCreated);
                    content.AppendLine("Last modified: " + fileProperties.DateModified);
                    content.AppendLine("Size: " + SizeCalculationHelper.SplitSize(fileProperties.Size));

                }
                catch (FileNotFoundException) { }
            }

            if (tab.textbox.CodeLanguage != null)
                content.AppendLine("Code language: " + tab.textbox.CodeLanguage.Name);

            content.AppendLine("Words: " + tab.CountWords());
            content.AppendLine("Lines: " + tab.textbox.NumberOfLines);
            content.AppendLine("Characters: " + tab.textbox.CharacterCount);

            var dialog = new ContentDialog
            {
                Background = DialogHelper.ContentDialogBackground(),
                Foreground = DialogHelper.ContentDialogForeground(),
                RequestedTheme = DialogHelper.DialogDesign,
                Title = "Info " + tab.DatabaseItem.FileName,
                Content = content.ToString(),
                CloseButtonText = "Ok",
                DefaultButton = ContentDialogButton.Close,
            };
            await dialog.ShowAsync();
        }
    }
}
