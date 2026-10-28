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
using Fastedit.Core.Storage;
using Fastedit.Core.Tab;
using System.Collections.Generic;
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

            if (currentlySelectedTabPage != null)
            {
                SelectTabSpacesMenubarItem();
                SelectLineEndingMenubarItem();
            }

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
            title = Multilingual.L("T_FasteditSettings_2");
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

        //load line ending items to menubar!
        LineEndingHelper.MakeAndAddLineEndingItems(LineEndingMenubarFlyout, LineEnding_Click);
        LineEndingHelper.MakeAndAddLineEndingItems(RunCommandWindowItem_LineEndings, LineEnding_Click);

        if (!AppSettings.FirstStart)
        {
            AppSettings.FirstStart = true;
            InfoMessages.WelcomeMessage();
        }
    }

    public void SelectTabSpacesMenubarItem()
    {
        TabsSpacesHelper.SelectToggleMenuItemsFromMenu(TabKeyBehaviourFlyout, currentlySelectedTabPage);
    }
    public void SelectLineEndingMenubarItem()
    {
        LineEndingHelper.SelectItems(LineEndingMenubarFlyout.Items, currentlySelectedTabPage);
    }

    private void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (DesignWindowHelper.IsWindowOpen())
        {
            InfoMessages.CloseDesignEditor();
            args.Cancel = true;

            return;
        }

        if (!SaveDatabase())
        {
            args.Cancel = true;
        }
        else
        {
            FileChangeManager.StopAll();
        }
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

        TabWindowHelper.ShowInNewWindow(tabControl, args.Tab as TabPageItem);
    }
    private void SettingsUpdater_SettingsTabClosed()
    {
        //Event when the settings page got closed
        ApplySettings();
    }

    private void Page_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        //handle in preview, otherwise the textbox consumes the tab key
        if (e.Key != VirtualKey.Tab || !KeyHelper.IsKeyPressed(VirtualKey.Control))
            return;

        if (KeyHelper.IsKeyPressed(VirtualKey.Shift))
            TabPageHelper.SelectPreviousTab(tabControl, true);
        else
            TabPageHelper.SelectNextTab(tabControl, true);
        e.Handled = true;
    }

    private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        var ctrl = KeyHelper.IsKeyPressed(VirtualKey.Control);
        var shift = KeyHelper.IsKeyPressed(VirtualKey.Shift);
        var menu = KeyHelper.IsKeyPressed(VirtualKey.Menu);

        if (ctrl && shift && e.Key== VirtualKey.P)
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
                case VirtualKey.H:
                    Replace_Click(null, null);
                    break;
                case VirtualKey.G:
                    GoToLine_Click(null, null);
                    break;
                case VirtualKey.Add:
                case (VirtualKey)187: //OemPlus, the + key on the main keyboard
                    ZoomIn_Click(null, null);
                    break;
                case VirtualKey.Subtract:
                case (VirtualKey)189: //OemMinus
                    ZoomOut_Click(null, null);
                    break;
                case VirtualKey.Number0:
                case VirtualKey.NumberPad0:
                    ResetZoom_Click(null, null);
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

            SelectTabSpacesMenubarItem();
            SelectLineEndingMenubarItem();

            //set the focus to the textbox:
            tab.textbox.Focus(FocusState.Programmatic);
            searchControl.Visibility = ConvertHelper.BoolToVisibility(searchControl.searchOpen && searchControl.currentTab == tab);

            VisualizeWhitespaceCharacters_MenuFlyoutItem.IsChecked = tab.GetEffectiveWhitespaceSetting();
            textboxReadonlyMenubarItem.IsChecked = currentlySelectedTabPage.IsReadOnly;
            WordWrapMenuIndicator.IsChecked = currentlySelectedTabPage.WordWrap;

            textStatusBar.UpdateAll();

            FileChangeManager.CheckTabForExternalChanges(tab);
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
        TabWindowHelper.ShowInNewWindow(tabControl, currentlySelectedTabPage);
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
    private void ResetZoom_Click(object sender, RoutedEventArgs e)
    {
        EditActions.ResetZoom(currentlySelectedTabPage);
    }
    private void TabSpaces_Click(object sender, RoutedEventArgs e)
    {
        if (currentlySelectedTabPage == null)
            return;

        if (sender is ToggleMenuFlyoutItem item)
        {
            //only set this per current document
            currentlySelectedTabPage.SetTabsSpaces(ConvertHelper.ToInt(item.Tag, -1));
        }
        else if (sender is QuickAccessWindowItem runitem)
        {
            //only set this per current document
            currentlySelectedTabPage.SetTabsSpaces(ConvertHelper.ToInt(runitem.Tag, -1));
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
        ReloadLanguage();
        SettingsUpdater.UpdateSettings(this, tabControl, mainMenubar, textStatusBar, currentDesign);
    }

    public void ReloadLanguage()
    {
        Multilingual.Reload();

        if (mainMenubar?.Items?.Count >= 5)
        {
            // 1. File
            var fileMenu = mainMenubar.Items[0];
            fileMenu.Title = Multilingual.L("MainPage_MenuBarItem_1.Title");
            UpdateMenuItem(fileMenu.Items, 0, "MainPage_MenuFlyoutItem_1.Text"); // New
            UpdateMenuItem(fileMenu.Items, 1, "MainPage_MenuFlyoutItem_2.Text"); // Open
            UpdateMenuItem(fileMenu.Items, 3, "MainPage_MenuFlyoutItem_3.Text"); // Save
            UpdateMenuItem(fileMenu.Items, 4, "MainPage_MenuFlyoutItem_4.Text"); // Save As
            UpdateMenuItem(fileMenu.Items, 5, "MainPage_MenuFlyoutItem_5.Text"); // Save All
            UpdateMenuItem(fileMenu.Items, 7, "MainPage_MenuFlyoutItem_6.Text"); // Recycle Bin
            UpdateMenuItem(fileMenu.Items, 9, "MainPage_MenuFlyoutItem_7.Text"); // Settings

            // 2. Edit
            var editMenu = mainMenubar.Items[1];
            editMenu.Title = Multilingual.L("MainPage_MenuBarItem_2.Title");
            UpdateMenuItem(editMenu.Items, 0, "MainPage_MenuFlyoutItem_8.Text");  // Undo
            UpdateMenuItem(editMenu.Items, 1, "MainPage_MenuFlyoutItem_9.Text");  // Redo
            UpdateMenuItem(editMenu.Items, 3, "MainPage_MenuFlyoutItem_10.Text"); // Cut
            UpdateMenuItem(editMenu.Items, 4, "MainPage_MenuFlyoutItem_11.Text"); // Copy
            UpdateMenuItem(editMenu.Items, 5, "MainPage_MenuFlyoutItem_12.Text"); // Paste
            UpdateMenuItem(editMenu.Items, 6, "MainPage_MenuFlyoutItem_13.Text"); // Duplicate Line
            UpdateMenuItem(editMenu.Items, 8, "MainPage_MenuFlyoutItem_14.Text"); // Search
            UpdateMenuItem(editMenu.Items, 9, "MainPage_MenuFlyoutItem_15.Text"); // Replace
            UpdateMenuItem(editMenu.Items, 10, "MainPage_MenuFlyoutItem_16.Text"); // Go To
            UpdateMenuItem(editMenu.Items, 11, "MainPage_MenuFlyoutItem_17.Text"); // Select All

            // 3. Document
            var docMenu = mainMenubar.Items[2];
            docMenu.Title = Multilingual.L("MainPage_MenuBarItem_3.Title");
            UpdateMenuItem(docMenu.Items, 0, "MainPage_ToggleMenuFlyoutItem_1.Text"); // Word wrap
            UpdateMenuItem(docMenu.Items, 1, "MainPage_MenuFlyoutItem_18.Text"); // File Info
            UpdateMenuItem(docMenu.Items, 2, "MainPage_MenuFlyoutItem_19.Text"); // Rename
            UpdateMenuItem(docMenu.Items, 3, "MainPage_MenuFlyoutItem_20.Text"); // Show in File Explorer
            UpdateMenuItem(docMenu.Items, 4, "MainPage_MenuFlyoutSubItem_1.Text"); // Line Ending
            UpdateMenuItem(docMenu.Items, 5, "MainPage_MenuFlyoutItem_21.Text"); // Encoding
            UpdateMenuItem(docMenu.Items, 7, "MainPage_MenuFlyoutItem_22.Text"); // Close
            UpdateMenuItem(docMenu.Items, 8, "MainPage_MenuFlyoutItem_23.Text"); // Close All
            UpdateMenuItem(docMenu.Items, 9, "MainPage_MenuFlyoutItem_24.Text"); // Undock Tab
            UpdateMenuItem(docMenu.Items, 11, "MainPage_ToggleMenuFlyoutItem_2.Text"); // Readonly
            UpdateMenuItem(docMenu.Items, 13, "MainPage_ToggleMenuFlyoutItem_3.Text"); // Visualize Whitespace

            // 4. Advanced
            var advMenu = mainMenubar.Items[3];
            advMenu.Title = Multilingual.L("MainPage_MenuBarItem_4.Title");
            UpdateMenuItem(advMenu.Items, 0, "MainPage_MenuFlyoutSubItem_2.Text"); // Syntax Highlighting
            if (advMenu.Items.Count > 2 && advMenu.Items[2] is MenuFlyoutSubItem tabBehaviour)
            {
                tabBehaviour.Text = Multilingual.L("MainPage_MenuFlyoutSubItem_3.Text");
                UpdateMenuItem(tabBehaviour.Items, 0, "MainPage_MenuFlyoutItem_25.Text"); // Auto Detect
                UpdateMenuItem(tabBehaviour.Items, 2, "MainPage_ToggleMenuFlyoutItem_4.Text"); // Use Tab
                UpdateMenuItem(tabBehaviour.Items, 4, "MainPage_ToggleMenuFlyoutItem_5.Text"); // 2 Spaces
                UpdateMenuItem(tabBehaviour.Items, 5, "MainPage_ToggleMenuFlyoutItem_6.Text"); // 4 Spaces
                UpdateMenuItem(tabBehaviour.Items, 6, "MainPage_ToggleMenuFlyoutItem_7.Text"); // 8 Spaces
            }
            if (advMenu.Items.Count > 3 && advMenu.Items[3] is MenuFlyoutSubItem reformatTabs)
            {
                reformatTabs.Text = Multilingual.L("MainPage_MenuFlyoutSubItem_4.Text");
                UpdateMenuItem(reformatTabs.Items, 0, "MainPage_MenuFlyoutItem_26.Text"); // Use Tab
                UpdateMenuItem(reformatTabs.Items, 2, "MainPage_MenuFlyoutItem_27.Text"); // 2 Spaces
                UpdateMenuItem(reformatTabs.Items, 3, "MainPage_MenuFlyoutItem_28.Text"); // 4 Spaces
                UpdateMenuItem(reformatTabs.Items, 4, "MainPage_MenuFlyoutItem_29.Text"); // 8 Spaces
            }
            UpdateMenuItem(advMenu.Items, 5, "MainPage_MenuFlyoutItem_30.Text"); // Surround With
            UpdateMenuItem(advMenu.Items, 7, "MainPage_MenuFlyoutItem_31.Text"); // Quick Access

            // 5. View
            var viewMenu = mainMenubar.Items[4];
            viewMenu.Title = Multilingual.L("MainPage_MenuBarItem_5.Title");
            UpdateMenuItem(viewMenu.Items, 0, "MainPage_MenuFlyoutItem_32.Text"); // Compact Overlay
            UpdateMenuItem(viewMenu.Items, 1, "MainPage_MenuFlyoutItem_33.Text"); // Fullscreen
            UpdateMenuItem(viewMenu.Items, 2, "MainPage_MenuFlyoutItem_34.Text"); // Always On Top
            UpdateMenuItem(viewMenu.Items, 4, "MainPage_MenuFlyoutItem_35.Text"); // Zoom In
            UpdateMenuItem(viewMenu.Items, 5, "MainPage_MenuFlyoutItem_36.Text"); // Zoom Out
            UpdateMenuItem(viewMenu.Items, 6, "MainPage_MenuFlyoutItem_37.Text"); // Reset Zoom
        }

        if (addTabButton != null)
        {
            ToolTipService.SetToolTip(addTabButton, Multilingual.L("MainPage_SplitButton_1.ToolTipService.ToolTip"));
        }

        searchControl?.ReloadLanguage();
        textStatusBar?.ReloadLanguage();
        MakeAppTitle(tabControl.SelectedItem);
    }

    private static void UpdateMenuItem(IList<MenuFlyoutItemBase> list, int index, string resourceKey)
    {
        if (index >= 0 && index < list.Count)
        {
            if (list[index] is MenuFlyoutItem item)
                item.Text = Multilingual.L(resourceKey);
            else if (list[index] is MenuFlyoutSubItem subItem)
                subItem.Text = Multilingual.L(resourceKey);
        }
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
                Text = item.Value == null ? Multilingual.L("T_None") : item.Value.Name,
                Tag = item.Key.GetHashCode(),
            };
            menuItem.Click += SyntaxHighlighting_Click;
            CodeLanguageSelector.Items.Add(menuItem);

            var runCommandWindowItem = new QuickAccessWindowItem
            {
                Command = item.Value == null ? Multilingual.L("T_None") : item.Value.Name,
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
    public bool SaveDatabase(bool ShowProgress = true, bool closeWindows = true)
    {
        return TabPageHelper.SaveTabDatabase(tabdatabase, tabControl, ShowProgress ? progressWindow : null, closeWindows);
    }
    public void BackupChangedTabs()
    {
        TabPageHelper.BackupChangedTabs(tabdatabase, tabControl);
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

    private void LineEnding_Click(LineEnding lineEnding)
    {
        if (currentlySelectedTabPage != null && currentlySelectedTabPage.textbox != null)
        {
            currentlySelectedTabPage.LineEnding = lineEnding;
        }
    }

    private void VisualizeWhitespaceCharacters_Click(object sender, RoutedEventArgs e)
    {
        if (currentlySelectedTabPage == null)
            return;

        //reset when the same as in the settings
        bool clickedValue = VisualizeWhitespaceCharacters_MenuFlyoutItem.IsChecked;
        if (clickedValue == AppSettings.ShowWhitespaceCharacters)
            currentlySelectedTabPage.ShowWhitespaceCharacters = null;
        else
            currentlySelectedTabPage.ShowWhitespaceCharacters = clickedValue;
    }

    private void ReformatTabsInDocument_Click(object sender, RoutedEventArgs e)
    {
        TabsSpacesHelper.RewriteTabsSpaces(currentlySelectedTabPage, sender);
    }

    private void AutodetectTabsSpaces_Click(object sender, RoutedEventArgs e)
    {
        if (currentlySelectedTabPage == null)
            return;

        (bool useSpacesInstead, int tabs) = currentlySelectedTabPage.textbox.DetectTabsSpaces();
        currentlySelectedTabPage.SetTabsSpaces(useSpacesInstead ? tabs : -1);
    }

    private void ToggleReadonly_Click(object sender, RoutedEventArgs e)
    {
        if (currentlySelectedTabPage == null)
            return;

        currentlySelectedTabPage.IsReadOnly = !currentlySelectedTabPage.IsReadOnly;
    }

    private void RevealTabInFileExplorer(object sender, RoutedEventArgs e)
    {
        if (currentlySelectedTabPage == null)
            return;
        FileExplorerHelper.OpenExplorerAtPath(currentlySelectedTabPage.DatabaseItem.FilePath);
    }

    private void WordWrap_Click(object sender, RoutedEventArgs e)
    {
        if (currentlySelectedTabPage == null)
            return;

        WordWrapMenuIndicator.IsChecked = currentlySelectedTabPage.WordWrap = !currentlySelectedTabPage.WordWrap;
    }
}
