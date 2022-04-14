using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Fastedit.Controls
{
    public sealed partial class ColorChooserButton : UserControl
    {
        public bool IsUsedAsDisplay { get; set; }
        private bool _IsCompact;
        public bool IsCompact
        {
            get => _IsCompact;
            set
            {
                if (value)
                {
                    Colordisplay.Height = 20;
                }
                else
                {
                    Colordisplay.Height = 30;
                }
                _IsCompact = value;
            }
        }
        private readonly AppSettings appsettings = new AppSettings();
        private readonly Color buttonbordercolor = Colors.Black;

        public ColorChooserButton()
        {
            this.InitializeComponent();

            buttonbordercolor = appsettings.CurrentApplicationTheme == ElementTheme.Light ? Colors.Black : Colors.White;

            Colordisplay.BorderBrush = new SolidColorBrush(buttonbordercolor);
        }

        public delegate void ColorChanged(ColorPicker sender);
        public event ColorChanged ColorChangedEvent;

        public bool IsAlphaEnabled
        {
            set
            {
                ColorPickerFlyout.IsAlphaEnabled = value;
            }
        }
        public Color Color
        {
            get { return ColorPickerFlyout.Color; }
            set { ColorPickerFlyout.Color = value; }
        }

        private void Rectangle_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (!IsUsedAsDisplay)
            {
                flyout.ShowAt(Colordisplay);
            }
        }

        private void ColorPickerFlyout_ColorChanged_1(ColorPicker sender, ColorChangedEventArgs args)
        {

        }

        private void flyout_Closed(object sender, object e)
        {
            if (!IsUsedAsDisplay)
            {
                ColorChangedEvent?.Invoke(ColorPickerFlyout);
            }
        }
    }
}
