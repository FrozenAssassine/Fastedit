using Fastedit.Controls.Textbox;
using Fastedit.Core;
using Fastedit.Extensions;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using mucx = Microsoft.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
    public class EncodingDialog
    {
        ContentDialog dialog = null;
        Encoding selectedEncoding = null;

        private int GetIndexOfSelected(ComboBox cb, Encoding enc)
        {
            var name = Encodings.EncodingToString(enc);
            var lst = cb.Items.ToList();
            return lst.IndexOf(lst.Find(item => ((ComboBoxItem)item).Content.ToString() == name));
        }

        public EncodingDialog(mucx.TabViewItem TabPage)
        {
            if (TabPage.Content is TextControlBox textbox)
            {
                var encodingcombobox = new ComboBox
                {
                    Width = 150,
                };

                for(int i = 0; i<Encodings.AllEncodingNames.Count; i++)
                {
                    encodingcombobox.Items.Add(new ComboBoxItem { Content = Encodings.AllEncodingNames[i] });
                }
               
                encodingcombobox.HorizontalAlignment = HorizontalAlignment.Center;
                encodingcombobox.VerticalAlignment = VerticalAlignment.Center;
                encodingcombobox.SelectedIndex = GetIndexOfSelected(encodingcombobox, textbox.Encoding);
                encodingcombobox.SelectionChanged += delegate
                {
                    if (encodingcombobox.SelectedItem is ComboBoxItem item)
                    {
                        selectedEncoding = Encodings.StringToEncoding(item.Content.ToString());
                    }
                };
                dialog = new ContentDialog
                {
                    Background = DefaultValues.ContentDialogBackgroundColor(),
                    Foreground = DefaultValues.ContentDialogForegroundColor(),
                    RequestedTheme = DefaultValues.ContentDialogTheme(),
                    CornerRadius = DefaultValues.DefaultDialogCornerRadius,
                    CloseButtonText = AppSettings.GetResourceStringStatic("EncodingDialog_Button_Cancel/Text"),
                    PrimaryButtonText = AppSettings.GetResourceStringStatic("EncodingDialog_Button_Done/Text"),
                    Content = encodingcombobox,
                    Width = 200,
                    Title = AppSettings.GetResourceStringStatic("EncodingDialog_Title/Text"),
                };
                dialog.PrimaryButtonClick += delegate
                {
                    if (selectedEncoding != null)
                    {
                        textbox.Encoding = selectedEncoding;
                    }
                };
            }
        }
        public async Task ShowDialog()
        {
            await dialog.ShowAsync();
        }
    }
}
