using Fastedit.Helper;
using Fastedit.Storage;
using Fastedit.Tab;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
    public class EncodingDialog
    {
        private static ComboBox encodingCombobox;

        public static async Task Show(TabPageItem tab)
        {
            if (tab == null)
                return;

            //create the combobox:
            if (encodingCombobox == null)
            {
                encodingCombobox = new ComboBox
                {
                    Header = "Encoding:",
                    HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center,
                    VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center
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
                SecondaryButtonText = tab.DatabaseItem.FileToken.Length > 0 ? "Reopen" : "", //only when tab is a local file
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
            };
            var res = await dialog.ShowAsync();
            dialog.Content = null;

            if (res == ContentDialogResult.Primary)
                tab.Encoding = EncodingHelper.GetEncodingByIndex(encodingCombobox.SelectedIndex);
            else if (res == ContentDialogResult.Secondary)
            {
                //reopen the file with the encoding specified
                await OpenFileHelper.ReopenWithEncoding(tab, EncodingHelper.GetEncodingByIndex(encodingCombobox.SelectedIndex));
            }
        }
    }
}
