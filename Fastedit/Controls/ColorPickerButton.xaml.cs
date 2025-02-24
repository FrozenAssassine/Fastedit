using Microsoft.UI.Xaml.Controls;
using Windows.UI;
using Fastedit.Helper;
using Microsoft.UI.Xaml.Input;

namespace Fastedit.Controls
{
    
    public sealed partial class ColorPickerButton : UserControl
    {
        private bool _IsCompact;

        public ColorPickerButton()
        {
            this.InitializeComponent();
        }

        //public properties and events:
        public bool IsUsedAsDisplay { get; set; }
        public bool IsAlphaEnabled
        {
            set
            {
                ColorPickerFlyout.IsAlphaEnabled = value;
            }
        }

        public Color? Color
        {
            get => ColorPickerFlyout.Color;
            set => ColorPickerFlyout.Color = ConvertHelper.ToColor(value);
        }

        public string Header { get; set; }
        public bool IsCompact
        {
            get => _IsCompact;
            set
            {
                Colordisplay.Height = value ? 20 : 30;
                _IsCompact = value;
            }
        }

        public delegate void ColorChangedEvent(ColorPicker args);
        public event ColorChangedEvent ColorChanged;


        //private events
        private void Rectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!IsUsedAsDisplay)
            {
                flyout.ShowAt(Colordisplay);
            }
        }
        private void PickerFlyout_Closed(object sender, object e)
        {
            if (!IsUsedAsDisplay)
            {
                ColorChanged?.Invoke(ColorPickerFlyout);
            }
        }
    }
}
