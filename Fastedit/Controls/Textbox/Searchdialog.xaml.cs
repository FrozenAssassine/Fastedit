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
        public TabViewItem tabpage = null;
        private TabActions tabactions = null;
        public TextControlBox textbox = null;

        public Searchdialog(TabViewItem tabpage, TabActions tabactions)
        {
            this.InitializeComponent();
            this.tabpage = tabpage;
            this.tabactions = tabactions;
            textbox = tabactions.GetTextBoxFromTabPage(tabpage);
        }

        public new Brush Background
        {
            get => SearchWindow.Background;
            set => SearchWindow.Background = value;
        }
        public bool SearchIsOpen
        {
            get => SearchWindow.Visibility == Visibility.Visible;
            set { SearchWindow.Visibility = Convert.BoolToVisibility(value); }
        }
        public void ShowReplace(bool Show)
        {
            if (Show == true)
            {
                ExpandSearch.Begin();
                SearchWindow.Height = 125;
                ExpandSearchBoxForReplaceButton.Content = "\uF0AD";
                appsettings.SaveSettings("SearchExpanded", 0);
                TextToReplaceTextBox.Visibility = Visibility.Visible;
                ReplaceAllButton.Visibility = Visibility.Visible;
                StartReplaceButton.Visibility = Visibility.Visible;
            }
            else
            {
                if (SearchWindow.Height > 45)
                {
                    CollapseSearch.Begin();
                }
                ExpandSearchBoxForReplaceButton.Content = "\uF0AE";
                appsettings.SaveSettings("SearchExpanded", 1);
                TextToReplaceTextBox.Visibility = Visibility.Collapsed;
                ReplaceAllButton.Visibility = Visibility.Collapsed;
                StartReplaceButton.Visibility = Visibility.Collapsed;
            }
        }
        public void Find(bool Up = false)
        {
            var tb = tabactions.GetTextBoxFromSelectedTabPage();
            if (tb != null)
            {
                var res = tb.FindInText(TextToFindTextbox.Text, Up, FindMatchCaseButton.IsChecked ?? false, FindWholeWordButton.IsChecked ?? false);

                SearchWindow.BorderBrush = res ? DefaultValues.CorrectInput_Color : SearchWindow.BorderBrush = DefaultValues.WrongInput_Color;
            }
        }
        public void ShowSearchWindow(string text = "")
        {
            if (textbox == null)
                return;

            TextToFindTextbox.Text = text.Length == 0 ? textbox.SelectedText : text;
            SearchIsOpen = true;

            appsettings.SaveSettings("SearchOpen", true);
            FindMatchCaseButton.IsChecked = appsettings.GetSettingsAsBool("FindMatchCase", false);
            FindWholeWordButton.IsChecked = appsettings.GetSettingsAsBool("FindWholeWord", false);

            TextToFindTextbox.Focus(FocusState.Keyboard);
            TextToFindTextbox.SelectAll();
        }
        public void CloseSearchWindow()
        {
            SearchIsOpen = false;
            appsettings.SaveSettings("SearchOpen", false);
        }
        public void ToggleSearchWnd(bool Replace)
        {
            if (!SearchIsOpen)
            {
                ShowSearchWindow("");
                this.ShowReplace(Replace);
            }
            else
            {
                CloseSearchWindow();
            }
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
            appsettings.SaveSettings("FindMatchCase", FindMatchCaseButton.IsChecked);
        }
        private void FindWholeWordButton_Click(object sender, RoutedEventArgs e)
        {
            appsettings.SaveSettings("FindWholeWord", FindWholeWordButton.IsChecked);

        }
        private void ReplaceAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (textbox != null)
            {
                if (tabactions.GetTextBoxFromSelectedTabPage().ReplaceAll(
                    TextToFindTextbox.Text, TextToReplaceTextBox.Text, false, FindMatchCaseButton.IsChecked ?? false, FindWholeWordButton.IsChecked ?? false))
                {
                    SearchWindow.BorderBrush = DefaultValues.CorrectInput_Color;
                }
                else
                {
                    SearchWindow.BorderBrush = DefaultValues.WrongInput_Color;
                }
            }
        }
        private void ReplaceCurrentButton_Click(object sender, RoutedEventArgs e)
        {
            if (textbox != null)
            {
                if (textbox.ReplaceInText(
                    TextToFindTextbox.Text, TextToReplaceTextBox.Text,
                    false, FindMatchCaseButton.IsChecked ?? false, FindWholeWordButton.IsChecked ?? false))
                {
                    SearchWindow.BorderBrush = DefaultValues.CorrectInput_Color;
                }
                else
                {
                    SearchWindow.BorderBrush = DefaultValues.WrongInput_Color;
                }
            }
        }
        private void SearchWindow_CloseButtonClick(object sender, RoutedEventArgs e)
        {
            CloseSearchWindow();
        }
        private void ExpandSearchBoxForReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSearchWindow();
            ShowReplace(appsettings.GetSettingsAsInt("SearchExpanded", 0) == 1);
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
