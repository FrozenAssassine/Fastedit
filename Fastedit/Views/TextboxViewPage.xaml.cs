using Fastedit.Core;
using Fastedit.Core.Tab;
using Fastedit.Extensions;
using Fastedit.Helper;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit.Views
{
    public sealed partial class TextboxViewPage : Page
    {
        private readonly AppSettings appsettings = new AppSettings();

        public void SetTitlebar(string Text)
        {
            TitleDisplay.Text = Text;
            ApplicationView.GetForCurrentView().Title = Text;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            var currentView = SystemNavigationManager.GetForCurrentView();

            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            if (appsettings.GetSettingsAsBool("UseMica", false))
            {
                Titlebar.Background = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                Color titlebarcolor = appsettings.GetSettingsAsColorWithDefault("TitleBarBackgroundColor", DefaultValues.DefaultTitleBarBackgroundColor);
                Titlebar.Background = appsettings.CreateBrushWithOrWithoutAcrylic(titlebarcolor);
            }

            Window.Current.SetTitleBar(Titlebar);
        }
        public async Task<bool> CloseWindow()
        {
            return await ApplicationView.GetForCurrentView().TryConsolidateAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SetSettingsToTextBox();
            BackgroundHelper.SetBackgroundToPage(this);
            base.OnNavigatedTo(e);
        }

        public TextboxViewPage()
        {
            this.InitializeComponent();
            this.SizeChanged += TextboxViewPage_SizeChanged;
            this.KeyDown += TextboxViewPage_KeyDown;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Windows.Foundation.Size { Height = 100, Width = 250 });
        }

        /*private async void Save(bool SaveAs = false)
        {
            string Title = "";
            if (TabPage != null && tabactions != null)
            {
                await TabPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if (SaveAs)
                        await savefilehelper.SaveFileAs(TabPage);
                    else
                        await savefilehelper.Save(TabPage);

                    Title = tabactions.GetTextBoxFromTabPage(TabPage).Header;
                });
            }
            SetTitlebar(Title);      
        }*/

        //Handle all the events
        private async void TextboxViewPage_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
            //var shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);

            //Control was pressed:
            if (ctrl.HasFlag(CoreVirtualKeyStates.Down))
            {
                //Close this window and return back to the mainwindow
                if (e.Key == VirtualKey.B)
                {
                    await CloseWindow();
                }
                if (e.Key == VirtualKey.F11)
                {
                    Utilities.ToggleFullscreen();
                }

                //if (e.Key == VirtualKey.S)
                //{
                //    Save();
                //}

                ////Control and shift was pressed:
                //if (shift.HasFlag(CoreVirtualKeyStates.Down))
                //{
                //    if(e.Key == VirtualKey.S)
                //    {
                //        Save(true);
                //    }
                //}
            }
        }
        private void TextboxViewPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 300)
            {
                TitleDisplay.Visibility = Visibility.Collapsed;
            }
            else
            {
                TitleDisplay.Visibility = Visibility.Visible;
                TitleDisplay.Margin = new Thickness(0, 8, 0, 0);
                TitleDisplay.HorizontalAlignment = HorizontalAlignment.Center;
            }
        }
        private async void ReturnToOldWindowButton_Click(object sender, RoutedEventArgs e)
        {
            await CloseWindow();
        }
        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            FullscreenButton.Content = Utilities.ToggleFullscreen() ? "\uE73F" : "\uE740";
        }
        //private async void Save_Click(object sender, RoutedEventArgs e)
        //{
        //    Save();
        //}

        public TextWrapping Textwrapping
        {
            get => MainTextbox.WordWrap;
            set => MainTextbox.WordWrap = value;
        }
        public string Title
        {
            get => TitleDisplay.Text;
            set { TitleDisplay.Text = value; SetTitlebar(this.Title); }
        }
        //A string to let the app know which tab belongs to this page
        public string TabPageName { get; set; }
        //Get the text from the textbox
        public string TextboxText
        {
            get => MainTextbox.GetText();
            set => MainTextbox.SetText(value);
        }
        //Get/Set the zoomfactor for the textbox
        public double ZoomFactor
        {
            get => MainTextbox._zoomFactor;
            set
            {
                MainTextbox._zoomFactor = value;
                MainTextbox.SetFontZoomFactor(MainTextbox._zoomFactor);
            }
        }
        //Get/Set whether tabpage is readonly:
        public bool IsReadonly
        {
            get => MainTextbox.IsReadOnly;
            set => MainTextbox.IsReadOnly = value;
        }
        //Get/Set the cursorposition
        public int SelectionStart
        {
            get => MainTextbox.SelectionStart;
            set => MainTextbox.SelectionStart = value;
        }
        //return whether the textbox is modfied
        public bool IsModified { get => MainTextbox.IsModified; }

        private void SetSettingsToTextBox()
        {
            Titlebar.Background = new SolidColorBrush(appsettings.GetSettingsAsColorWithDefault("TabColorFocused", DefaultValues.DefaultTabColorFocused));
            MainTextbox.FontFamily = new FontFamily(appsettings.GetSettingsAsString("FontFamily", DefaultValues.DefaultFontFamily));
            MainTextbox.TextColor = appsettings.GetSettingsAsColorWithDefault("TextColor", DefaultValues.DefaultTextColor);
            MainTextbox.Background = appsettings.GetSettingsAsColorWithDefault("TextBackgroundColor", DefaultValues.DefaultTextBackgroundColor);
            MainTextbox.TextSelectionColor = appsettings.GetSettingsAsColorWithDefault("TextSelectionColor", DefaultValues.DefaultTextSelectionColor);
            MainTextbox.LineNumberBackground = appsettings.GetSettingsAsColorWithDefault("LineNumberBackgroundColor", DefaultValues.DefaultLineNumberBackgroundColor);
            MainTextbox.LineNumberForeground = appsettings.GetSettingsAsColorWithDefault("LineNumberForegroundColor", DefaultValues.DefaultLineNumberForegroundColor);
            MainTextbox.ShowLineNumbers = appsettings.GetSettingsAsBool("ShowLineNumbers", true);
            MainTextbox.SpellChecking = appsettings.GetSettingsAsBool("Spellchecking", DefaultValues.SpellCheckingEnabled);
            MainTextbox.FontSizeWithoutZoom = appsettings.GetSettingsAsInt("FontSize", DefaultValues.DefaultFontsize);
            MainTextbox.SetFontZoomFactor(MainTextbox._zoomFactor);
            MainTextbox.LineHighlighterBackground = appsettings.GetSettingsAsColorWithDefault("LineHighlighterBackground", Colors.Transparent);
            MainTextbox.LineHighlighterForeground = appsettings.GetSettingsAsColorWithDefault("LineHighlighterForeground", DefaultValues.SystemAccentColor);
            MainTextbox.LineHighlighter = appsettings.GetSettingsAsBool("LineHighlighter", true);
        }

        private async void CompactOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            var viewmode = ApplicationView.GetForCurrentView().ViewMode;
            viewmode = viewmode == ApplicationViewMode.CompactOverlay ?
                ApplicationViewMode.Default : ApplicationViewMode.CompactOverlay;

            var res = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(viewmode);

            viewmode = ApplicationView.GetForCurrentView().ViewMode;
            CompactOverlayButton.Content = viewmode == ApplicationViewMode.CompactOverlay ? "\uEE47" : "\uEE49";
        }
    }
}
