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
            content.AppendLine(Multilingual.L("T_Extension") + fileExtension + " (" + extension.ExtensionName + ")"); ;

        //only if the tab is based on a file
        if (!tab.DatabaseItem.WasNeverSaved)
        {
            //Maybe not use storage file here?
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(tab.DatabaseItem.FilePath);
                BasicProperties fileProperties = await file.GetBasicPropertiesAsync();

                content.AppendLine(Multilingual.L("T_Path") + tab.DatabaseItem.FilePath);
                content.AppendLine(Multilingual.L("T_Created") + file.DateCreated.ToString("G"));
                content.AppendLine(Multilingual.L("T_LastModified") + fileProperties.DateModified.ToString("G"));
                content.AppendLine(Multilingual.L("T_Size") + SizeCalculationHelper.SplitSize(fileProperties.Size));
            }
            catch (FileNotFoundException) {  }
        }

        if (tab.textbox.SyntaxHighlighting != null)
            content.AppendLine(Multilingual.L("T_Language") + tab.textbox.SyntaxHighlighting.Name);

        content.AppendLine(Multilingual.L("T_Words") + tab.textbox.WordCount());
        content.AppendLine(Multilingual.L("T_Lines") + tab.textbox.NumberOfLines);
        content.AppendLine(Multilingual.L("T_Characters") + tab.textbox.CharacterCount());
        content.AppendLine(Multilingual.L("T_Encoding_2") + EncodingHelper.GetEncodingName(tab.Encoding));
        content.AppendLine(Multilingual.L("T_LineEnding") + tab.LineEnding.ToString());

        var dialog = new ContentDialog
        {
            Background = DialogHelper.ContentDialogBackground(),
            Foreground = DialogHelper.ContentDialogForeground(),
            RequestedTheme = DialogHelper.DialogDesign,
            Title = Multilingual.L("T_Info_2") + tab.DatabaseItem.FileName,
            Content = new TextBlock { MaxWidth = 500, Text = content.ToString(), IsTextSelectionEnabled = true, TextWrapping=Microsoft.UI.Xaml.TextWrapping.Wrap },
            CloseButtonText = Multilingual.L("T_Ok"),
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = App.m_window.Content.XamlRoot,
            
        };
        await dialog.ShowAsync();
    }
}
