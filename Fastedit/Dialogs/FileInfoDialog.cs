using Fastedit.Helper;
using Fastedit.Storage;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Fastedit.Core.Tab;

namespace Fastedit.Dialogs;

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
        if (!tab.DatabaseItem.WasNeverSaved)
        {
            //Maybe not use storage file here?
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(tab.DatabaseItem.FilePath);
                BasicProperties fileProperties = await file.GetBasicPropertiesAsync();

                content.AppendLine("Path: " + tab.DatabaseItem.FilePath);
                content.AppendLine("Created: " + file.DateCreated.ToString("G"));
                content.AppendLine("Last Modified: " + fileProperties.DateModified.ToString("G"));
                content.AppendLine("Size: " + SizeCalculationHelper.SplitSize(fileProperties.Size));
            }
            catch (FileNotFoundException) {  }
        }

        if (tab.textbox.SyntaxHighlighting != null)
            content.AppendLine("Language: " + tab.textbox.SyntaxHighlighting.Name);

        content.AppendLine("Words: " + tab.textbox.WordCount());
        content.AppendLine("Lines: " + tab.textbox.NumberOfLines);
        content.AppendLine("Characters: " + tab.textbox.CharacterCount());
        content.AppendLine("Encoding: " + EncodingHelper.GetEncodingName(tab.Encoding));

        var dialog = new ContentDialog
        {
            Background = DialogHelper.ContentDialogBackground(),
            Foreground = DialogHelper.ContentDialogForeground(),
            RequestedTheme = DialogHelper.DialogDesign,
            Title = "Info " + tab.DatabaseItem.FileName,
            Content = new TextBlock { Text = content.ToString(), IsTextSelectionEnabled = true },
            CloseButtonText = "Ok",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = App.m_window.Content.XamlRoot
        };
        await dialog.ShowAsync();
    }
}
