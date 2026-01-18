using Fastedit.Dialogs;
using Fastedit.Helper;
using Fastedit.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Fastedit.Controls;
using Fastedit.Core.Tab;
using Fastedit.Core;

namespace Fastedit.Views;

public sealed partial class TabWindowPage : Page
{
    public Window window { get; set; }
    public TabPageItem tab { get; set; }
    public Grid MainGrid => mainGrid;
    public Grid Titlebar => titlebar;
    public TextStatusBar Statusbar => textStatusBar;
    public TextBlock TitleText => titleText;
    public BackdropWindowManager backDropWindowManager;

    public TabWindowPage(TabPageItem tab, Window window, BackdropWindowManager backDropWindowManager)
    {
        this.InitializeComponent();

        this.tab = tab;
        this.window = window;

        this.Loaded += TabWindowPage_Loaded;
        this.backDropWindowManager = backDropWindowManager;
    }

    private void TabWindowPage_Loaded(object sender, RoutedEventArgs e)
    {
        textBoxWrapper.Children.Add(tab.textbox);
        tab.textbox.EndSearch();
        tab.textbox.ContextFlyout = RightClickMenu;

        readonlyModeFlyoutItem.IsChecked = tab.IsReadOnly;

        UpdateTabIcon();

        InitStatusbar();
    }

    private void InitStatusbar()
    {
        textStatusBar.tabPage = tab;
        textStatusBar.window = window;
        textStatusBar.UpdateAll();

        this.tab.textbox.SelectionChanged += Textbox_SelectionChanged;
        this.tab.textbox.TextChanged += Textbox_TextChanged;
        this.tab.textbox.ZoomChanged += Textbox_ZoomChanged;
    }

    private void Textbox_ZoomChanged(TextControlBoxNS.TextControlBox sender, int zoomFactor)
    {
        textStatusBar.UpdateZoom();
    }

    private void Textbox_TextChanged(TextControlBoxNS.TextControlBox sender)
    {
        textStatusBar.UpdateText();
    }

    private void Textbox_SelectionChanged(TextControlBoxNS.TextControlBox sender, TextControlBoxNS.SelectionChangedEventHandler args)
    {
        textStatusBar.UpdateSelectionChanged();
    }

    private void UpdateTabIcon()
    {
        pageIcon.Glyph = tab.getTabIconStr(true);
    }

    public void Close()
    {
        //Show the default rightclick menu
        tab.textbox.ContextFlyout = null;
        tab.textbox.EndSearch();
        textBoxWrapper.Children.Clear();

        this.tab.textbox.SelectionChanged -= Textbox_SelectionChanged;
        this.tab.textbox.TextChanged -= Textbox_TextChanged;
        this.tab.textbox.ZoomChanged -= Textbox_ZoomChanged;
    }

    private void Fullscreen_Click(object sender, RoutedEventArgs e)
    {
        WindowHelper.ToggleFullscreen(window);
    }
    private void CompactOverlay_Click(object sender, RoutedEventArgs e)
    {
        WindowHelper.ToggleCompactOverlay(window);
    }
    private async void SaveAs_Click(object sender, RoutedEventArgs e)
    {
        await SaveFileHelper.SaveFileAs(tab, this.window);
        textStatusBar.UpdateFile();
    }
    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        await SaveFileHelper.Save(tab, this.window);
        textStatusBar.UpdateFile();
    }

    private async void Rename()
    {
        if (await RenameFileDialog.Show(tab, this.XamlRoot))
        {
            textStatusBar.UpdateFile();
        }
    }

    private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        var ctrl = KeyHelper.IsKeyPressed(Windows.System.VirtualKey.Control);
        var shift = KeyHelper.IsKeyPressed(Windows.System.VirtualKey.Shift);
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
                case Windows.System.VirtualKey.K:
                    CompactOverlay_Click(null, null);
                    break;
                case Windows.System.VirtualKey.D:
                    EditActions.DuplicateLine(tab);
                    break;
                case Windows.System.VirtualKey.F:
                    searchControl.ShowSearch(this.tab);
                    break;
                case Windows.System.VirtualKey.R:
                    searchControl.ShowReplace(this.tab);
                    break;
                case Windows.System.VirtualKey.Escape:
                    searchControl.Close();
                    break;
            }
            return;
        }

        if (e.Key == Windows.System.VirtualKey.F11)
        {
            Fullscreen_Click(null, null);
        }

        if (e.Key == Windows.System.VirtualKey.F2)
        {
            Rename();
        }
    }

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

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        window.Close();
    }

    private void Toggle_TopMost_Click(object sender, RoutedEventArgs e)
    {
        WindowHelper.ToggleTopMost(window);
    }

    private void OpenSearch_Click(object sender, RoutedEventArgs e)
    {
        searchControl.ShowSearch(tab);
    }

    private void OpenReplace_Click(object sender, RoutedEventArgs e)
    {
        searchControl.ShowReplace(tab);
    }

    private void ToggleReadOnlyMode_Click(object sender, RoutedEventArgs e)
    {
        tab.IsReadOnly = readonlyModeFlyoutItem.IsChecked = !tab.IsReadOnly;

        UpdateTabIcon();
    }
}
