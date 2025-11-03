using Fastedit.Controls;
using Fastedit.Dialogs;
using Fastedit.Helper;
using Fastedit.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.IO;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using TextControlBoxNS;
using Fastedit.Core.Settings;
using Fastedit.Core.Tab;
using System.Threading.Tasks;

namespace Fastedit;

public sealed partial class MainPage : Page
{
    TabDatabase tabdatabase = new TabDatabase();
    public TabPageItem currentlySelectedTabPage = null;
    bool FirstLoaded = false;
    bool TabsLoaded = false;
    public FrameworkElement[] ControlsToHideInSettings;
    ProgressWindowItem progressWindow;
    public VerticalTabsFlyoutControl verticalTabsFlyout = null;
    private SplitButton addTabButton = null;
    private SurroundWithFlyout surroundWithFlyout = new SurroundWithFlyout();
    public TextStatusBar TextStatusBar => textStatusBar;
    public TabView tabView => this.tabControl;
    public QuickAccessWindow RunCommandWindow => this.runCommandWindow;
    public Grid TitleBarGrid => this.customDragRegion;
    public SearchControl SearchControl => this.searchControl;
    public MainPage()
    {
        this.InitializeComponent();

        TabPageHelper.mainPage = this;
        if (progressWindow == null)
            progressWindow = new ProgressWindowItem(progressBar, progressInfo);

        //Enable auto save database
        AutoSaveDatabaseHelper.RegisterSave();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        //events:
        this.PointerPressed += MainPage_PointerPressed;
        SettingsTabPageHelper.SettingsTabClosed += SettingsUpdater_SettingsTabClosed;
        App.m_window.AppWindow.Closing += AppWindow_Closing;

        if (!TabsLoaded)
        {
            TabsLoaded = true;
            progressBar.IsActive = true;

            Initialise();

            //apply the settings
            ApplySettings();

            // Load the database
            await TabPageHelper.LoadTabDatabase(tabControl, tabdatabase);

            //Handle activation from files on start and check whether a new tab needs to be created
            if (!await AppActivationHelper.HandleAppActivation(tabControl) && tabControl.TabItems.Count == 0)
            {
                TabPageHelper.AddNewTab(tabView, true);
            }
            SettingsUpdater.UpdateTabs(tabControl);

            //Show new-version info
            VersionHelper.CheckNewVersion();

            progressBar.IsActive = false;
        }
    }

    public async Task TriggerAppActivationAfterStart()
    {
        await AppActivationHelper.HandleAppActivation(tabControl);
    }

    private void MakeAppTitle(object selectedTab)
    {
        string title = "";
        if (selectedTab == null || tabControl.TabItems.Count == 0)
            title = "Fastedit";
        else if (selectedTab is TabPageItem tab)
            title = $"{tab.Header}";
        else if (SettingsTabPageHelper.IsSettingsPage(tabControl.SelectedItem))
            title = "Fastedit - Settings";
            App.m_window.Title = title;
    }
    private void Initialise()
    {
        if (FirstLoaded)
            return;

        FirstLoaded = true;

        //Create all needed folders when they don't exist:
        if (!Directory.Exists(DefaultValues.DatabasePath))
            Directory.CreateDirectory(DefaultValues.DatabasePath);
        if (!Directory.Exists(DefaultValues.DesignPath))
            Directory.CreateDirectory(DefaultValues.DesignPath);

        //copy the designs only on first start or when forced by user
        DesignHelper.CopyDefaultDesigns();

        //Add all the controls, that need to be hidden when in settings
        ControlsToHideInSettings = [mainMenubar, textStatusBar, runCommandWindow];

        //Create additinal controls:
        CreateMenubarFromLanguage();

        if (!AppSettings.FirstStart)
        {
            AppSettings.FirstStart = true;
            InfoMessages.WelcomeMessage();
        }
    }

    private void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (DesignWindowHelper.IsWindowOpen())
        {
            InfoMessages.CloseDesignEditor();
            args.Cancel = true;

            return;
        }

        SaveDatabase();
    }

    private void tabControl_TabDragStarting(TabView sender, TabViewTabDragStartingEventArgs args)
    {
        if (args.Tab == SettingsTabPageHelper.settingsPage)
        {
            args.Cancel = true;
            return;
        }

        args.Data.RequestedOperation = DataPackageOperation.Move;
    }
    private async void tabControl_TabDragCompleted(TabView sender, TabViewTabDragCompletedEventArgs args)
    {
        if (args.Tab == SettingsTabPageHelper.settingsPage)
            return;

        if (args.DropResult == DataPackageOperation.Move)
        {
            return; // Tab was reordered inside the tab control, no need to open a new window
        }


        await TabWindowHelper.ShowInNewWindow(tabControl, args.Tab as TabPageItem);
    }
    private void SettingsUpdater_SettingsTabClosed()
    {
        //Event when the settings page got closed
        ApplySettings();
    }

    private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        var ctrl = KeyHelper.IsKeyPressed(VirtualKey.Control);
        var shift = KeyHelper.IsKeyPressed(VirtualKey.Shift);
        var menu = KeyHelper.IsKeyPressed(VirtualKey.Menu);

        if (ctrl && e.Key== VirtualKey.Tab)
            TabPageHelper.SelectNextTab(tabControl);
        else if (ctrl && shift && e.Key== VirtualKey.Tab)
            TabPageHelper.SelectPreviousTab(tabControl);
        else if (ctrl && shift && e.Key== VirtualKey.P)
        {
            if (SettingsTabPageHelper.IsSettingsPage(tabControl.SelectedItem))
                return;
            runCommandWindow.Toggle(tabControl);
        }

        if (ctrl)
        {
            if (menu)
            {
                switch (e.Key)
                {
                    case VirtualKey.R:
                        ApplySettings();
                        return;
                    default:
                        return;
                }
            }

            switch (e.Key)
            {
                case VirtualKey.N:
                    NewFile_Click(null, null);
                    break;
                case VirtualKey.O:
                    OpenFile_Click(null, null);
                    break;
                case VirtualKey.S:
                    if (shift)
                        SaveFileAs_Click(null, null);
                    else
                        SaveFile_Click(null, null);
                    break;
                case VirtualKey.F:
                    Search_Click(null, null);
                    break;
                case VirtualKey.R:
                    Replace_Click(null, null);
                    break;
                case VirtualKey.G:
                    GoToLine_Click(null, null);
                    break;
                case VirtualKey.Add:
                    ZoomIn_Click(null, null);
                    break;
                case VirtualKey.Subtract:
                    ZoomOut_Click(null, null);
                    break;
                case VirtualKey.I:
                    FileInfo_Click(null, null);
                    break;
                case VirtualKey.B:
                    if (SettingsTabPageHelper.IsSettingsPage(tabControl.SelectedItem))
                        return;
                    runCommandWindow.Toggle(tabControl);
                    break;
                case VirtualKey.W:
                    CloseTab_Click(null, null);
                    break;
                case VirtualKey.T:
                    AddTabButton_Click(null, null);
                    break;
                case VirtualKey.E:
                    ChangeEncoding_Click(null, null);
                    break;
                case VirtualKey.L:
                    UndockTab_Click(null, null);
                    break;
                case VirtualKey.K:
                    CompactOverlayMode_Click(null, null);
                    break;
                case VirtualKey.D:
                    DuplicateLine_Click(null, null);
                    break;
                case VirtualKey.M:
                    verticalTabsFlyout.Show(addTabButton);
                    break;
            }
        }

        switch (e.Key)
        {
            case VirtualKey.F1:
                Settings_Click(null, null);
                break;
            case VirtualKey.F11:
                Fullscreen_Click(null, null);
                break;
            case VirtualKey.Escape:
                searchControl.Close();
                break;
        }
    }
    private void MainPage_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        //Dismiss the RunCommandWindow
        var pos = e.GetCurrentPoint(runCommandWindow).Position;
        if (pos.X < 0 || pos.Y < 0 || pos.X > runCommandWindow.ActualWidth || pos.Y > runCommandWindow.ActualHeight)
            runCommandWindow.Hide();

        //Navigate back and forward using mousebutton 4 and 5
        if (e.GetCurrentPoint(sender as UIElement).Properties.IsXButton1Pressed)
            TabPageHelper.SelectPreviousTab(tabControl);
        else if (e.GetCurrentPoint(sender as UIElement).Properties.IsXButton2Pressed)
            TabPageHelper.SelectNextTab(tabControl);
    }

    //TabControl
    private void AddTabButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
    {
        TabPageHelper.AddNewTab(tabControl, true);

        verticalTabsFlyout?.UpdateFlyoutIfOpen();
    }
    private async void TabView_CloseTabButtonClick(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        await TabPageHelper.CloseTab(tabControl, args.Item);
        verticalTabsFlyout?.UpdateFlyoutIfOpen();
    }
    private void TabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (searchControl.searchOpen && tabControl.SelectedItem == null)
            searchControl.Visibility = Visibility.Collapsed;

        MakeAppTitle(tabControl.SelectedItem);

        if (tabControl.SelectedItem is TabPageItem tab)
        {
            textStatusBar.tabPage = currentlySelectedTabPage = tab;
            if (tab == null)
                return;

            SettingsTabPageHelper.SettingsSelected = false;
            
            TabPageHelper.LoadUnloadedTab(tab, progressWindow);

            //set the focus to the textbox:
            tab.textbox.Focus(FocusState.Programmatic);
            searchControl.Visibility = ConvertHelper.BoolToVisibility(searchControl.searchOpen && searchControl.currentTab == tab);

            textStatusBar.UpdateAll();
        }
        else if (SettingsTabPageHelper.IsSettingsPage(tabControl.SelectedItem))
        {
            if (searchControl.searchOpen)
                searchControl.Visibility = Visibility.Collapsed;

            currentlySelectedTabPage = null;
            SettingsTabPageHelper.SettingsSelected = true;
        }

        SettingsUpdater.SetControlsVisibility(tabControl, mainMenubar, textStatusBar);

        textStatusBar.IsEnabled = tabControl.TabItems.Count > 0;

        if (tabControl.TabItems.Count == 0)
        {
            currentlySelectedTabPage = null;
        }

        currentlySelectedTabPage?.textbox?.Focus(FocusState.Programmatic);
    }

    //Drag drop
    private async void Page_Drop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var files = await e.DataView.GetStorageItemsAsync();
            await TabPageHelper.OpenFiles(tabControl, files);
        }
    }
    private void Page_DragOver(object sender, DragEventArgs e)
    {
        //only accept files
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }
    }

    //File
    private void NewFile_Click(object sender, RoutedEventArgs e)
    {   
        TabPageHelper.AddNewTab(tabControl, true);
    }
    private async void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        await TabPageHelper.OpenFile(tabControl);
    }
    private async void SaveFile_Click(object sender, RoutedEventArgs e)
    {
        await TabPageHelper.SaveFile(currentlySelectedTabPage);
    }
    private async void SaveFileAs_Click(object sender, RoutedEventArgs e)
    {
        await TabPageHelper.SaveFileAs(currentlySelectedTabPage);
    }
    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        SettingsTabPageHelper.OpenSettings(this, tabControl);
    }
    private async void RecycleBin_Click(object sender, RoutedEventArgs e)
    {
        await new RecycleBinDialog().ShowAsync(tabControl);
    }
    private async void SaveAll_Click(object sender, RoutedEventArgs e)
    {
        await TabPageHelper.SaveAll(tabControl);
    }
    //Edit
    private void Undo_Click(object sender, RoutedEventArgs e)
    {
        EditActions.Undo(currentlySelectedTabPage);
    }
    private void Redo_Click(object sender, RoutedEventArgs e)
    {
        EditActions.Redo(currentlySelectedTabPage);
    }
    private void Cut_Click(object sender, RoutedEventArgs e)
    {
        EditActions.Cut(currentlySelectedTabPage);
    }
    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        EditActions.Copy(currentlySelectedTabPage);
    }
    private void Paste_Click(object sender, RoutedEventArgs e)
    {
        EditActions.Paste(currentlySelectedTabPage);
    }
    private void SelectAll_Click(object sender, RoutedEventArgs e)
    {
        EditActions.SelectAll(currentlySelectedTabPage);
    }
    private void DuplicateLine_Click(object sender, RoutedEventArgs e)
    {
        EditActions.DuplicateLine(currentlySelectedTabPage);
    }
    private void Search_Click(object sender, RoutedEventArgs e)
    {
        if (currentlySelectedTabPage == null)
            return;

        searchControl.ShowSearch(currentlySelectedTabPage);
    }
    private void Replace_Click(object sender, RoutedEventArgs e)
    {
        if (currentlySelectedTabPage == null)
            return;

        searchControl.ShowReplace(currentlySelectedTabPage);
    }
    private async void GoToLine_Click(object sender, RoutedEventArgs e)
    {
        await GoToLineDialog.Show(currentlySelectedTabPage);
    }

    //Document
    private async void CloseTab_Click(object sender, RoutedEventArgs e)
    {
        await TabPageHelper.CloseTab(tabControl, currentlySelectedTabPage);
        verticalTabsFlyout?.UpdateFlyoutIfOpen();
    }
    private async void FileInfo_Click(object sender, RoutedEventArgs e)
    {
        await FileInfoDialog.Show(currentlySelectedTabPage);
    }
    private async void ChangeEncoding_Click(object sender, RoutedEventArgs e)
    {
        await EncodingDialog.Show(currentlySelectedTabPage);
        textStatusBar.UpdateEncoding();
    }
    private void SyntaxHighlighting_Click(object sender, RoutedEventArgs e)
    {
        if (currentlySelectedTabPage == null)
            return;

        if (sender is MenuFlyoutItem item)
        {
            currentlySelectedTabPage.HighlightLanguage = (SyntaxHighlightID)ConvertHelper.ToInt(item.Tag);
        }
        else if (sender is QuickAccessWindowItem rcwitem)
        {
            currentlySelectedTabPage.HighlightLanguage = (SyntaxHighlightID)ConvertHelper.ToInt(rcwitem.Tag);
        }
    }

    private async void UndockTab_Click(object sender, RoutedEventArgs e)
    {
        await TabWindowHelper.ShowInNewWindow(tabControl, currentlySelectedTabPage);
    }

    //View
    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        EditActions.ZoomIn(currentlySelectedTabPage);
    }
    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        EditActions.ZoomOut(currentlySelectedTabPage);
    }
    private void TabSpaces_Click(object sender, RoutedEventArgs e)
    {
        if (currentlySelectedTabPage == null)
            return;

        if (sender is MenuFlyoutItem item)
        {
            TabPageHelper.TabsOrSpaces(tabControl, item.Tag);
        }
        else if (sender is QuickAccessWindowItem runitem)
        {
            TabPageHelper.TabsOrSpaces(tabControl, runitem.Tag);
        }
    }
    private void Fullscreen_Click(object sender, RoutedEventArgs e)
    {
        WindowHelper.ToggleFullscreen(App.m_window);
    }
    private void CompactOverlayMode_Click(object sender, RoutedEventArgs e)
    {
        WindowHelper.ToggleCompactOverlay(App.m_window);
    }
    private void ShowRunCommandWindow_Click(object sender, RoutedEventArgs e)
    {
        runCommandWindow.Toggle(tabControl);
    }
    private void ReloadSettings_Click(object sender, RoutedEventArgs e)
    {
        ApplySettings();
    }

    private void AddButton_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is SplitButton btn)
        {
            addTabButton = btn;
        }
    }

    private async void RenameFile_Click(object sender, RoutedEventArgs e)
    {
        if (SettingsTabPageHelper.SettingsSelected && currentlySelectedTabPage != null)
            return;

        await RenameFileDialog.Show(currentlySelectedTabPage);
        textStatusBar.UpdateFile();
    }
    private async void CloseAll_Click(object sender, RoutedEventArgs e)
    {
        await TabPageHelper.CloseAll(tabControl);
    }

    public void ApplySettings(FasteditDesign currentDesign = null)
    {
        SettingsUpdater.UpdateSettings(this, tabControl, mainMenubar, textStatusBar, currentDesign);
    }
    private void CreateMenubarFromLanguage()
    {
        //items already added
        if (CodeLanguageSelector.Items.Count > 1)
            return;

        foreach (var item in TextControlBox.SyntaxHighlightings)
        {
            var menuItem = new MenuFlyoutItem
            {
                Text = item.Value == null ? "None" : item.Value.Name,
                Tag = item.Key.GetHashCode(),
            };
            menuItem.Click += SyntaxHighlighting_Click;
            CodeLanguageSelector.Items.Add(menuItem);

            var runCommandWindowItem = new QuickAccessWindowItem
            {
                Command = item.Value == null ? "None" : item.Value.Name,
                Tag = item.Key.GetHashCode(),
            };
            runCommandWindowItem.RunCommandWindowItemClicked += SyntaxHighlighting_Click;
            RunCommandWindowItem_SyntaxHighlighting.Items.Add(runCommandWindowItem);
        }
    }

    private void RunCommandWindow_ChangeDesign_Click(object sender, RoutedEventArgs e)
    {
        AppSettings.CurrentDesign = (sender as QuickAccessWindowItem).Tag.ToString();
        ApplySettings();
    }
    private void RunCommandWindowItem_Designs_ItemSelected()
    {
        RunCommandWindowItem_Designs.Items.Clear();
        foreach (var path in DesignHelper.GetDesignsFilesFromFolder())
        {
            var runCommandWindowItem = new QuickAccessWindowItem
            {
                Command = DesignHelper.GetDesignNameFromPath(path),
                Tag = path,
                TextColor = runCommandWindow.textColor,
            };
            runCommandWindowItem.RunCommandWindowItemClicked += RunCommandWindow_ChangeDesign_Click;
            RunCommandWindowItem_Designs.Items.Add(runCommandWindowItem);
        }
    }
    private void RunCommandWindowItem_Designs_SelectedChanged(IQuickAccessWindowItem item)
    {
        AppSettings.CurrentDesign = item.Tag.ToString();
        ApplySettings();
    }

    private void RunCommandWindowItem_SytaxHighlighting_SelectedChanged(IQuickAccessWindowItem item)
    {
        SyntaxHighlighting_Click(item, null);
    }
    public void SelectedTabChanged()
    {
        TabView_SelectionChanged(tabControl, null);
    }
    public void ChangeSelectedTab(TabPageItem tab)
    {
        tabControl.SelectedItem = tab;
    }
    public void SaveDatabase(bool ShowProgress = true, bool closeWindows = true)
    {
        TabPageHelper.SaveTabDatabase(tabdatabase, tabControl, ShowProgress ? progressWindow : null, closeWindows);
    }
    public void ShowSettings(string page = null)
    {
        SettingsTabPageHelper.OpenSettings(this, tabControl, page);
    }
    private void runCommandWindow_Closed()
    {
        if(currentlySelectedTabPage != null && currentlySelectedTabPage.textbox != null)
            currentlySelectedTabPage.textbox.Focus(FocusState.Programmatic);
    }

    private void SurroundWith_Click(object sender, RoutedEventArgs e)
    {
        if (currentlySelectedTabPage != null && currentlySelectedTabPage.textbox != null)
            surroundWithFlyout.ShowFlyout(currentlySelectedTabPage.textbox);
    }

    private void Toggle_TopMost_Click(object sender, RoutedEventArgs e)
    {
        WindowHelper.ToggleTopMost(App.m_window);
    }

    private async void SupportDevelopment_Click(object sender, RoutedEventArgs e)
    {
        await Windows.System.Launcher.LaunchUriAsync(new Uri("https://www.paypal.com/donate/?hosted_button_id=Q7YWPMBV6YNCQ"));

    }
}
