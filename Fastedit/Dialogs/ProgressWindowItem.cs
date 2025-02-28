using Microsoft.UI.Xaml.Controls;

namespace Fastedit.Dialogs;

public class ProgressWindowItem
{
    public ProgressWindowItem(ProgressRing progressRing, TextBlock progressDisplay)
    {
        ProgressRing = progressRing;
        ProgressDisplay = progressDisplay;
    }

    public ProgressRing ProgressRing { get; set; }
    public TextBlock ProgressDisplay { get; set; }

    public void ShowProgress()
    {
        ProgressRing.IsActive = true;
        ProgressRing.Visibility = ProgressDisplay.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
    }
    public void HideProgress()
    {
        ProgressRing.IsActive = false;
        ProgressRing.Visibility = ProgressDisplay.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
    }
    public void SetText(string text)
    {
        ProgressDisplay.Text = text;
    }
}
