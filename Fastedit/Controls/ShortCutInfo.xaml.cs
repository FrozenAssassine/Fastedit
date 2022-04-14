using Fastedit.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Controls
{
    public sealed partial class ShortCutInfo : UserControl
    {
        public ShortCutInfo()
        {
            this.InitializeComponent();
        }

        private KeyModifiers _secondmodifier = KeyModifiers.None;
        public KeyModifiers SecondModifier
        {
            set
            {
                _secondmodifier = value;
                SetKey();
            }
        }

        private KeyModifiers _modifier = KeyModifiers.None;
        public KeyModifiers Modifier
        {
            set
            {
                _modifier = value;
                SetKey();
            }
        }

        private string ModifierString()
        {
            if (_modifier == KeyModifiers.Control)
            {
                return "Ctrl + ";
            }

            if (_modifier == KeyModifiers.Windows)
            {
                return "Window + ";
            }

            if (_modifier == KeyModifiers.Menu)
            {
                return "Alt + ";
            }

            if (_modifier == KeyModifiers.Shift)
            {
                return "Shift + ";
            }
            else
            {
                return "";
            }
        }
        private string SecondModifierString()
        {
            if (_secondmodifier == KeyModifiers.Control)
            {
                return "Ctrl + ";
            }

            if (_secondmodifier == KeyModifiers.Windows)
            {
                return "Window + ";
            }

            if (_secondmodifier == KeyModifiers.Menu)
            {
                return "Alt + ";
            }

            if (_secondmodifier == KeyModifiers.Shift)
            {
                return "Shift + ";
            }
            else
            {
                return "";
            }
        }
        private void SetKey()
        {
            display.Text = ModifierString() + SecondModifierString() + _Key;
            displayOnPress.Text = _ActionOnClick;
        }

        private string _Key;
        public string Key
        {
            set
            {
                _Key = value;
                SetKey();
            }
        }

        private string _ActionOnClick = "";
        public string ActionOnClick { set { _ActionOnClick = value; SetKey(); } }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 700)
            {
                displayOnPress.Margin = new Thickness(310, 5, 5, 5);
                display.Width = 200;
            }
            else
            {
                displayOnPress.Margin = new Thickness(210, 5, 5, 5);
                display.Width = 100;
            }
        }
    }
}
