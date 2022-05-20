using Fastedit.Core.Tab;
using Fastedit.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Controls;
using Convert = Fastedit.Extensions.Convert;
using Fastedit.Controls.Textbox;
using Windows.UI;

namespace Fastedit.Controls.Textbox
{
    public sealed partial class Searchdialog : UserControl
    {
        private AppSettings appsettings = new AppSettings();
        public TextControlBox textbox = null;
       
        public Searchdialog(TextControlBox textbox)
        {
            this.InitializeComponent();
            this.textbox = textbox;
        }

        public new Brush Background
        {
            get => SearchWindow.Background;
            set => SearchWindow.Background = value;
        }
        public bool SearchIsOpen
        {
            get => this.Visibility == Visibility.Visible;
            set
            {
                this.Visibility = Convert.BoolToVisibility(value);
                if (value)
                {
                    TextToFindTextbox.Focus(FocusState.Keyboard);
                    TextToFindTextbox.SelectAll();
                }
            }
        }
        private bool _ReplaceIsOpen = false;
        public bool ReplaceIsOpen
        {
            get => _ReplaceIsOpen;
            set
            {
                if (value)
                {
                    ExpandSearch.Begin();
                    SearchWindow.Height = 125;
                    ExpandSearchBoxForReplaceButton.Content = "\uF0AD";
                    if (SaveToSettings)
                        appsettings.SaveSettings("SearchExpanded", 0);

                }
                else
                {
                    if (SearchWindow.Height > 45)
                        CollapseSearch.Begin();
                    ExpandSearchBoxForReplaceButton.Content = "\uF0AE";
                    if (SaveToSettings)
                        appsettings.SaveSettings("SearchExpanded", 1);
                }
                _ReplaceIsOpen = value;
                TextToReplaceTextBox.Visibility = ReplaceAllButton.Visibility =
                StartReplaceButton.Visibility = Convert.BoolToVisibility(value);
                TextToReplaceTextBox.Focus(FocusState.Keyboard);
                TextToReplaceTextBox.SelectAll();
            }
        }
        public void Find(bool Up = false)
        {
            if (textbox != null)
            {
                var res = textbox.FindInText(TextToFindTextbox.Text, Up, FindMatchCaseButton.IsChecked ?? false, FindWholeWordButton.IsChecked ?? false);
                ColorWindowBorder(res);
            }
        }
        public void Show(string text = "")
        {
            if (textbox == null)
                return;

            TextToFindTextbox.Text = text.Length == 0 ? textbox.SelectedText : text;
            SearchIsOpen = true;

            if (SaveToSettings)
            {
                appsettings.SaveSettings("SearchOpen", true);
                FindMatchCaseButton.IsChecked = appsettings.GetSettingsAsBool("FindMatchCase", false);
                FindWholeWordButton.IsChecked = appsettings.GetSettingsAsBool("FindWholeWord", false);
            }

            TextToFindTextbox.Focus(FocusState.Keyboard);
            TextToFindTextbox.SelectAll();
        }
        public void Close()
        {
            SearchIsOpen = false;
            if(SaveToSettings)
                appsettings.SaveSettings("SearchOpen", false);
        }
        public void Toggle(bool Replace)
        {
            if (!SearchIsOpen)
            {
                Show("");
                this.Replace(Replace);
            }
            else
                Close();
        }
        public void Replace(bool IsOn)
        {
            ReplaceIsOpen = IsOn;
        }
        public bool SaveToSettings { get; set; } = true;



        private void ColorWindowBorder(bool state)
        {
            if (SaveToSettings)
                SearchWindow.BorderBrush = state ? DefaultValues.CorrectInput_Color : DefaultValues.WrongInput_Color;
        }
        private void ReplaceTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                ReplaceCurrentButton_Click(null, null);
            }
        }
        private void SearchTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            //Search down on Enter and up on Shift + Enter//
            var shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);
            if (e.Key == VirtualKey.Enter)
            {
                Find(shift.HasFlag(CoreVirtualKeyStates.Down));
            }
        }
        private void SearchUpButton_Click(object sender, RoutedEventArgs e)
        {
            Find(true);
        }
        private void SearchDownButton_Click(object sender, RoutedEventArgs e)
        {
            Find(false);
        }
        private void FindMatchCaseButton_Click(object sender, RoutedEventArgs e)
        {
            if(SaveToSettings)
                appsettings.SaveSettings("FindMatchCase", FindMatchCaseButton.IsChecked);
        }
        private void FindWholeWordButton_Click(object sender, RoutedEventArgs e)
        {
            if(SaveToSettings)
                appsettings.SaveSettings("FindWholeWord", FindWholeWordButton.IsChecked);
        }
        private void ReplaceAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (textbox != null)
            {
                var res = textbox.ReplaceAll(TextToFindTextbox.Text, TextToReplaceTextBox.Text,
                    false, FindMatchCaseButton.IsChecked ?? false, FindWholeWordButton.IsChecked ?? false);
                ColorWindowBorder(res);
            }
        }
        private void ReplaceCurrentButton_Click(object sender, RoutedEventArgs e)
        {
            if (textbox != null)
            {
                var res = textbox.ReplaceInText(
                    TextToFindTextbox.Text, TextToReplaceTextBox.Text,
                    false, FindMatchCaseButton.IsChecked ?? false, FindWholeWordButton.IsChecked ?? false);
                ColorWindowBorder(res);
            }
        }
        private void SearchWindow_CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void ExpandSearchBoxForReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            Replace(SaveToSettings ? appsettings.GetSettingsAsInt("SearchExpanded", 0) == 1 : !ReplaceIsOpen);
        }
        private void TextBoxes_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.SelectAll();
            }
        }
    }
}
