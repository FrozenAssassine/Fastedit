using Fastedit.Helper;
using Fastedit.Storage;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Fastedit.Core.Tab;

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
                Header = "Encoding:",
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                Width=200,
            };
            encodingCombobox.ItemsSource = EncodingHelper.AllEncodingNames;
        }

        encodingCombobox.SelectedIndex = EncodingHelper.GetIndexByEncoding(tab.Encoding);
        var dialog = new ContentDialog
        {
            Background = DialogHelper.ContentDialogBackground(),
            Foreground = DialogHelper.ContentDialogForeground(),
            RequestedTheme = DialogHelper.DialogDesign,
            Title = "Encoding",
            Content = encodingCombobox,
            PrimaryButtonText = "Done",
            SecondaryButtonText = tab.DatabaseItem.WasNeverSaved ? "" : "Reopen",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = App.m_window.Content.XamlRoot
        };
        var res = await dialog.ShowAsync();
        dialog.Content = null;

        if (res == ContentDialogResult.Primary)
            tab.Encoding = EncodingHelper.GetEncodingByIndex(encodingCombobox.SelectedIndex);
        else if (res == ContentDialogResult.Secondary)
        {
            OpenFileHelper.ReopenWithEncoding(tab, EncodingHelper.GetEncodingByIndex(encodingCombobox.SelectedIndex));
        }
    }
}
