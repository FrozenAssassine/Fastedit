using Fastedit.Helper;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Fastedit.Controls;

public sealed partial class SettingsControl : UserControl
{
    public delegate void ClickedEvent(SettingsControl sender);
    public event ClickedEvent Clicked;

    public SettingsControl()
    {
        this.InitializeComponent();
    }

    private Brush _Background;
    public new Brush Background
    {
        get => _Background;
        set { mainGrid.Background = _Background = value; }
    }

    public bool Clickable { get; set; }

    private string _Glyph;
    public string Glyph { get => _Glyph; set { _Glyph = value; iconDisplay.Visibility = ConvertHelper.BoolToVisibility(value.Length > 0); } }
    public string Header { get; set; }
    public string InfoText { get; set; }
    public new UIElement Content
    {
        set { contentHost.Content = value; }
    }

    private void MainGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        Clicked?.Invoke(this);
    }

    private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (!Clickable)
            return;

        EnterStoryboard.Begin();
    }

    private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!Clickable)
            return;

        ExitStoryboard.Begin();
    }
}
