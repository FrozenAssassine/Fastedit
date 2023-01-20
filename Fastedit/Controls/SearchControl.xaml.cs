using Fastedit.Helper;
using Fastedit.Settings;
using TextControlBox.Helper;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Fastedit.Controls
{
    public sealed partial class SearchControl : UserControl
    {
        private TextControlBox.TextControlBox currentTextbox = null;
        private SearchWindowState searchWindowState = SearchWindowState.Hidden;

        public SearchControl()
        {
            this.InitializeComponent();
        }

        private void BeginSearch(string searchword, bool matchCase, bool wholeWord)
        {
            if (currentTextbox == null)
                return;

            try
            {
                //begin the search
                var res = currentTextbox.BeginSearch(searchword, wholeWord, matchCase);
                ColorWindowBorder(res);
            }
            catch
            {
                //Due to bug in TextControlBox
            }
        }
        private void ToggleVisibility(bool visible)
        {
            textToReplaceTextBox.Visibility = ReplaceAllButton.Visibility =
                StartReplaceButton.Visibility = ConvertHelper.BoolToVisibility(visible);
        }
        private void ColorWindowBorder(SearchResult result)
        {
            SearchWindow.BorderBrush = new SolidColorBrush(result == SearchResult.Found ? DefaultValues.correctInputColor : DefaultValues.wrongInputColor);
        }
        private void hideWindow()
        {
            hideSearchAnimation.Begin();
            searchWindowState = SearchWindowState.Hidden;
        }
        private void showWindow()
        {
            this.Visibility = Visibility.Visible;
            showSearchAnimation.Begin();
            searchWindowState = SearchWindowState.Default;
        }
        private void expandReplace()
        {
            expandSearchAnimation.Begin();
            searchWindowState = SearchWindowState.Expanded;
        }
        private void collapseReplace()
        {
            collapseSearchAnimation.Begin();
            searchWindowState = SearchWindowState.Default;
        }

        public void ShowSearch(TextControlBox.TextControlBox textbox)
        {
            if (textbox == null)
                return;
            currentTextbox = textbox;

            //hide the window when in search state or show it when in hidden state or show it when hidden:
            if (searchWindowState == SearchWindowState.Default)
                hideWindow();
            else if (searchWindowState == SearchWindowState.Expanded)
                collapseReplace();
            else
            {
                showWindow();
                collapseReplace();
            }


            if (textbox.SelectionLength > 0 && textbox.SelectionLength < 200)
                textToFindTextbox.Text = textbox.SelectedText;

            textToFindTextbox.Focus(FocusState.Keyboard);
            textToFindTextbox.SelectAll();
        }
        public void ShowReplace(TextControlBox.TextControlBox textbox)
        {
            if (textbox == null)
                return;
            currentTextbox = textbox;

            //hide the window when in replace state or expand it when in search state or show it when hidden:
            if (searchWindowState == SearchWindowState.Expanded)
                hideWindow();
            else if (searchWindowState == SearchWindowState.Default)
                expandReplace();
            else
            {
                showWindow();
                expandReplace();
            }

            if (textbox.SelectionLength > 0 && textbox.SelectionLength < 200)
                textToFindTextbox.Text = textbox.SelectedText;

            textToReplaceTextBox.Focus(FocusState.Keyboard);
            textToReplaceTextBox.SelectAll();
            textToFindTextbox.Focus(FocusState.Keyboard);
            textToFindTextbox.SelectAll();
        }
        public void Close()
        {
            currentTextbox.EndSearch();
            hideWindow();
            currentTextbox = null;
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
            var shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);
            if (e.Key == VirtualKey.Enter)
            {
                if (currentTextbox == null)
                    return;

                try
                {
                    if (shift)
                        currentTextbox.FindPrevious();
                    else
                        currentTextbox.FindNext();
                }
                catch
                {
                    //Due to bug in TextControlBox
                }
            }
        }
        private void SearchUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentTextbox == null)
                return;

            try
            {
                currentTextbox.FindPrevious();
            }
            catch
            {
                //Due to bug in TextControlBox
            }
        }
        private void SearchDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentTextbox == null)
                return;
            try
            {
                currentTextbox.FindNext();
            }
            catch
            {
                //Due to bug in TextControlBox
            }
        }
        private void FindMatchCaseButton_Click(object sender, RoutedEventArgs e)
        {
            //AppSettings.SaveSettings("FindMatchCase", FindMatchCaseButton.IsChecked);
        }
        private void FindWholeWordButton_Click(object sender, RoutedEventArgs e)
        {
            //if (SaveToSettings)
            //appsettings.SaveSettings("FindWholeWord", FindWholeWordButton.IsChecked);
        }
        private void ReplaceAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentTextbox == null)
                return;

            var res = currentTextbox.ReplaceAll(
                textToFindTextbox.Text,
                textToReplaceTextBox.Text,
                FindMatchCaseButton.IsChecked ?? false,
                FindWholeWordButton.IsChecked ?? false
                );

            ColorWindowBorder(res);
        }
        private void ReplaceCurrentButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentTextbox == null)
                return;
        }
        private void SearchWindow_CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void ExpandSearchBoxForReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (searchWindowState == SearchWindowState.Expanded)
                ShowSearch(currentTextbox);
            else
                ShowReplace(currentTextbox);
        }
        private void TextBoxes_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.SelectAll();
            }
        }

        private void textToFindTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            BeginSearch(textToFindTextbox.Text, FindMatchCaseButton.IsChecked ?? false, FindWholeWordButton.IsChecked ?? false);
        }

        private void collapseSearchAnimation_Completed(object sender, object e)
        {
            ToggleVisibility(false);
        }

        private void expandSearchAnimation_Completed(object sender, object e)
        {
            ToggleVisibility(true);
        }

        private void hideSearchAnimation_Completed(object sender, object e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
    public enum SearchWindowState
    {
        Expanded, //replace and search
        Default, //only search
        Hidden //not visible
    }
}
