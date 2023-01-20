using Fastedit.Helper;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Fastedit.Controls
{
    public sealed partial class SettingsControl : UserControl
    {
        public delegate void ClickedEvent(SettingsControl sender);
        public event ClickedEvent Clicked;

        public SettingsControl()
        {
            this.InitializeComponent();
        }

        public bool Clickable { get; set; }

        private string _Glyph;
        public string Glyph { get => _Glyph; set { _Glyph = value; iconDisplay.Visibility = ConvertHelper.BoolToVisibility(value.Length > 0); } }
        public string Header { get; set; }
        public new UIElement Content
        {
            set
            {
                contentHost.Content = value;
            }
        }

        private void mainGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
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
}
