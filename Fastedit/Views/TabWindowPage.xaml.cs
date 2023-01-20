using Fastedit.Helper;
using Fastedit.Storage;
using Fastedit.Tab;
using System;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Fastedit.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TabWindowPage : Page
    {
        public AppWindow window { get; set; }
        public TabPageItem tab { get; set; }
        public Grid MainGrid => mainGrid;

        public TabWindowPage(TabPageItem tab, AppWindow window)
        {
            this.InitializeComponent();

            this.tab = tab;
            this.window = window;

            Grid.SetRow(tab.textbox, 1);
            //Add the textbox to the contentControl
            this.mainGrid.Children.Add(tab.textbox);

            tab.textbox.ContextFlyout = RightClickMenu;
        }

        public void Close()
        {
            //Show the default rightclick menu
            tab.textbox.ContextFlyout = null;

            Grid.SetRow(tab.textbox, 0);
            this.mainGrid.Children.Remove(tab.textbox);
        }

        private void Fullscreen_Click(object sender, RoutedEventArgs e)
        {
            WindowHelper.ToggleFullscreenForAppWindow(window);
        }
        private void CompactOverlay_Click(object sender, RoutedEventArgs e)
        {
            WindowHelper.ToggleCompactOverlayForAppWindow(window);
        }
        private async void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            await SaveFileHelper.SaveFileAs(tab);
        }
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            await SaveFileHelper.Save(tab);
        }
        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            await OpenFileHelper.OpenFileForTab(tab);
        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var ctrl = Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            var shift = Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            if (ctrl)
            {
                switch (e.Key)
                {
                    case Windows.System.VirtualKey.L:
                        Close_Click(null, null);
                        break;
                    case Windows.System.VirtualKey.S:
                        if (shift)
                            SaveAs_Click(null, null);
                        else
                            Save_Click(null, null);
                        break;
                    case Windows.System.VirtualKey.O:
                        Open_Click(null, null);
                        break;
                    case Windows.System.VirtualKey.F11:
                        Fullscreen_Click(null, null);
                        break;
                    case Windows.System.VirtualKey.K:
                        CompactOverlay_Click(null, null);
                        break;
                    case Windows.System.VirtualKey.D:
                        EditActions.DuplicateLine(tab);
                        break;
                }
            }
        }
        public Grid Titlebar => titlebar;

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            tab.textbox.Cut();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            tab.textbox.Paste();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            tab.textbox.Copy();
        }

        private async void Close_Click(object sender, RoutedEventArgs e)
        {
            if (window != null)
                await window.CloseAsync();
        }
    }
}
