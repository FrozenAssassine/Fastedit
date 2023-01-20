using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Fastedit.Controls
{
    public sealed partial class StatusbarItem : UserControl
    {
        public delegate void StatusbarItemClickEvent(StatusbarItem sender, Button children);
        public event StatusbarItemClickEvent StatusbarItemClick;

        public StatusbarItem()
        {
            this.InitializeComponent();
        }

        public FlyoutBase CustomFlyout
        {
            set
            {
                button.Flyout = value;
            }
        }

        public UIElement FlyoutContent
        {
            get => flyout.Content;
            set
            {
                button.Flyout = value == null ? null : flyout;
                flyout.Content = value;
            }
        }

        private string _StaticText;
        private string _ChangingText;
        public string StaticText { get => _StaticText; set { _StaticText = value; UpdateContent(); } }
        public string ChangingText { get => _ChangingText; set { _ChangingText = value; UpdateContent(); } }

        public new Brush Foreground { set { button.Foreground = value; } get => button.Foreground; }
        public void HideFlyout()
        {
            flyout.Hide();
        }

        public void UpdateContent()
        {
            button.Content = _StaticText + _ChangingText;
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            StatusbarItemClick?.Invoke(this, button);
        }
    }
}
