using Fastedit.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using TextControlBoxNS;

namespace Fastedit.Controls
{
    public sealed partial class SurroundWithFlyout : Flyout
    {
        private TextControlBox textbox;

        public SurroundWithFlyout()
        {
            this.InitializeComponent();
        }

        public void ShowFlyout(TextControlBox textbox)
        {
            surroundText1.Text = surroundText2.Text = "";
            this.textbox = textbox;
            surroundText2.Visibility = Visibility.Collapsed;

            base.ShowAt(textbox, new Microsoft.UI.Xaml.Controls.Primitives.FlyoutShowOptions { Position = textbox.GetCursorPosition() });
        }

        private void UserControl_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Tab)
            {
                surroundText2.Visibility = surroundText2.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
            else if(e.Key == Windows.System.VirtualKey.Enter)
            {
                try
                {
                    if (surroundText1.Text.Length > 0 && surroundText2.Text.Length > 0)
                        textbox.SurroundSelectionWith(surroundText1.Text, surroundText2.Text);
                    else if (surroundText1.Text.Length > 0)
                        textbox.SurroundSelectionWith(surroundText1.Text);
                }
                catch
                {
                    InfoMessages.UnhandledException("Internal textbox error in surround selection with");
                }

                this.Hide();
            }
        }
    }
}
