﻿using Fastedit.Helper;
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
            string fileExtension = Path.GetExtension(tab.DatabaseItem.FilePath.Length > 0 ? tab.DatabaseItem.FilePath : tab.DatabaseItem.FileName);
            var extension = FileExtensions.FindByExtension(fileExtension);
            if (extension != null)
                content.AppendLine("Extension: " + fileExtension + " (" + extension.ExtensionName + ")"); ;

            //only if the tab is based on a file
            var res = await FutureAccessListHelper.GetFileAsync(tab.DatabaseItem.FileToken);
            if (res.success)
            {
                BasicProperties fileProperties = await res.file.GetBasicPropertiesAsync();

                content.AppendLine("Path: " + tab.DatabaseItem.FilePath);
                content.AppendLine("Created: " + res.file.DateCreated);
                content.AppendLine("Last modified: " + fileProperties.DateModified);
                content.AppendLine("Size: " + SizeCalculationHelper.SplitSize(fileProperties.Size));
            }

            if (tab.textbox.CodeLanguage != null)
                content.AppendLine("Code language: " + tab.textbox.CodeLanguage.Name);

            content.AppendLine("Words: " + tab.CountWords());
            content.AppendLine("Lines: " + tab.textbox.NumberOfLines);
            content.AppendLine("Characters: " + tab.textbox.CharacterCount);
            content.AppendLine("Encoding: " + EncodingHelper.GetEncodingName(tab.Encoding));

            string FileName = tab.DatabaseItem.FileToken.Length == 0 ? tab.DatabaseItem.FileName : Path.GetFileName(tab.DatabaseItem.FilePath);

            var dialog = new ContentDialog
            {
                Background = DialogHelper.ContentDialogBackground(),
                Foreground = DialogHelper.ContentDialogForeground(),
                RequestedTheme = DialogHelper.DialogDesign,
                Title = "Info " + FileName,
                Content = new TextBlock { Text = content.ToString(), IsTextSelectionEnabled = true },
                CloseButtonText = "Ok",
                DefaultButton = ContentDialogButton.Close,
            };
            await dialog.ShowAsync();
        }
    }
}
