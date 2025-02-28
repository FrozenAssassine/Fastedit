using Fastedit.Helper;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;

namespace Fastedit.Controls
{
    public sealed partial class StatusbarItem : UserControl, INotifyPropertyChanged
    {
        public StatusbarItem()
        {
            this.InitializeComponent();
        }

        public string Text { get => CustomText ?? StaticText + ChangingText; }
        private string _CustomText;
        public string CustomText { get => _CustomText; set { _CustomText = value; NotifyPropertyChanged("Text"); } }
        public FlyoutBase CustomFlyout { set => button.Flyout = value; }
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
        public string StaticText { get => _StaticText; set { _StaticText = value; NotifyPropertyChanged("Text"); }}
        public string ChangingText { get => _ChangingText; set { _ChangingText = value; NotifyPropertyChanged("Text"); }}
        private Brush _Foreground { get; set; }
        public new Brush Foreground { get => _Foreground; set { _Foreground = value; NotifyPropertyChanged("Foreground"); } }

        public delegate void StatusbarItemClickEvent(StatusbarItem sender, Button children);
        public event StatusbarItemClickEvent StatusbarItemClick;
        public event PropertyChangedEventHandler PropertyChanged;

        public event FlyoutOpeningEvent FlyoutOpening;
        public delegate void FlyoutOpeningEvent();
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void HideFlyout()
        {
            flyout.Hide();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StatusbarItemClick?.Invoke(this, button);
        }

        private void Flyout_Opening(object sender, object e)
        {
            FlyoutOpening?.Invoke();
        }
    }
}
