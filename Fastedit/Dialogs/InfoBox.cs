using Fastedit.Core;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
    public class InfoBox : ContentDialog
    {
        //private ContentDialog dialog = null;
        public InfoBox(string content, string Title = "")
        {
            this.Title = Title;
            Background = DefaultValues.ContentDialogBackgroundColor();
            CornerRadius = DefaultValues.DefaultDialogCornerRadius;
            PrimaryButtonText = "Ok";
            Foreground = DefaultValues.ContentDialogForegroundColor();
            RequestedTheme = DefaultValues.ContentDialogTheme();

            Content = new TextBlock
            {
                Text = content,
                TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap
            };
        }
    }
}
