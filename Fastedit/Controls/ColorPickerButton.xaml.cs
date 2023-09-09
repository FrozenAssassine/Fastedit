using Fastedit.Settings;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Fastedit.Helper;

namespace Fastedit.Controls
{
    
    public sealed partial class ColorPickerButton : Windows.UI.Xaml.Controls.UserControl
    {
        private bool _IsCompact;
        private readonly Color buttonbordercolor;

        public ColorPickerButton()
        {
            this.InitializeComponent();

            buttonbordercolor = ThemeHelper.CurrentTheme == ElementTheme.Light ? Colors.Black : Colors.White;
            Colordisplay.BorderBrush = new SolidColorBrush(buttonbordercolor);
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
        private void Rectangle_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
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
