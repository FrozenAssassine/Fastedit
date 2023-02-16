using Fastedit.Controls;
using Fastedit.Dialogs;
using Fastedit.Helper;
using Fastedit.Settings;
using Fastedit.Storage;
using Fastedit.Tab;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Fastedit
{
    public sealed partial class MainPage : Page
    {
        TitlebarHelper titlebarhelper;
        TabDatabase tabdatabase = new TabDatabase();
        public TabPageItem currentlySelectedTabPage = null;
        bool FirstLoaded = false;
        bool TabsLoaded = false;
        public List<FrameworkElement> ControlsToHideInSettings = new List<FrameworkElement>();
        ProgressWindowItem progressWindow;
        public VerticalTabsFlyoutControl verticalTabsFlyout = null;
        public TabView tabView => tabControl;

        public MainPage()
        {
            this.InitializeComponent();

            TabPageHelper.mainPage = this;
            InfoMessages.InfoMessagePanel = infobarDisplay;

            //events:
            this.PointerPressed += MainPage_PointerPressed;
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += MainPage_CloseRequested;
            SettingsTabPageHelper.SettingsTabClosed += SettingsUpdater_SettingsTabClosed;

            //classes
            if (titlebarhelper == null)
                titlebarhelper = new TitlebarHelper(CustomDragRegion, ShellTitlebarInset, FlowDirection);

            if (progressWindow == null)
                progressWindow = new ProgressWindowItem(progressBar, progressInfo);

            //Enable auto save database
            AutoDatabaseSaveHelper.RegisterSave();
        }

        //A function, that can be called from anywhere to update the settings
        public void ApplySettings(FasteditDesign currentDesign = null)
        {
            SettingsUpdater.UpdateSettings(this, tabControl, mainMenubar, statusbar, currentDesign);
        }
        private async Task Initialise()
        {
            if (!FirstLoaded)
            {
                FirstLoaded = true;

                //Create all needed folders when they don't exist:
                if (!Directory.Exists(DefaultValues.DatabasePath))
                    Directory.CreateDirectory(DefaultValues.DatabasePath);
                if (!Directory.Exists(DefaultValues.DesignPath))
                    Directory.CreateDirectory(DefaultValues.DesignPath);

                //copy the designs only on first start or when forced by user
                await DesignHelper.CopyDefaultDesigns();

                await DesignHelper.LoadDesign();

                //Add all the controls, that need to be hidden when in settings
                ControlsToHideInSettings.Add(mainMenubar);
                ControlsToHideInSettings.Add(statusbar);
                ControlsToHideInSettings.Add(searchControl);
                ControlsToHideInSettings.Add(runCommandWindow);

                //Create additinal controls:
                CreateMenubarFromLanguage();

                titlebarhelper.SetTitlebar();

                if (AppSettings.GetSettings(AppSettingsValues.App_FirstStart).Length == 0)
                {
                    AppSettings.SaveSettings(AppSettingsValues.App_FirstStart, "L");
                    InfoMessages.WelcomeMessage();
                }
            }
            ApplySettings();
        }
        private void CreateMenubarFromLanguage()
        {
            //items already added
            if (CodeLanguageSelector.Items.Count > 1)
                return;

            try
            {
                foreach (var item in TextControlBox.TextControlBox.CodeLanguages)
                {
                    var menuItem = new MenuFlyoutItem
                    {
                        Text = item.Value.Name,
                        Tag = item.Key,
                    };
                    menuItem.Click += CodeLanguage_Click;
                    CodeLanguageSelector.Items.Add(menuItem);

                    var runCommandWindowItem = new RunCommandWindowItem
                    {
                        Command = item.Value.Name,
                        Tag = item.Key,
                    };
                    runCommandWindowItem.RunCommandWindowItemClicked += CodeLanguage_Click;
                    RunCommandWindowItem_CodeLanguages.Items.Add(runCommandWindowItem);
                }
            }
            catch
            {

            }

            var noneItem = new MenuFlyoutItem
            {
                Text = "None",
                Tag = "",
            };
            noneItem.Click += CodeLanguage_Click;
            CodeLanguageSelector.Items.Add(noneItem);

            var noneCmdWindowItem = new RunCommandWindowItem
            {
                Command = "None",
                Tag = "",
            };
            noneCmdWindowItem.RunCommandWindowItemClicked += CodeLanguage_Click;
            RunCommandWindowItem_CodeLanguages.Items.Add(noneCmdWindowItem);
        }
        public void UpdateStatubar()
        {
            if (currentlySelectedTabPage == null || statusbar.Visibility == Visibility.Collapsed)
                return;

            Statusbar_Column.ChangingText = currentlySelectedTabPage.textbox.CursorPosition.CharacterPosition.ToString();
            Statusbar_Line.ChangingText = (currentlySelectedTabPage.textbox.CursorPosition.LineNumber + 1).ToString();
            Statusbar_FilePath.ChangingText = currentlySelectedTabPage.DatabaseItem.FileName.ToString();
            Statusbar_Zoom.ChangingText = currentlySelectedTabPage.textbox.ZoomFactor + "%";
            Statusbar_Encoding.ChangingText = EncodingHelper.GetEncodingName(currentlySelectedTabPage.Encoding);
        }
        public void SelectedTabChanged()
        {
            TabView_SelectionChanged(tabControl, null);
        }
        public void ChangeSelectedTab(TabPageItem tab)
        {
            tabControl.SelectedItem = tab;
        }
        public async Task SaveDatabase(bool ShowProgress = true, bool closeWindows = true)
        {
            await TabPageHelper.SaveTabDatabase(tabdatabase, tabControl, ShowProgress ? progressWindow : null, closeWindows);
        }
        public void ShowSettings(string page = null)
        {
            SettingsTabPageHelper.OpenSettings(this, tabControl, page);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Save back the value and read from it after all tabs are loaded
            AppActivationHelper.NavigationEvent = e;

            //handle activation from files and commandline after the tabs are already loaded
            if (TabsLoaded)
            {
                await AppActivationHelper.HandleAppActivation(tabControl);
            }

            base.OnNavigatedTo(e);
        }
        private async void tabControl_TabDragStarting(TabView sender, TabViewTabDragStartingEventArgs args)
        {
            if (currentlySelectedTabPage == null)
                return;

            await SaveFileHelper.DragFileToPath(currentlySelectedTabPage, args);
        }
        private void SettingsUpdater_SettingsTabClosed()
        {
            //Event when the settings page got closed
            ApplySettings();
        }
        private async void MainPage_CloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            var deferral = e.GetDeferral();

            if (DesignWindowHelper.IsWindowOpen())
            {
                InfoMessages.CloseDesignEditor();
                e.Handled = true;
                deferral.Complete();
                return;
            }

            await SaveDatabase();

            deferral.Complete();
        }
        private async void CustomDragRegion_Loaded(object sender, RoutedEventArgs e)
        {
            if (!TabsLoaded)
            {
                TabsLoaded = true;

                progressBar.IsActive = true;

                await Initialise();

                //load the database
                await TabPageHelper.LoadTabDatabase(tabControl, tabdatabase);

                //handle activation from files and commandline on start
                await AppActivationHelper.HandleAppActivation(tabControl);

                //apply the settings
                SettingsUpdater.UpdateSettings(this, tabControl, mainMenubar, statusbar, DesignHelper.CurrentDesign);

                //Show new-version info
                VersionHelper.CheckNewVersion();

                progressBar.IsActive = false;
            }
        }
        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            var ctrl = Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            var shift = Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            var menu = Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Menu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

            if (ctrl && args.VirtualKey == Windows.System.VirtualKey.Tab)
                TabPageHelper.SelectNextTab(tabControl);
            else if (ctrl && shift && args.VirtualKey == Windows.System.VirtualKey.Tab)
                TabPageHelper.SelectPreviousTab(tabControl);

            if (ctrl)
            {
                if (menu)
                {
                    switch (args.VirtualKey)
                    {
                        case VirtualKey.R:
                            ApplySettings();
                            return;
                        default:
                            return;
                    }
                }

                switch (args.VirtualKey)
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
                        if (SettingsTabPageHelper.SettingsPageOpen)
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
                        ShowTabInNewWindow_Click(null, null);
                        break;
                    case VirtualKey.K:
                        CompactOverlayMode_Click(null, null);
                        break;
                    case VirtualKey.D:
                        DuplicateLine_Click(null, null);
                        break;
                }
            }

            switch (args.VirtualKey)
            {
                case VirtualKey.F1:
                    Settings_Click(null, null);
                    break;
                case VirtualKey.F11:
                    Fullscreen_Click(null, null);
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
        private void AddTabButton_Click(Microsoft.UI.Xaml.Controls.SplitButton sender, Microsoft.UI.Xaml.Controls.SplitButtonClickEventArgs args)
        {
            TabPageHelper.AddNewTab(tabControl, true);

            verticalTabsFlyout?.UpdateFlyoutIfOpen();
        }
        private async void TabView_CloseTabButtonClick(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            await TabPageHelper.CloseTab(tabControl, args.Item);
            verticalTabsFlyout?.UpdateFlyoutIfOpen();
        }
        private async void TabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl.SelectedItem is TabPageItem tab)
            {
                currentlySelectedTabPage = tab;
                if (tab == null)
                    return;

                SettingsTabPageHelper.SettingsSelected = false;

                UpdateStatubar();

                //set the focus to the textbox:
                tab.textbox.Focus(FocusState.Programmatic);

                await TabPageHelper.LoadUnloadedTab(tab, progressWindow);
            }
            else if (SettingsTabPageHelper.IsSettingsPage(tabControl.SelectedItem))
            {
                currentlySelectedTabPage = null;
                SettingsTabPageHelper.SettingsSelected = true;
                SettingsTabPageHelper.HideControls();
                return;
            }

            if (tabControl.TabItems.Count == 0)
                currentlySelectedTabPage = null;

            //show hidden controls
            if (!SettingsTabPageHelper.SettingsSelected)
            {
                SettingsUpdater.SetControlsVisibility(tabControl, mainMenubar, statusbar);
            }
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
            await new RecycleBinDialog(tabControl).ShowDialog();
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
        private void CodeLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (currentlySelectedTabPage == null)
                return;

            if (sender is MenuFlyoutItem item)
            {
                if (item != null && item.Tag != null)
                {
                    if (item.Tag.ToString().Length == 0)
                        currentlySelectedTabPage.CodeLanguage = null;

                    currentlySelectedTabPage.CodeLanguage = TextControlBox.TextControlBox.GetCodeLanguageFromId(item.Tag.ToString());
                }
            }
            else if (sender is RunCommandWindowItem rcwitem)
            {
                if (rcwitem != null && rcwitem.Tag != null)
                {
                    if (rcwitem.Tag.ToString().Length == 0)
                        currentlySelectedTabPage.CodeLanguage = null;

                    currentlySelectedTabPage.CodeLanguage = TextControlBox.TextControlBox.GetCodeLanguageFromId(rcwitem.Tag.ToString());
                }
            }
        }
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (currentlySelectedTabPage == null)
                return;

            searchControl.ShowSearch(currentlySelectedTabPage.textbox);
        }
        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            if (currentlySelectedTabPage == null)
                return;

            searchControl.ShowReplace(currentlySelectedTabPage.textbox);
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
            UpdateStatubar();
        }
        private void ShareDocument_Click(object sender, RoutedEventArgs e)
        {
            ShareDialog.Share(currentlySelectedTabPage);
        }
        private async void ShowTabInNewWindow_Click(object sender, RoutedEventArgs e)
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
            else if (sender is RunCommandWindowItem runitem)
            {
                TabPageHelper.TabsOrSpaces(tabControl, runitem.Tag);
            }
        }
        private void Fullscreen_Click(object sender, RoutedEventArgs e)
        {
            WindowHelper.ToggleFullscreen();
        }
        private void CompactOverlayMode_Click(object sender, RoutedEventArgs e)
        {
            WindowHelper.ToggleCompactOverlay();
        }
        private void ShowRunCommandWindow_Click(object sender, RoutedEventArgs e)
        {
            runCommandWindow.Toggle(tabControl);
        }

        private void Statusbar_GoToLineTextbox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                int res = ConvertHelper.ToInt(Statusbar_GoToLineTextbox.Text, -1) - 1;
                if (res == -1)
                    return;

                EditActions.GoToLine(currentlySelectedTabPage, res);
                Statusbar_Line.HideFlyout();
            }
        }
    }
}
