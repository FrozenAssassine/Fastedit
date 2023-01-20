using Windows.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
    public class ProgressWindowItem
    {
        public ProgressWindowItem(Microsoft.UI.Xaml.Controls.ProgressRing progressRing, TextBlock progressDisplay)
        {
            ProgressRing = progressRing;
            ProgressDisplay = progressDisplay;
        }

        public Microsoft.UI.Xaml.Controls.ProgressRing ProgressRing { get; set; }
        public TextBlock ProgressDisplay { get; set; }

        public void ShowProgress()
        {
            ProgressRing.IsActive = true;
            ProgressRing.Visibility = ProgressDisplay.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }
        public void HideProgress()
        {
            ProgressRing.IsActive = false;
            ProgressRing.Visibility = ProgressDisplay.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
        public void SetText(string text)
        {
            ProgressDisplay.Text = text;
        }
    }
}
