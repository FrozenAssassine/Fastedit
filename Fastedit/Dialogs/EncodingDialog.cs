using Fastedit.Helper;
using Fastedit.Storage;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Fastedit.Core.Tab;
using Fastedit.Core.Storage;

namespace Fastedit.Dialogs;

public class EncodingDialog
{
    private static ComboBox encodingCombobox;

    public static async Task Show(TabPageItem tab)
    {
        if (tab == null)
            return;

        if (encodingCombobox == null)
        {
            encodingCombobox = new ComboBox
            {
                Header = Multilingual.L("T_Encoding"),
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                Width=200,
            };
            encodingCombobox.ItemsSource = EncodingHelper.AllEncodingNames;
        }

        encodingCombobox.Header = Multilingual.L("T_Encoding");
        encodingCombobox.SelectedIndex = EncodingHelper.GetIndexByEncoding(tab.Encoding);
        var dialog = new ContentDialog
        {
            Background = DialogHelper.ContentDialogBackground(),
            Foreground = DialogHelper.ContentDialogForeground(),
            RequestedTheme = DialogHelper.DialogDesign,
            Title = Multilingual.L("T_EncodingTitle"),
            Content = encodingCombobox,
            PrimaryButtonText = Multilingual.L("T_Done"),
            SecondaryButtonText = tab.DatabaseItem.WasNeverSaved ? "" : Multilingual.L("T_Reopen"),
            CloseButtonText = Multilingual.L("T_Cancel"),
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = App.m_window.Content.XamlRoot
        };
        var res = await dialog.ShowAsync();
        dialog.Content = null;

        if (res == ContentDialogResult.Primary)
        {
            var newEncoding = EncodingHelper.GetEncodingByIndex(encodingCombobox.SelectedIndex);
            if (!newEncoding.Equals(tab.Encoding))
            {
                tab.Encoding = newEncoding;
                //the file has to be saved again to apply the new encoding
                TabPageHelper.UpdateSaveStatus(tab, true);
            }
        }
        else if (res == ContentDialogResult.Secondary)
        {
            if (tab.DatabaseItem.IsModified)
            {
                var confirmDialog = new ContentDialog
                {
                    Background = DialogHelper.ContentDialogBackground(),
                    Foreground = DialogHelper.ContentDialogForeground(),
                    RequestedTheme = DialogHelper.DialogDesign,
                    Title = Multilingual.L("T_Warning"),
                    Content = Multilingual.L("T_ReopenUnsavedWarning"),
                    PrimaryButtonText = Multilingual.L("T_Reopen"),
                    CloseButtonText = Multilingual.L("T_Cancel"),
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = App.m_window.Content.XamlRoot
                };
                var confirmRes = await confirmDialog.ShowAsync();
                if (confirmRes != ContentDialogResult.Primary)
                    return;
            }

            OpenFileHelper.ReopenWithEncoding(tab, EncodingHelper.GetEncodingByIndex(encodingCombobox.SelectedIndex));
        }
    }
}
