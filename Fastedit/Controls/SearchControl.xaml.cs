using Fastedit.Helper;
using Fastedit.Settings;
using Fastedit.Tab;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using TextControlBoxNS;
using Windows.System;

namespace Fastedit.Controls
{
    public sealed partial class SearchControl : UserControl
    {
        private TextControlBox currentTextbox = null;
        public TabPageItem currentTab = null;
        public bool searchOpen = false;

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
                //Due to bug in TextControlBox TODO! double check and fix!
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

        public void ShowSearch(TabPageItem tab)
        {
            if (tab == null)
                return;
            currentTextbox = tab.textbox;
            currentTab = tab;
            searchOpen = true;

            //hide the window when in search state or show it when in hidden state or show it when hidden:
            if (searchWindowState == SearchWindowState.Default && !currentTextbox.HasSelection)
                hideWindow();
            else if (searchWindowState == SearchWindowState.Expanded)
                collapseReplace();
            else
            {
                showWindow();
                collapseReplace();
            }

            if (currentTextbox.HasSelection && currentTextbox.CalculateSelectionPosition().Length < 200)
            {
                textToFindTextbox.Text = currentTextbox.SelectedText;
            }

            textToFindTextbox.Focus(FocusState.Keyboard);
            textToFindTextbox.SelectAll();
        }
        public void ShowReplace(TabPageItem tab)
        {
            if (tab == null)
                return;
            
            currentTextbox = tab.textbox;
            currentTab = tab;
            searchOpen = true;

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

            if (currentTextbox.HasSelection && currentTextbox.CalculateSelectionPosition().Length < 200)
            {
                textToFindTextbox.Text = currentTextbox.SelectedText;
            }

            textToReplaceTextBox.Focus(FocusState.Keyboard);
            textToReplaceTextBox.SelectAll();
            textToFindTextbox.Focus(FocusState.Keyboard);
            textToFindTextbox.SelectAll();
        }
        public void Close()
        {
            searchOpen = false;
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
            var shift = KeyHelper.IsKeyPressed(VirtualKey.Shift);
            if (e.Key == VirtualKey.Enter)
            {
                if (currentTextbox == null)
                    return;

                if (shift)
                    currentTextbox.FindPrevious();
                else
                    currentTextbox.FindNext();
            }
        }
        private void SearchUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentTextbox == null)
                return;

            currentTextbox.FindPrevious();
        }
        private void SearchDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentTextbox == null)
                return;
            currentTextbox.FindNext();
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

            if (res == SearchResult.Found)
            {
                currentTab.DatabaseItem.IsModified = true;
                currentTab.UpdateHeader();
            }

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
                ShowSearch(currentTab);
            else
                ShowReplace(currentTab);
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
