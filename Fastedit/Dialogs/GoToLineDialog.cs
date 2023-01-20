using Fastedit.Helper;
using Fastedit.Tab;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
    internal class GoToLineDialog
    {
        public static async Task<bool> Show(TabPageItem tab)
        {
            if (tab == null)
                return false;

            var input = new NumberBox
            {
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch,
                Maximum = tab.textbox.NumberOfLines,
                Minimum = 0,
                LargeChange = 50,
                SmallChange = 1,
            };
            var dialog = new ContentDialog
            {
                Background = DialogHelper.ContentDialogBackground(),
                Foreground = DialogHelper.ContentDialogForeground(),
                RequestedTheme = DialogHelper.DialogDesign,
                Title = "Go to line",
                Content = input,
                PrimaryButtonText = "Ok",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
            };
            var res = await dialog.ShowAsync();

            if (res == ContentDialogResult.Primary)
            {
                EditActions.GoToLine(tab, ConvertHelper.ToInt(input.Value - 1, 0));
                return true;
            }
            return false;
        }

    }
}
