using Fastedit.Controls.Textbox;
using Fastedit.Core;
using Fastedit.Core.Tab;
using Fastedit.Dialogs;
using Fastedit.Extensions;
using Fastedit.ExternalData;
using Fastedit.Helper;
using Fastedit.Views;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Convert = Fastedit.Extensions.Convert;
using MenuBarItem = Microsoft.UI.Xaml.Controls.MenuBarItem;
using muxc = Microsoft.UI.Xaml.Controls;
using StringBuilder = Fastedit.Extensions.StringBuilder;

namespace Fastedit
{
    public sealed partial class MainPage : Page
    {
        //Other Classes
        private TabActions tabactions = null;
        private readonly AppSettings appsettings = new AppSettings();
        private readonly SaveFileHelper savefilehelper = new SaveFileHelper();
        private readonly TabPageHelper tabpagehelper = new TabPageHelper();
        private readonly CustomDesigns customdesigns = null;

        //Settings entrys:
        private FontFamily TextBoxFontfamily = new FontFamily(DefaultValues.DefaultFontFamily);
        private int TextBoxFontSize = DefaultValues.DefaultFontsize;
        private Color TextBoxBackgroundcolor = Colors.Transparent;
        private Color TextColor = Colors.Transparent;
        private Color TitleBarBackgroundColor = Colors.Transparent;
        private Color TextSelectionColor = Colors.Transparent;
        private Color TabColorNotFocused = Colors.Transparent;
        private Color TabColorFocused = Colors.Transparent;
        private Color LineNumberForegroundColor = Colors.Transparent;
        private Color LineNumberBackgroundColor = Colors.Transparent;
        private Color StatusbarBackgroundColor = Colors.Transparent;
        private Color StatusbarForegroundColor = Colors.Transparent;
        private Color LineHighlighterForeground = Colors.Blue;
        private Color LineHighlighterBackground = Colors.Blue;
        private bool ShowLineNumbers = true;
        private bool ShowStatusBar = true;
        private bool IsHandWritingEnabled = DefaultValues.HandWritingEnabled;
        private bool ShowSelectionFlyout = false;
        private bool ShowMenubar
        {
            get => MainMenuBar != null;
            set
            {
                if (value && MainMenuBar == null)
                {
                    MainMenuBar = FindName("MainMenuBar") as muxc.MenuBar;
                }
                else if (value == false && MainMenuBar != null)
                {
                    UnloadObject(MainMenuBar);
                }
            }
        }
        private bool ShowLineHighlighter = true;

        //Variables
        private bool HasAlreadyNavigatedTo = false; 
        private bool SettingsWindowSelected = false;
        private bool preventZoomOnFactorChanged = false;
        private readonly DispatcherTimer newTabSaveTime = new DispatcherTimer();
        private TextControlBox CurrentlySelectedTabPage_Textbox = null;
        private muxc.TabViewItem CurrentlySelectedTabPage = null;
        private muxc.FontIconSource TabPageFontIconSource = null;
        private List<OpenedSecondaryViewItem> OpenedSecondaryViews = new List<OpenedSecondaryViewItem>();
        private muxc.TabViewItem SettingsTabPage = null;

        public MainPage()
        {
            this.InitializeComponent();

            if (tabactions == null)
                tabactions = new TabActions(TextTabControl, this);
            if(customdesigns == null)
                customdesigns = new CustomDesigns(null, this);

            //Subscribe to the events:
            SizeChanged += MainPage_SizeChanged;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += Application_OnCloseRequest;
            new ThemeListener().ThemeChanged += Application_ThemeChanged;
        }

        //Initialisation
        private async Task Initialisation()
        {
            //Only if application has started the very first time
            bool IsFirstStart = false;
            if (appsettings.GetSettingsAsBool("FirstStart", true))
            {
                IsFirstStart = true;
                await CopyThemesToFolder();
                appsettings.SaveSettings("FirstStart", false);
            }

            await SetSettings(false);

            //Prevent repeated call of this functions:
            if (HasAlreadyNavigatedTo == false)
            {
                HasAlreadyNavigatedTo = true;
                //Only load the tabs from the database
                if (appsettings.GetSettingsAsBool("LoadRecentTabs", DefaultValues.LoadRecentTabsOnStart))
                {
                    await tabactions.LoadTabs(false);
                }

                //Check if it is running on a new version and it is not the first start
                //If yes show the infobar which leads to the changelog
                if (IsOnNewVersion() && !IsFirstStart)
                    ShowNewVersionInfobar();
            }
        }
        private void AfterInitialisation()
        {
            SetTitlebar();
            ApplySettingsToAllTabPages();
            AutoBackupDataBaseTimer();
            TextTabControl_SelectionChanged(null, null);
            //Add the buttons with encoding to the Statusbar
            AddEncodingButtonsToStatusbar();
        }
        private async Task CopyThemesToFolder()
        {
            try
            {
                string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
                StorageFolder sourceFolder = await StorageFolder.GetFolderFromPathAsync($"{root}\\Assets\\Designs");

                var files = await sourceFolder.GetFilesAsync();
                for (int i = 0; i < files.Count; i++)
                {
                    StorageFile file = files[i];
                    if (file != null)
                    {
                        await file.CopyAsync(customdesigns.DesignsFolder, file.Name, NameCollisionOption.ReplaceExisting);
                    }
                }

                //Decide to either load a theme with Mica or Acrylic  
                if(VersionHelper.GetWindowsVersion() == WindowsVersion.Windows11)
                    await customdesigns.LoadDesignFromFile(
                        await customdesigns.GetFileFromDesignsFolder(DefaultValues.DefaultWindows11ThemeName));
                else
                    await customdesigns.LoadDesignFromFile(
                        await customdesigns.GetFileFromDesignsFolder(DefaultValues.DefaultThemeName));
            }
            catch (Exception e)
            {
                ShowInfobar("Could not load Themes to the folder\n" + e.Message, "", muxc.InfoBarSeverity.Error);
            }
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await Initialisation();

            //If the app was started by the fileactivationevent:
            if (e != null && e.Parameter is FileActivatedEventArgs fae)
            {
                //Dont create a new Tab cause a file is opened on start
                if (e != null)
                {
                    await tabactions.OpenFilesFromEvent(fae);
                }
            }
            else if (e.Parameter is CommandLineLaunchNavigationParameter args)
            {
                try
                {
                    await tabactions.OpenFileFromCommandLine(args);
                }
                catch (Exception ex)
                {
                    await new MessageDialog(ex.Message).ShowAsync();
                }
            }

            if (tabactions.GetTabItemCount() < 1)
                tabactions.NewTab();

            AfterInitialisation();
        }

        //MainPage-Events:
        private void MainPage_DragOver(object sender, DragEventArgs e)
        {
            DragOver(e);
        }
        private async void MainPage_Drop(object sender, DragEventArgs e)
        {
            await DropFile(sender, e);
        }
        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TextTabControl.Width = Statusbar.Width = e.NewSize.Width;
            if (e.NewSize.Width < 700 && WordCountDisplay.Margin.Left != 3)
            {
                WordCountDisplay.Margin = SaveStatusDisplay.Margin =
                EncodingDisplay.Margin = LineNumberDisplay.Margin =
                ZoomDisplay.Margin = FileNameDisplay.Margin = new Thickness(0, 0, 0, 0);
            }
            else if (e.NewSize.Width >= 700 && WordCountDisplay.Margin.Left == 0)
            {
                WordCountDisplay.Margin = SaveStatusDisplay.Margin =
                EncodingDisplay.Margin = LineNumberDisplay.Margin =
                ZoomDisplay.Margin = FileNameDisplay.Margin = new Thickness(10, 0, 10, 0);
            }
        }
        private async void MainPage_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
            var shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);
            var alt = Window.Current.CoreWindow.GetKeyState(VirtualKey.Menu);

            //Commands without modifierkey:
            KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.F11, Fullscreen_Action);
            KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.F1, OpenSettings_Action);

            //Call everytime control is pressed:
            if (ctrl.HasFlag(CoreVirtualKeyStates.Down) && !alt.HasFlag(CoreVirtualKeyStates.Down))
            {
                if (!SettingsWindowSelected)
                {
                    KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.N, NewTab_Action);
                    KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.T, NewTab_Action);
                    KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.O, Open_Action);
                }
            }

            //Call only if a tab exists and the settingswindow is not opened:
            if (tabactions.GetTabItemCount() > 0 && !SettingsWindowSelected)
            {
                if (e.Key == VirtualKey.Escape)
                {
                    if (GoToLineWindow != null && SearchWindow != null)
                    {
                        if (GoToLineWindowIsOpen && !SearchIsOpen)
                        {
                            CloseGoToLineDialog();
                        }

                        if (SearchIsOpen && !GoToLineWindowIsOpen)
                        {
                            CloseSearchWindow();
                        }

                        if (SearchIsOpen && GoToLineWindowIsOpen)
                        {
                            CloseSearchWindow();
                            CloseGoToLineDialog();
                        }
                    }
                    tabactions.GetTextBoxFromSelectedTabPage().Focus(FocusState.Programmatic);
                }

                KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.F3, Find, false);
                KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.F2, OpenRenameFlyout);

                //Call every time the shortcut is pressed
                if (ctrl.HasFlag(CoreVirtualKeyStates.Down) && !alt.HasFlag(CoreVirtualKeyStates.Down))
                {
                    muxc.TabViewItem Tab = tabactions.GetSelectedTabPage();
                    if (Tab != null)
                    {
                        KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.G, ShowGoToLineWindow);
                        KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.F, ToggleSearchWnd, false);
                        KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.R, ToggleSearchWnd, true);
                        KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.W, CloseSelectedTab_SaveDatabase);
                        KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.J, ShowFileInfoDialog);
                        KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.E, ShowEncodingDialog);
                        KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.B, ExpandTabToNewWindow_Action);
                        KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.M, ToggleMarkdown_Action);
                        KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.Q, SurroundWithText_Action);

                        //Ctrl + S
                        if (e.Key == VirtualKey.S)
                        {
                            try
                            {
                                if (shift.HasFlag(CoreVirtualKeyStates.Down))
                                {
                                    await savefilehelper.SaveFileAs(Tab);
                                }
                                else
                                {
                                    await savefilehelper.Save(Tab);
                                }

                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Exception in MainPage --> Page_KeyDown --> e.Key == VirtualKey.S :\n" + ex.Message);
                            }
                        }
                    }

                    //Select tabs
                    int tabtoselect = 0;
                    switch (e.Key)
                    {
                        case VirtualKey.Number1:
                            tabtoselect = 1;
                            break;
                        case VirtualKey.Number2:
                            tabtoselect = 2;
                            break;
                        case VirtualKey.Number3:
                            tabtoselect = 3;
                            break;
                        case VirtualKey.Number4:
                            tabtoselect = 4;
                            break;
                        case VirtualKey.Number5:
                            tabtoselect = 5;
                            break;
                        case VirtualKey.Number6:
                            tabtoselect = 6;
                            break;
                        case VirtualKey.Number7:
                            tabtoselect = 7;
                            break;
                        case VirtualKey.Number8:
                            tabtoselect = 8;
                            break;
                        case VirtualKey.Number9:
                            tabtoselect = tabactions.GetTabItemCount();
                            break;
                    }
                    if (tabtoselect != 0 && tabactions.GetTabItemCount() >= tabtoselect)
                    {
                        TextTabControl.SelectedIndex = tabtoselect - 1;
                    }
                }
            }

            //Call only if a tab exists
            if (tabactions.GetTabItemCount() > 0)
            {
                if (ctrl.HasFlag(CoreVirtualKeyStates.Down))
                {
                    if (alt.HasFlag(CoreVirtualKeyStates.Down))
                    {
                        KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.Left, NavigateToPreviousTab_Action);
                        KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.Right, NavigateToNextTab_Action);
                    }
                    if (e.Key == VirtualKey.Tab)
                    {
                        if (shift.HasFlag(CoreVirtualKeyStates.Down))
                        {
                            NavigateToPreviousTab_Action();
                        }
                        else
                        {
                            NavigateToNextTab_Action();
                        }
                    }
                }
            }
        }
        private async void Application_OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            var deferral = e.GetDeferral();

            if (IsContentDialogOpen())
                e.Handled = true;

            if (OpenedSecondaryViews.Count > 0)
            {
                ShowInfobar(InfoBarMessages.CloseAllInstances, InfoBarMessages.CloseAllInstances_Title, muxc.InfoBarSeverity.Warning);
                e.Handled = true;
            }
            else
            {
                if (tabactions != null)
                {
                    if (await tabactions.CloseTabs() == false)
                        e.Handled = true;
                }
                else
                    Debug.WriteLine("MainPage -> Application_OnCloseRequest : TabActions is null");
            }

            if (IsContentDialogOpen())
                e.Handled = true;

            deferral.Complete();
        }
        private async void Application_ThemeChanged(ThemeListener sender)
        {
            await ChangeTheme(sender.CurrentTheme);
            await SetSettings(true);
        }
        private async Task ChangeTheme(ApplicationTheme sender)
        {
            void ReportError()
            {
                ShowInfobar(ErrorDialogs.LoadDesignError());
            }
            
            string DesignName;
            StorageFolder outputFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(DefaultValues.CustomDesigns_FolderName, CreationCollisionOption.OpenIfExists);
            if (outputFolder == null)
            {
                ReportError();
                return;
            }

            if (sender == ApplicationTheme.Dark)
            {
                DesignName = appsettings.GetSettingsAsString("ThemeForDarkMode", DefaultValues.DefaultThemeName);
            }
            else
            {
                DesignName = appsettings.GetSettingsAsString("ThemeForLightMode", DefaultValues.DefaultThemeName);
            }
            if (await customdesigns.LoadDesignFromFile(
                await customdesigns.GetFileFromDesignsFolder(DesignName)) == false)
            {
                ReportError();
            }
        }

        //TextTabControl-Events:
        private void TextTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //If tabitems equals zero, disable all items:
            if (TextTabControl.TabItems.Count == 0)
            {
                for (int i = 0; i < ToolbarFlyout.Items.Count; i++)
                {
                    if (ToolbarFlyout.Items[i] is MenuFlyoutItem item)
                    {
                        if (item.Name != "DropDownMenu_New" && item.Name != "DropDownMenu_Open" && item.Name != "DropDownMenu_Settings")
                        {
                            item.IsEnabled = false;
                        }
                    }
                    if (ToolbarFlyout.Items[i] is MenuFlyoutSubItem subitem)
                    {
                        subitem.IsEnabled = false;
                    }
                }
            }

            var tabpage = tabactions.GetSelectedTabPage();
            if (tabpage != null && tabpage.Content is TextControlBox textbox)
            {
                ShowHideControlsOnSelectionChanged(true);

                CurrentlySelectedTabPage = tabpage;
                CurrentlySelectedTabPage_Textbox = textbox;

                SettingsWindowSelected = false;

                if (ShowMenubar && MainMenuBar != null)
                {
                    MainMenuBar.Visibility = Visibility.Visible;
                }

                if (ShowStatusBar)
                {
                    Statusbar.Visibility = Visibility.Visible;
                }
                SearchIsOpen = appsettings.GetSettingsAsBool("SearchOpen", false);
                GoToLineWindowIsOpen = appsettings.GetSettingsAsBool("GoToLineWindowOpened", false);

                if (textbox.TabSaveMode == TabSaveMode.SaveAsFile || textbox.TabSaveMode == TabSaveMode.SaveAsTemp)
                {
                    if (IsControlNOTNull(CanNotRenameFileInfo))
                    {
                        CanNotRenameFileInfo.Visibility = Visibility.Collapsed;
                        RenameTextBox.IsEnabled = true;
                        RenameFileButton.IsEnabled = true;
                    }
                }
                else if (textbox.TabSaveMode == TabSaveMode.SaveAsDragDrop)
                {
                    CanNotRenameFileInfo.Visibility = Visibility.Visible;
                    RenameTextBox.IsEnabled = false;
                    RenameFileButton.IsEnabled = false;
                }

                //Show /hide the OpenWithEncoding Button when the file was not even saved
                OpenWithEncodingButton.IsEnabled = textbox.TabSaveMode == TabSaveMode.SaveAsTemp ? false : true;

                TextControlBox_ZoomChangedEvent(textbox, textbox._zoomFactor);
                TextControlBox_LineNumberchangedEvent(textbox, textbox.GetCurrentLineNumber);
                TextControlBox_DocumentTitleChangedEvent(textbox, textbox.Header);
                Content_EncodingChangedEvent(textbox, textbox.Encoding);
                Textbox_WordCountChangedEvent(textbox, textbox.CountWords());
                //Content_ColumnChangedEvent(textbox, textbox.GetCurrentColumn);
                Content_SaveStatusChangedEvent(textbox, textbox.IsModified);

                SetTitlebarText(tabpagehelper.GetTabFileName(tabpage));

                EncodingButton_Click(FindName(Encodings.EncodingToString(textbox.Encoding).Replace("-", "_") + "_EncodingButton"), null);
                //Check wordwrapbutton
                DropDownMenu_WordWrap.IsChecked = textbox.WordWrap == TextWrapping.Wrap;
            }
            else
            {
                ShowHideControlsOnSelectionChanged(false);
                CurrentlySelectedTabPage = null;
                CurrentlySelectedTabPage_Textbox = null;
                SettingsWindowSelected = true;

                if (tabpage != null && tabpage.Content is Frame)
                {
                    if (ShowMenubar)
                    {
                        MainMenuBar.Visibility = Visibility.Collapsed;
                    }
                    if (Statusbar != null)
                    {
                        Statusbar.Visibility = Visibility.Collapsed;
                    }
                    if (SearchIsOpen)
                        SearchIsOpen = false;

                    if (GoToLineWindowIsOpen)
                    {
                        GoToLineWindowIsOpen = false;
                    }
                }
            }
        }
        private void TextTabControl_AddTabButtonClick(muxc.TabView sender, object args)
        {
            NewTab_Action();
        }
        private async void TextTabControl_TabCloseRequested(muxc.TabView sender, muxc.TabViewTabCloseRequestedEventArgs args)
        {
            if (tabactions.GetTextBoxFromTabPage(args.Tab) != null)
            {
                await tabactions.CloseTabAndSaveDataBase(args.Tab);
            }
            else if (SettingsWindowSelected || args.Tab.Content is Frame)
            {
                CloseSettingsTabPage();
            }
            //Create a new tab when no tab exists
            if (tabactions.GetTabItemCount() == 0)
            {
                NewTab_Action();
            }
        }
        private async void TextTabControl_TabDroppedOutside(muxc.TabView sender, muxc.TabViewTabDroppedOutsideEventArgs args)
        {
            if (args.Tab != null)
                await ExpandTabPageToNewView(args.Tab);
        }

        //Titlebar
        private void Titlebar_Loaded(object sender, RoutedEventArgs e)
        {
            SetTitlebar();
        }
        private void SetTitlebar()
        {
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.InactiveForegroundColor = Convert.WhiteOrBlackFromColorBrightness(StatusbarBackgroundColor);

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            Window.Current.SetTitleBar(Titlebar);
            Titlebar.UpdateLayout();
        }
        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (FlowDirection == FlowDirection.LeftToRight)
            {
                Titlebar.MinWidth = sender.SystemOverlayRightInset;
                ShellTitlebarInset.MinWidth = sender.SystemOverlayLeftInset;
            }
            else
            {
                Titlebar.MinWidth = sender.SystemOverlayLeftInset;
                ShellTitlebarInset.MinWidth = sender.SystemOverlayRightInset;
            }
            Titlebar.Height = ShellTitlebarInset.Height = sender.Height;
        }

        //Autosave temp files / AutoBackup Database
        //private void AutosaveTempfilesTimer()
        //{
        //    var BackupTimeInMinutes = appsettings.GetSettingsAsInt("AutoSaveFileTime", DefaultValues.AutoSaveTempFileMinutes);
        //    if (BackupTimeInMinutes > 0)
        //    {
        //        var AutosaveTempfilesTimer = new DispatcherTimer();
        //        AutosaveTempfilesTimer.Tick += async delegate
        //        {
        //            await tabactions.SaveAllTabChanges();
        //        };
        //        AutosaveTempfilesTimer.Interval = new TimeSpan(0, 0, BackupTimeInMinutes, 0);
        //        AutosaveTempfilesTimer.Start();
        //    }
        //}
        private void AutoBackupDataBaseTimer()
        {
            var BackupTimeInMinutes = appsettings.GetSettingsAsInt("AutoBackupDatabaseTime", DefaultValues.AutoBackupDataBaseMinutes);
            if (BackupTimeInMinutes > 0)
            {
                var dp = new DispatcherTimer();
                dp.Tick += async delegate
                {
                    await tabactions.SaveAllTabChangesToBackup();
                };
                dp.Interval = new TimeSpan(0, 0, BackupTimeInMinutes, 0);
                dp.Start();
            }
        }

        //Settings
        public async Task SetSettingsToTheme()
        {
            ThemeHelper.RootTheme =
                this.RequestedTheme =
                (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsInt("ThemeIndex", 0).ToString());

            if (appsettings.GetSettingsAsBool("AutomaticThemeChange", false))
            {
                await ChangeTheme(App.Current.RequestedTheme);
            }
        }
        public void SetSettingsToTabPage(muxc.TabViewItem Tab, Thickness ContentThickness)
        {
            if (tabactions == null || Tab == null) return;
            if (tabactions.GetTextBoxFromTabPage(Tab) is TextControlBox textbox)
            {
                if (textbox == null)
                    return;

                textbox.ZoomChangedEvent += TextControlBox_ZoomChangedEvent;
                textbox.DocumentTitleChangedEvent += TextControlBox_DocumentTitleChangedEvent;
                textbox.LineNumberchangedEvent += TextControlBox_LineNumberchangedEvent;
                textbox.EncodingChangedEvent += Content_EncodingChangedEvent;
                textbox.SaveStatusChangedEvent += Content_SaveStatusChangedEvent;
                textbox.WordCountChangedEvent += Textbox_WordCountChangedEvent;
                
                if (textbox.FontFamily != TextBoxFontfamily)
                {
                    textbox.FontFamily = TextBoxFontfamily;
                }

                if (textbox.TextColor != TextColor)
                {
                    textbox.TextColor = TextColor;
                }

                if (textbox.Background != TextBoxBackgroundcolor)
                {
                    textbox.Background = TextBoxBackgroundcolor;
                }

                if (textbox.TextSelectionColor != TextSelectionColor)
                {
                    textbox.TextSelectionColor = TextSelectionColor;
                }

                if (textbox.LineNumberBackground != LineNumberBackgroundColor)
                {
                    textbox.LineNumberBackground = LineNumberBackgroundColor;
                }

                if (textbox.LineNumberForeground != LineNumberForegroundColor)
                {
                    textbox.LineNumberForeground = LineNumberForegroundColor;
                }

                if (textbox.Margin != ContentThickness)
                {
                    textbox.Margin = ContentThickness;
                }

                if(textbox.IsHandWritingEnabled != IsHandWritingEnabled)
                {
                    textbox.IsHandWritingEnabled = IsHandWritingEnabled;
                }

                if(textbox.ShowSelectionFlyout != ShowSelectionFlyout)
                {
                    textbox.ShowSelectionFlyout = ShowSelectionFlyout;
                }

                textbox.LineHighlighterBackground = LineHighlighterBackground;
                textbox.LineHighlighterForeground = LineHighlighterForeground;
                textbox.ShowLineNumbers = ShowLineNumbers;
                textbox.LineHighlighter = ShowLineHighlighter;
                textbox.FontSizeWithoutZoom = TextBoxFontSize;
                textbox.SetFontZoomFactor(textbox._zoomFactor);
                textbox.UpdateLayout();
                
                //Change the tabicon
                //Generate new icon if buffer is null
                if (TabPageFontIconSource == null)
                {
                    TabPageFontIconSource = new muxc.FontIconSource
                    {
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        Glyph = appsettings.GetSettingsAsString("TabIconId", DefaultValues.DefaultTabIconId)
                    };
                    Debug.WriteLine(Tab.Name + "=" + TabPageFontIconSource.Glyph);
                }
                if (textbox.IsReadOnly)
                    Tab.IconSource = new muxc.SymbolIconSource() { Symbol = Symbol.ProtectedDocument };
                else
                    Tab.IconSource = TabPageFontIconSource;

                Tab.UpdateLayout();
            }
        }
        public void ApplySettingsToAllTabPages()
        {
            //Apply settings to all TabPages
            Thickness textboxmargin = TextBoxMargin();
            var TabItems = tabactions.GetTabItems();
            for (int i = 0; i < TabItems.Count; i++)
            {
                if (TabItems[i] is muxc.TabViewItem Tab)
                {
                    SetSettingsToTabPage(Tab, textboxmargin);
                }
            }
        }
        private void SetControlColors()
        {
            //TextBackgroundColor --> Page1
            TextBoxBackgroundcolor = appsettings.GetSettingsAsColorWithDefault("TextBackgroundColor", DefaultValues.DefaultTextBackgroundColor);
            //TitleBarBackgroundColor --> Page4
            TitleBarBackgroundColor = appsettings.GetSettingsAsColorWithDefault("TitleBarBackgroundColor", DefaultValues.DefaultTitleBarBackgroundColor);
            //TextColor --> Page1
            TextColor = appsettings.GetSettingsAsColorWithDefault("TextColor", DefaultValues.DefaultTextColor);
            //TextSelectionColor --> Page1
            TextSelectionColor = appsettings.GetSettingsAsColorWithDefault("TextSelectionColor", DefaultValues.DefaultTextSelectionColor);
            //Tabcolor not focused --> Page4
            TabColorNotFocused = appsettings.GetSettingsAsColorWithDefault("TabColorNotFocused", DefaultValues.DefaultTabColorNotFocused);
            //Tab color focused --> Page4
            TabColorFocused = appsettings.GetSettingsAsColorWithDefault("TabColorFocused", DefaultValues.DefaultTabColorFocused);
            //LineNumberForeground --> Page1
            LineNumberForegroundColor = appsettings.GetSettingsAsColorWithDefault("LineNumberForegroundColor", DefaultValues.DefaultLineNumberForegroundColor);
            //LineNumberBackground --> Page1
            LineNumberBackgroundColor = appsettings.GetSettingsAsColorWithDefault("LineNumberBackgroundColor", DefaultValues.DefaultLineNumberBackgroundColor);
            //StatusbarForeground --> Page8
            StatusbarForegroundColor = appsettings.GetSettingsAsColorWithDefault("StatusbarForegroundColor", DefaultValues.DefaultStatusbarForegroundColor);
            //StatusbarBackground --> Page8
            StatusbarBackgroundColor = appsettings.GetSettingsAsColorWithDefault("StatusbarBackgroundColor", DefaultValues.DefaultStatusbarBackgroundColor);
            //LineHighlighterColor --> Page 1
            LineHighlighterBackground = appsettings.GetSettingsAsColorWithDefault("LineHighlighterBackground", Colors.Transparent);
            LineHighlighterForeground = appsettings.GetSettingsAsColorWithDefault("LineHighlighterForeground", DefaultValues.SystemAccentColor);

            //If user wants the text color and line color to be the same
            if (appsettings.GetSettingsAsInt("LineNumberForegroundColorIndex", 1) == 1)
            {
                LineNumberForegroundColor = appsettings.GetSettingsAsColorWithDefault("TextColor", DefaultValues.SystemAccentColorLight2);
            }
        }
        private void SetSettingsToStatusbar()
        {
            if (ShowStatusBar = appsettings.GetSettingsAsBool("ShowStatusbar", true))
            {
                if (!SettingsWindowSelected)
                {
                    Statusbar.Visibility = Visibility.Visible;
                }

                if (appsettings.GetSettingsAsBool("StatusbarInBoldFont", false))
                {
                    LineNumberDisplay.FontWeight = ZoomDisplay.FontWeight = WordCountDisplay.FontWeight =
                    FileNameDisplay.FontWeight = EncodingDisplay.FontWeight = SaveStatusDisplay.FontWeight
                    = FontWeights.Bold;
                }
                else
                {
                    LineNumberDisplay.FontWeight = ZoomDisplay.FontWeight = WordCountDisplay.FontWeight =
                    FileNameDisplay.FontWeight = EncodingDisplay.FontWeight = SaveStatusDisplay.FontWeight
                    = FontWeights.Normal;
                }

                Statusbar.Background = new SolidColorBrush(StatusbarBackgroundColor);
                SaveStatusDisplay.Foreground = WordCountDisplay.Foreground = LineNumberDisplay.Foreground = ZoomDisplay.Foreground =
                    FileNameDisplay.Foreground = EncodingDisplay.Foreground =
                    new SolidColorBrush(StatusbarForegroundColor);

                WordCountDisplay.Visibility = Convert.BoolToVisibility(appsettings.GetSettingsAsBool("ShowWordCountButtonOn_SBar", false));
                SaveStatusDisplay.Visibility = Convert.BoolToVisibility(appsettings.GetSettingsAsBool("ShowSaveStatusButtonOn_SBar", true));
                EncodingDisplay.Visibility = Convert.BoolToVisibility(appsettings.GetSettingsAsBool("ShowEncodingButtonOn_SBar", true));
                LineNumberDisplay.Visibility = Convert.BoolToVisibility(appsettings.GetSettingsAsBool("ShowLinenumberButtonOn_SBar", true));
                ZoomDisplay.Visibility = Convert.BoolToVisibility(appsettings.GetSettingsAsBool("ShowZoomButtonOn_SBar", true));
                FileNameDisplay.Visibility = Convert.BoolToVisibility(appsettings.GetSettingsAsBool("ShowRenameButtonOn_SBar", true));
            }
            Statusbar.Visibility = Convert.BoolToVisibility(ShowStatusBar);
        }
        private void SetSettingsToTitlebarButtons()
        {
            //Check if DropdownMenu is hidden, and when the are show the Settingsbutton in the corner
            //Apply dark/light themes to all buttons:
            var titlebarbuttonTheme = Convert.ThemeFromColorBrightness(TitleBarBackgroundColor);

            DropDownMenu.Visibility = Convert.BoolToVisibility(appsettings.GetSettingsAsBool("ShowDropdown", true));
            DropDownMenu.RequestedTheme = titlebarbuttonTheme;

            NavigateToPreviousTab.Visibility = Convert.BoolToVisibility(appsettings.GetSettingsAsBool("ShowNavigateToPreviousTab", false));
            NavigateToPreviousTab.RequestedTheme = titlebarbuttonTheme;

            NavigateToNextTab.Visibility = Convert.BoolToVisibility(appsettings.GetSettingsAsBool("ShowNavigateToNextTab", false));
            NavigateToNextTab.RequestedTheme = titlebarbuttonTheme;
        }
        private void SetSettingsToEditor()
        {
            //LineHighlighter
            ShowLineHighlighter = appsettings.GetSettingsAsBool("LineHighlighter", true);            
            //LineNumbers
            ShowLineNumbers = appsettings.GetSettingsAsBool("ShowLineNumbers", true);
            ShowSelectionFlyout = appsettings.GetSettingsAsBool("TextboxShowSelectionFlyout", false);
            IsHandWritingEnabled = appsettings.GetSettingsAsBool("HandwritingEnabled", DefaultValues.HandWritingEnabled);
            //Get data for Fontsize and Fontfamily and store in local variable to use it in SetSettingsToTabPage function
            TextBoxFontfamily = new FontFamily(appsettings.GetSettingsAsString("FontFamily", DefaultValues.DefaultFontFamily));
            TextBoxFontSize = appsettings.GetSettingsAsInt("FontSize", DefaultValues.DefaultFontsize);
        }
        private void SetSettingsToSearchDialog()
        {
            SearchReplaceWindowDisplay.Margin = new Thickness(10, 40 + (MainMenuBar != null ? MainMenuBar.Height : 0), 10, 0);
            GoToLineWindow.Background = SearchWindow.Background = DefaultValues.ContentDialogBackgroundColor();

            //Align the searchwindow either to the right or in the center
            GoToLineWindow.HorizontalAlignment = SearchReplaceWindowDisplay.HorizontalAlignment =
                appsettings.GetSettingsAsBool("SearchPanelCenterAlign", true) ? HorizontalAlignment.Center : HorizontalAlignment.Right;

            //Search dialog:
            if (appsettings.GetSettingsAsBool("SearchOpen", false))
                ShowSearchWindow("");
            else
                CloseSearchWindow();

            //Expand the search for replacing: 
            ShowReplaceOnSearch(!(appsettings.GetSettingsAsInt("SearchExpanded", 1) == 1));
        }
        private void SetSettingsToTabControl()
        {
            Color UnselectedTabViewItemColor;

            if (appsettings.GetSettingsAsBool("UseMica", false))
            {
                TextTabControl.Background = new SolidColorBrush(Colors.Transparent);
                UnselectedTabViewItemColor = Convert.GetColorFromThemeReversed(ThemeHelper.RootTheme);
            }
            else
            {
                TextTabControl.Background = appsettings.CreateBrushWithOrWithoutAcrylic(TitleBarBackgroundColor);
                UnselectedTabViewItemColor = Convert.WhiteOrBlackFromColorBrightness(TabColorNotFocused.A > 50 ? TabColorNotFocused : TitleBarBackgroundColor);
            }

            //Set the Icon for the tabpage:
            TabPageFontIconSource = new muxc.FontIconSource
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Glyph = appsettings.GetSettingsAsString("TabIconId", DefaultValues.DefaultTabIconId)
            };

            TextTabControl.RequestedTheme = Convert.ThemeFromColorBrightness(TitleBarBackgroundColor);
            (TextTabControl.Resources["TabViewItemHeaderBackground"] as SolidColorBrush).Color = TabColorNotFocused;
            (TextTabControl.Resources["TabViewItemHeaderBackgroundSelected"] as SolidColorBrush).Color = TabColorFocused;
            (TextTabControl.Resources["TabViewItemHeaderForeground"] as SolidColorBrush).Color = UnselectedTabViewItemColor;
            (TextTabControl.Resources["TabViewItemHeaderForegroundSelected"] as SolidColorBrush).Color = Convert.WhiteOrBlackFromColorBrightness(TabColorFocused);
            UnderTabControlLine.Background = new SolidColorBrush(TabColorFocused);
            UnderTabControlLine.Visibility = Convert.BoolToVisibility(appsettings.GetSettingsAsBool("ShowUnderTabLine", true));
            TextTabControl.TabWidthMode = 
                (muxc.TabViewWidthMode)Enum.Parse(typeof(muxc.TabViewWidthMode),
                appsettings.GetSettingsAsString("TabSizeModeIndex", DefaultValues.defaultTabSizeMode.ToString()));

            TextTabControl.Margin = new Thickness(0, 0, 0, Statusbar.Visibility == Visibility.Collapsed && !SettingsWindowSelected ? 0 : Statusbar.Height);
            TextTabControl.UpdateLayout();
        }
        private void SetSettingsToMenubar()
        {
            ShowMenubar = appsettings.GetSettingsAsBool("ShowMenubar", true);
            if(ShowMenubar)
            {
                int index = appsettings.GetSettingsAsInt("MenuBarAlignment", 1);
                if (index > 2 || index < 0)
                    index = 0;

                MainMenuBar.HorizontalAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), index.ToString()); 
            }
        }
        public async Task SetSettings(bool ApplyToAllTabPages = true)
        {
            try
            {
                //Set the theme and do the automatic theme change
                await SetSettingsToTheme();
                               
                //App-Background:
                BackgroundHelper.SetBackgroundToPage(this);
                
                //Application Language
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = appsettings.GetSettingsAsString("AppLanguage", "en-US");

                //Load the colors for the controls from settings, right at the beginning
                SetControlColors();

                //Menubar
                SetSettingsToMenubar();

                //Statusbar
                SetSettingsToStatusbar();

                //Tabcontrol
                SetSettingsToTabControl();

                SetSettingsToTitlebarButtons();

                SetSettingsToEditor();

                //Spellcheckingbutton
                DropDownMenu_SpellChecking.IsChecked = appsettings.GetSettingsAsBool("Spellchecking", DefaultValues.SpellCheckingEnabled);
                //Fullscreenbutton
                Utilities.FullScreen(appsettings.GetSettingsAsBool("FullScreen", false), appsettings);

                //Searchdialog
                SetSettingsToSearchDialog();

                if (ApplyToAllTabPages)
                {
                    ApplySettingsToAllTabPages();
                }

                TextTabControl_SelectionChanged(null, null);
                SetTitlebar();
            }
            catch (Exception e)
            {
                Debug.WriteLine("EXCEPTION in Mainpage -> SetSettings\n" + e.StackTrace);
                ShowInfobar(ErrorDialogs.LoadSettingsError(e));
            }
        }
        public Thickness TextBoxMargin()
        {
            return new Thickness(
                0,
                UnderTabControlLine.Height + (ShowMenubar ? (MainMenuBar.ActualHeight == 0 ? MainMenuBar.Height : MainMenuBar.ActualHeight) : 0),
                0,
                0
                );
        }

        /// <summary>
        ///Used for the keyevent when ESC-Key is pressed. Hides the flyout when it is opened
        /// </summary>
        private void HideFlyoutIfOpened(Flyout flyout)
        {
            if (flyout.IsOpen)
            {
                flyout.Hide();
            }
        }
        private bool IsControlNOTNull(object Control)
        {
            return Control != null;
        }
        private void SetTitlebarText(string title)
        {
            ApplicationView.GetForCurrentView().Title = title;
        }
        public muxc.InfoBar ShowInfobar(string Content = "", string Title = "", muxc.InfoBarSeverity severity = muxc.InfoBarSeverity.Warning, int VisibilityTime = 5, object content = null)
        {
            var MainInformationInfobar = new muxc.InfoBar
            {
                Severity = severity,
                Title = Title,
                Message = Content,
                IsOpen = true,
                Content = content,
                Margin = new Thickness(0, 0, 0, 5),
                Name = "Infobar" + InfoBarDisplay.Children.Count
            };
            var HideInfobarTimer = new DispatcherTimer();
            HideInfobarTimer.Interval = TimeSpan.FromSeconds(VisibilityTime);
            HideInfobarTimer.Tick += delegate
            {
                MainInformationInfobar.IsOpen = false;
                InfoBarDisplay.Children.Remove(MainInformationInfobar);

                HideInfobarTimer.Stop();
                InfoBarDisplay.UpdateLayout();
            };
            HideInfobarTimer.Start();
            InfoBarDisplay.Children.Add(MainInformationInfobar);
            return MainInformationInfobar;
        }
        public muxc.InfoBar ShowInfobar(muxc.InfoBar infobar, int VisibilityTime = 5)
        {
            if (infobar.Margin.Bottom != 5)
                infobar.Margin = new Thickness(0, 0, 0, 5);

            infobar.Name = "Infobar" + InfoBarDisplay.Children.Count;
            var HideInfobarTimer = new DispatcherTimer();
            HideInfobarTimer.Interval = TimeSpan.FromSeconds(VisibilityTime);
            HideInfobarTimer.Tick += delegate
            {
                infobar.IsOpen = false;
                InfoBarDisplay.Children.Remove(infobar);

                HideInfobarTimer.Stop();
                InfoBarDisplay.UpdateLayout();
            };
            HideInfobarTimer.Start();
            InfoBarDisplay.Children.Add(infobar);
            return infobar;
        }
        private bool IsOnNewVersion()
        {
            string version = Package.Current.Id.Version.Major + "." +
                Package.Current.Id.Version.Minor + "." +
                Package.Current.Id.Version.Build;
            if (version != appsettings.GetSettings("AppVersion"))
            {
                appsettings.SaveSettings("AppVersion", version);
                return true;
            }
            return false;
        }
        private void ShowNewVersionInfobar()
        {
            if (NewVersionInfobar == null)
                NewVersionInfobar = FindName("NewVersionInfobar") as muxc.InfoBar;

            NewVersionInfobar.Background = DefaultValues.ContentDialogBackgroundColor();
            NewVersionInfobar.Foreground = DefaultValues.ContentDialogForegroundColor();
            NewVersionInfobar.Closed += delegate
            {
                if(NewVersionInfobar != null)
                    UnloadObject(NewVersionInfobar);
            };
            string version = Package.Current.Id.Version.Major + "." +
                Package.Current.Id.Version.Minor + "." +
                Package.Current.Id.Version.Build;
            NewVersionInfobar.Message = $"{appsettings.GetResourceString("InfoBarMessage_NewVersion_Text1/Text")} {version}";
            NewVersionInfobar.IsOpen = true;
        }

        private void ShowHideControlsOnSelectionChanged(bool isEnabled)
        {
            //just check two
            if (!DropDownMenu_Redo.IsEnabled || !DropDownMenu_New.IsEnabled)
            {
                //DropDownMenu:
                for (int i = 0; i < ToolbarFlyout.Items.Count; i++)
                {
                    if (ToolbarFlyout.Items[i] is MenuFlyoutItem item)
                    {
                        if(item.Tag is string str && str.Equals("HideIfNoTab", StringComparison.Ordinal))
                        {
                            item.IsEnabled = isEnabled;
                        }
                    }
                    if (ToolbarFlyout.Items[i] is MenuFlyoutSubItem subitem)
                    {
                        if (subitem.Tag is string str && str.Equals("HideIfNoTab", StringComparison.Ordinal))
                        {
                            subitem.IsEnabled = isEnabled;
                        }
                    }
                }
                //Menubar:
                for(int i = 0; i<MainMenuBar.Items.Count; i++)
                {
                    if(MainMenuBar.Items[i] is MenuBarItem mbitem)
                    {
                        if (mbitem.Tag is string str && str.Equals("HideIfNoTab", StringComparison.Ordinal))
                        {
                            mbitem.IsEnabled = isEnabled;
                        }
                        else
                        {
                            for (int j = 0; j < mbitem.Items.Count; j++)
                            {
                                if (mbitem.Items[j] is MenuFlyoutItem mfi)
                                {
                                    if (mfi.Tag is string str2 && str2.Equals("HideIfNoTab", StringComparison.Ordinal))
                                    {
                                        mfi.IsEnabled = isEnabled;
                                    }
                                }
                                else if (mbitem.Items[j] is ToggleMenuFlyoutItem tmfi)
                                {
                                    if (tmfi.Tag is string str2 && str2.Equals("HideIfNoTab", StringComparison.Ordinal))
                                    {
                                        tmfi.IsEnabled = isEnabled;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //Drag-Drop
        private async Task<bool> OpenStorageFiles(IReadOnlyList<IStorageItem> StorageItems, TabSaveMode savemode = TabSaveMode.SaveAsDragDrop)
        {
            bool result = false;
            Thickness textboxmargin = TextBoxMargin();
            for (int i = 0; i < StorageItems.Count; i++)
            {
                if (StorageItems[i] is StorageFile file)
                {
                    if (tabactions.GetTabItemCount() == 1)
                    {
                        if (TextTabControl.TabItems[0] is muxc.TabViewItem tab)
                        {
                            //override first created tab, only if it is emty
                            if (tabpagehelper.GetIsModified(tab) == false && tabpagehelper.GetTabText(tab).Length == 0 && tabpagehelper.GetTabFilepath(tab).Length == 0)
                            {
                                await tabactions.RemoveTab(tab);
                            }
                        }
                    }
                    var (Succed, TabPage) = await tabactions.DoOpenFile(null, file, savemode, false, false, "", false, true, true);
                    if (Succed == true)
                        SetSettingsToTabPage(TabPage, textboxmargin);
                    else
                        result = false;
                }
            }
            if (await tabactions.SaveAllTabChanges() == false)
                result = false;
            TextTabControl_SelectionChanged(null, null);
            return result;
        }
        public async Task DropFile(object sender, DragEventArgs e)
        {
            try
            {
                if (e.DataView.Contains(StandardDataFormats.Text))
                {
                    if (CurrentlySelectedTabPage_Textbox != null)
                    {
                        CurrentlySelectedTabPage_Textbox.SelectedText = await e.DataView.GetTextAsync();
                    }
                }
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    var items = await e.DataView.GetStorageItemsAsync();
                    await OpenStorageFiles(items);
                }
            }
            catch (Exception exception)
            {
                ShowInfobar(appsettings.GetResourceString("ErrorDialogs_DragDropError/Text") + "\n" + exception.Message, appsettings.GetResourceString("ErrorDialogs_Header_Error.Text"), muxc.InfoBarSeverity.Error);
            }
        }
        public new void DragOver(DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;

            if (e.DragUIOverride != null)
            {
                e.DragUIOverride.Caption = "Open";
                e.DragUIOverride.SetContentFromSoftwareBitmap(new Windows.Graphics.Imaging.SoftwareBitmap(Windows.Graphics.Imaging.BitmapPixelFormat.Rgba8, 400, 400));
                e.DragUIOverride.IsContentVisible = true;
            }
        }
        
        //Dialogs:
        public async void ShowEncodingDialog()
        {
            if (CurrentlySelectedTabPage != null)
            {
                await new EncodingDialog(CurrentlySelectedTabPage).ShowDialog();
            }
        }
        public async void ShowRecyclebinDialog()
        {
            await new RecyclebinWindow(tabactions).ShowDialog();
        }
        private async void ShowFileInfoDialog()
        {
            if (CurrentlySelectedTabPage != null)
            {
                await new FileInfoDialog(CurrentlySelectedTabPage).ShowAsync();
            }
        }
        private bool IsContentDialogOpen()
        {
            var openedpopups = VisualTreeHelper.GetOpenPopups(Window.Current);
            for (int i = 0; i < openedpopups.Count; i++)
            {
                if (openedpopups[i].Child is ContentDialog)
                {
                    return true;
                }
            }
            return false;
        }

        public async void CloseSettingsTabPage()
        {
            if (SettingsTabPage != null)
            {
                TextTabControl.TabItems.Remove(SettingsTabPage);
                tabactions.SettingsTabPage = null;
                SettingsWindowSelected = false;
                await SetSettings();
            }
        }
        public void ShowSettingsTabPage(string tabforpage = "")
        {
            //Create only if null otherwise select it
            if (SettingsTabPage == null)
            {
                var frame = new Frame
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                frame.Navigate(typeof(SettingsPage), new SettingsNavigationParameter { Mainpage = this, Tabcontrol = TextTabControl, PageToNavigateTo = tabforpage });
                tabactions.SettingsTabPage =  SettingsTabPage = new muxc.TabViewItem
                {
                    Content = frame,
                    Header = "Settings",
                    Tag = "Settings",
                    IconSource = new muxc.SymbolIconSource { Symbol = Symbol.Setting }
                };
            }
            if(!TextTabControl.TabItems.Contains(SettingsTabPage))
                TextTabControl.TabItems.Add(SettingsTabPage);
            TextTabControl.SelectedItem = SettingsTabPage;
        }

        //Actions
        private void Copy_Action()
        {
            if (CurrentlySelectedTabPage_Textbox != null)
            {
                CurrentlySelectedTabPage_Textbox.Copy();
            }
        }
        private void Cut_Action()
        {
            if (CurrentlySelectedTabPage_Textbox != null)
            {
                CurrentlySelectedTabPage_Textbox.Cut();
            }
        }
        private void Paste_Action()
        {
            if (CurrentlySelectedTabPage != null && CurrentlySelectedTabPage_Textbox != null)
            {
                if (tabpagehelper.GetTabReadOnly(CurrentlySelectedTabPage) == false)
                {
                    CurrentlySelectedTabPage_Textbox.Paste();
                }
            }
        }
        private void Redo_Action()
        {
            if (CurrentlySelectedTabPage != null)
            {
                if (tabpagehelper.GetTabReadOnly(CurrentlySelectedTabPage) == false)
                {
                    CurrentlySelectedTabPage_Textbox.Redo();
                }
            }
        }
        private void Undo_Action()
        {
            if (CurrentlySelectedTabPage != null)
            {
                if (tabpagehelper.GetTabReadOnly(CurrentlySelectedTabPage) == false)
                {
                    CurrentlySelectedTabPage_Textbox.Undo();
                }
            }
        }
        private async void Save_Action()
        {
            if (CurrentlySelectedTabPage != null)
            {
                await savefilehelper.Save(CurrentlySelectedTabPage);
            }
        }
        private async void Open_Action()
        {

            var tabs = await tabactions.OpenFile();
            if (tabs != null)
            {
                for (int i = 0; i < tabs.Count; i++)
                {
                    if (tabs[i] != null)
                    {
                        SetSettingsToTabPage(tabs[i], TextBoxMargin());
                    }
                }
            }
        }
        private async void SaveAs_Action()
        {
            if (CurrentlySelectedTabPage != null)
            {
                await savefilehelper.SaveFileAs(CurrentlySelectedTabPage);
            }
        }
        private void SelectAll_Action()
        {
            if (CurrentlySelectedTabPage_Textbox != null)
            {
                CurrentlySelectedTabPage_Textbox.SelectAll();
            }
        }
        private void WordWrap_Action()
        {
            if (CurrentlySelectedTabPage_Textbox != null)
            {
                CurrentlySelectedTabPage_Textbox.WordWrap =
                    CurrentlySelectedTabPage_Textbox.WordWrap == TextWrapping.Wrap ? TextWrapping.NoWrap : TextWrapping.Wrap;
            }
        }
        private async void Encoding_Action()
        {
            if (CurrentlySelectedTabPage != null)
            {
                await new EncodingDialog(CurrentlySelectedTabPage).ShowDialog();
            }
        }
        private void ZoomIn_Action()
        {
            if (CurrentlySelectedTabPage != null)
            {
                tabpagehelper.ZoomIn(CurrentlySelectedTabPage);
            }
        }
        private void ZoomOut_Action()
        {
            if (CurrentlySelectedTabPage != null)
            {
                tabpagehelper.ZoomOut(CurrentlySelectedTabPage);
            }
        }
        private void SearchWindow_Action()
        {
            if (!SearchIsOpen && CurrentlySelectedTabPage_Textbox != null)
            {
                ShowSearchWindow(CurrentlySelectedTabPage_Textbox.SelectedText);
                ShowReplaceOnSearch(false);
            }
            else
            {
                CloseSearchWindow();
            }
        }
        private void Share_Action()
        {
            if (CurrentlySelectedTabPage_Textbox != null)
            {
                new ShareFile(CurrentlySelectedTabPage_Textbox);
            }
        }
        private void SpellChecking_Action()
        {
            var tabitems = tabactions.GetTabItems();
            for (int i = 0; i < tabitems.Count; i++)
            {
                if (tabitems[i] is muxc.TabViewItem Tab)
                {
                    if (Tab.Content is TextControlBox textbox)
                    {
                        textbox.SpellChecking = !textbox.SpellChecking;
                        appsettings.SaveSettings("Spellchecking", textbox.SpellChecking);
                    }
                }
            }
        }
        private void NavigateToNextTab_Action()
        {
            tabactions.SelectNextTab();
        }
        private void NavigateToPreviousTab_Action()
        {
            tabactions.SelectPreviousTab();
        }
        private void DuplicateLine_Action()
        {
            if (CurrentlySelectedTabPage_Textbox != null)
            {
                CurrentlySelectedTabPage_Textbox.DuplicateLine();
            }
        }
        public void NewTab_Action()
        {
            if (tabactions.GetTabItemCount() > -1)
            {
                muxc.TabViewItem tab = tabactions.NewTab();
                newTabSaveTime.Tick += async delegate
                {
                    await tabactions.SaveAllTabChanges();
                    newTabSaveTime.Stop();
                };
                newTabSaveTime.Interval = new TimeSpan(0, 0, 0, 0, 1000);
                newTabSaveTime.Start();
                if (tab != null)
                {
                    SetSettingsToTabPage(tab, TextBoxMargin());
                }
            }
        }
        private void OpenSettings_Action()
        {
            ShowSettingsTabPage();
        }
        private async void CloseSelectedTab_SaveDatabase()
        {
            if (tabactions.GetTextBoxFromTabPage(CurrentlySelectedTabPage) != null)
            {
                await tabactions.CloseTabAndSaveDataBase(CurrentlySelectedTabPage);
            }
            else if (SettingsWindowSelected)
            {
                CloseSettingsTabPage();
            }
            //Create a new tab when no tab exists
            if(tabactions.GetTabItemCount() == 0)
            {
                NewTab_Action();
            }
        }
        private async void ExpandTabToNewWindow_Action()
        {
            if (CurrentlySelectedTabPage != null)
                await ExpandTabPageToNewView(CurrentlySelectedTabPage);
        }
        private void Fullscreen_Action()
        {
            Utilities.ToggleFullscreen(appsettings);
        }
        private void ToggleMarkdown_Action()
        {
            if (CurrentlySelectedTabPage != null && CurrentlySelectedTabPage_Textbox != null)
            {
                CurrentlySelectedTabPage_Textbox.MarkdownPreview = !CurrentlySelectedTabPage_Textbox.MarkdownPreview;
            }
        }
        private void SurroundWithText_Action()
        {
            if (CurrentlySelectedTabPage_Textbox == null)
                return;
            //reset to default state
            SurroundWith_Textbox2.Visibility = Visibility.Collapsed;
            SurroundWith_Textbox1.Text = SurroundWith_Textbox2.Text = "";
            var rect = CurrentlySelectedTabPage_Textbox.GetSelectionRect();

            SurroundWithFlyout.ShowAt(
                CurrentlySelectedTabPage_Textbox,
                new FlyoutShowOptions {
                    Position = new Point { X = rect.X + (rect.Width / 2) + 20, Y = rect.Y + (rect.Height / 2) - 40 - CurrentlySelectedTabPage_Textbox.GetScrollbarPositions().ScrollbarPositionVertical},
                    Placement = FlyoutPlacementMode.Auto}
                );
            SurroundWith_Textbox1.Focus(FocusState.Programmatic);
        }
        private void LockTab_Action()
        {
            if(CurrentlySelectedTabPage != null)
            {
                tabpagehelper.SetTabReadOnly(CurrentlySelectedTabPage, !tabpagehelper.GetTabReadOnly(CurrentlySelectedTabPage));
            }
        }

        //Click-Events
        private void NewDocumentButton(object sender, RoutedEventArgs e)
        {
            NewTab_Action();
        }
        private void SaveFileButton(object sender, RoutedEventArgs e)
        {
            Save_Action();
        }
        private void Save_Click(object sender, PointerRoutedEventArgs e)
        {
            Save_Action();
        }
        private void UndoButton(object sender, RoutedEventArgs e)
        {
            Undo_Action();
        }
        private void RedoButton(object sender, RoutedEventArgs e)
        {
            Redo_Action();
        }
        private void CutButton(object sender, RoutedEventArgs e)
        {
            Cut_Action();
        }
        private void CopyButton(object sender, RoutedEventArgs e)
        {
            Copy_Action();
        }
        private void PasteButton(object sender, RoutedEventArgs e)
        {
            Paste_Action();
        }
        public void Settingsbutton(object sender, RoutedEventArgs e)
        {
            OpenSettings_Action();
        }
        private void OpenFileButton(object sender, RoutedEventArgs e)
        {
            Open_Action();
        }
        private void SaveAsAppBarButtonName_Click(object sender, RoutedEventArgs e)
        {
            SaveAs_Action();
        }
        private void SelectAllAppBarButtonName_Click(object sender, RoutedEventArgs e)
        {
            SelectAll_Action();
        }
        private void WordWrapButton_Click(object sender, RoutedEventArgs e)
        {
            WordWrap_Action();
        }
        private void EncodingAppBarButtonName_Click(object sender, RoutedEventArgs e)
        {
            Encoding_Action();
        }
        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            Fullscreen_Action();
        }
        private void ZoomInAppBarButtonName_Click(object sender, RoutedEventArgs e)
        {
            ZoomIn_Action();
        }
        private void ZoomOutAppBarButtonName_Click(object sender, RoutedEventArgs e)
        {
            ZoomOut_Action();
        }
        private void SearchAppBarButtonName_Click(object sender, RoutedEventArgs e)
        {
            SearchWindow_Action();
        }
        private void ShareAppBarButtonName_Click(object sender, RoutedEventArgs e)
        {
            Share_Action();
        }
        private void SpellcheckingButton_Click(object sender, RoutedEventArgs e)
        {
            SpellChecking_Action();
        }
        private void GoToLineButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGoToLineWindow();
        }
        private void FileInfoButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFileInfoDialog();
        }
        private void RecycleBin_Click(object sender, RoutedEventArgs e)
        {
            ShowRecyclebinDialog();
            ApplySettingsToAllTabPages();
        }
        private void NavigateToPreviousTab_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPreviousTab_Action();
        }
        private void NavigateToNextTab_Click(object sender, RoutedEventArgs e)
        {
            NavigateToNextTab_Action();
        }
        private void ViewChangelog_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingsTabPage("Changelog");
        }
        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            CloseSelectedTab_SaveDatabase();
        }
        private void MarkdownPreview_Click(object sender, RoutedEventArgs e)
        {
            ToggleMarkdown_Action();
        }
        private void LockTab_Click(object sender, RoutedEventArgs e)
        {
            LockTab_Action();
        }
        private async void Close_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && CurrentlySelectedTabPage != null)
            {
                switch (Convert.ToInt(item.Tag))
                {
                    case 0:
                        await tabactions.CloseAllTabs();
                        break;
                    case 1:
                        await tabactions.CloseAllLeft(CurrentlySelectedTabPage);
                        break;
                    case 2:
                        await tabactions.CloseAllRight(CurrentlySelectedTabPage);
                        break;
                    case 3:
                        await tabactions.CloseAllButThis(CurrentlySelectedTabPage);
                        break;
                    case 4:
                        await tabactions.CloseAllWithoutSave();
                        break;
                }
            }
        }
        private void OpenInNewView_Click(object sender, RoutedEventArgs e)
        {
            ExpandTabToNewWindow_Action();
        }
        private async void FormatCode_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentlySelectedTabPage_Textbox == null)
                return;

            if (sender is MenuFlyoutItem item)
            {
                string text = CurrentlySelectedTabPage_Textbox.GetText();
                switch (Convert.ToInt(item.Tag, 0))
                {
                    case 0: //Json
                        text = CodeFormatter.FormatJson(text);
                        break;
                    case 1: //Xml
                        var res = CodeFormatter.FormatXml(text, out string output);
                        if (res != null)
                            ShowInfobar(res.Message, "Could not format you code:", InfoBarSeverity.Error);
                        else
                            text = output;
                        break;
                    case 2: //C#
                        text = CodeFormatter.FormatCs(text);
                        break;
                }
                await CurrentlySelectedTabPage_Textbox.ChangeText(text);
            }
        }
        private void SurroundWith_Click(object sender, RoutedEventArgs e)
        {
            SurroundWithText_Action();
        }
        private void SurroundWith_Textbox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (tb.Name == SurroundWith_Textbox1.Name)
                {
                    if (e.Key == VirtualKey.Enter)
                    {
                        CurrentlySelectedTabPage_Textbox.SurroundSelectionBy(SurroundWith_Textbox1.Text);
                        SurroundWithFlyout.Hide();
                    }
                    else if (e.Key == VirtualKey.Tab)
                    {
                        SurroundWith_Textbox2.Visibility = Visibility.Visible;
                    }
                }
                else if (tb.Name == SurroundWith_Textbox2.Name)
                {
                    if (e.Key == VirtualKey.Enter)
                    {
                        CurrentlySelectedTabPage_Textbox.SurroundSelectionBy(SurroundWith_Textbox1.Text, SurroundWith_Textbox2.Text);
                        SurroundWithFlyout.Hide();
                    }
                }
            }
        }
        private void DuplicateLine_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentlySelectedTabPage_Textbox != null)
                CurrentlySelectedTabPage_Textbox.DuplicateLine();
        }


        //Search-Dialog
        private bool SearchIsOpen
        {
            get => SearchWindow.Visibility == Visibility.Visible;
            set { SearchWindow.Visibility = Convert.BoolToVisibility(value); }
        }
        private void Find(bool Up = false)
        {
            var tb = tabactions.GetTextBoxFromSelectedTabPage();
            if (tb != null)
            {
                var res = tb.FindInText(TextToFindTextbox.Text, Up, FindMatchCaseButton.IsChecked ?? false, FindWholeWordButton.IsChecked ?? false);
                if (res)
                {
                    SearchWindow.BorderBrush = DefaultValues.CorrectInput_Color;
                }
                else
                {
                    SearchWindow.BorderBrush = DefaultValues.WrongInput_Color;
                }
            }
        }
        private void ShowReplaceOnSearch(bool Show)
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
                if(SearchWindow.Height > 45)
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
        public void ShowSearchWindow(string text = "")
        {
            if (CurrentlySelectedTabPage_Textbox == null)
                return;

            TextToFindTextbox.Text = text.Length == 0 ? CurrentlySelectedTabPage_Textbox.SelectedText : text;
            SearchIsOpen = true;

            appsettings.SaveSettings("SearchOpen", true);
            FindMatchCaseButton.IsChecked = appsettings.GetSettingsAsBool("FindMatchCase", false);
            FindWholeWordButton.IsChecked = appsettings.GetSettingsAsBool("FindWholeWord", false);

            TextToFindTextbox.Focus(FocusState.Keyboard);
            TextToFindTextbox.SelectAll();
        }
        private void CloseSearchWindow()
        {
            SearchIsOpen = false;
            appsettings.SaveSettings("SearchOpen", false);
        }
        private void ToggleSearchWnd(bool Replace)
        {
            if (!SearchIsOpen)
            {
                ShowSearchWindow("");
                ShowReplaceOnSearch(Replace);
            }
            else
            {
                CloseSearchWindow();
            }
        }
        //Search-Dialog Events:
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
            if (CurrentlySelectedTabPage_Textbox != null)
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
            if (CurrentlySelectedTabPage_Textbox != null)
            {
                if (CurrentlySelectedTabPage_Textbox.ReplaceInText(
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
            ShowReplaceOnSearch(appsettings.GetSettingsAsInt("SearchExpanded", 0) == 1);
        }

        //Go to line dialog
        private bool GoToLineWindowIsOpen { get => GoToLineWindow.Visibility == Visibility.Visible; set { GoToLineWindow.Visibility = Convert.BoolToVisibility(value); } }
        private void CloseGoToLineDialog()
        {
            GoToLineWindowIsOpen = false;
            appsettings.SaveSettings("GoToLineWindowOpened", false);
        }
        private void GoToLinebutton2_Click(object sender, RoutedEventArgs e)
        {
            bool Succed = DoGoToLine(GoToLineTextBox);
            GoToLineWindow.BorderBrush = Succed ? DefaultValues.CorrectInput_Color : DefaultValues.WrongInput_Color;
            if (appsettings.GetSettingsAsBool("HideGoToLineDialogAfterEntering", true) && Succed)
            {
                GoToLineWindowIsOpen = false;
            }
        }
        private void ShowGoToLineWindow()
        {
            if (!SettingsWindowSelected)
            {
                GoToLineWindowIsOpen = !GoToLineWindowIsOpen;
                appsettings.SaveSettings("GoToLineWindowOpened", GoToLineWindowIsOpen);
                GoToLineTextBox.Focus(FocusState.Programmatic);
            }
        }
        private void GoToLineWindow_CloseClick(object sender, RoutedEventArgs e)
        {
            CloseGoToLineDialog();
        }
        private void TextBoxes_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.SelectAll();
            }
        }

        ////////Statusbar////////
        //EVENTS//
        private void TextControlBox_ZoomChangedEvent(TextControlBox sender, double ZoomFactor)
        {
            if (Statusbar != null)
            {
                preventZoomOnFactorChanged = true;
                ZoomDisplay.Content = appsettings.GetResourceString("Statusbar_Display_Zoom/Text") + " " + (int)ZoomFactor + "%";
                ZoomFlyoutSlider.Value = (int)ZoomFactor;
                ZoomFlyoutSlider.Header = ZoomFactor + "%";
                preventZoomOnFactorChanged = false;
            }
        }
        private void TextControlBox_DocumentTitleChangedEvent(TextControlBox sender, string Header)
        {
            if (Statusbar != null && FileNameDisplay != null)
            {
                FileNameDisplay.Content = Header.Replace("*", "");
            }
        }
        private void TextControlBox_LineNumberchangedEvent(TextControlBox sender, int CurrentLine)
        {
            if (Statusbar != null)
            {
                LineNumberDisplay.Content = appsettings.GetResourceString("Statusbar_Display_Line/Text") + " " + CurrentLine;
                //LineNumberDisplay.Content = appsettings.GetResourceString("Statusbar_Display_Line/Text") + " " + CurrentLine + " " + appsettings.GetResourceString("Statusbar_Display_Column/Text") + " " + sender.GetCurrentColumn;
            }
        }
        private void Content_EncodingChangedEvent(TextControlBox sender, Encoding e)
        {
            if (Statusbar != null)
            {
                EncodingDisplay.Content = Encodings.EncodingToString(e);
            }
        }
        private void Content_SaveStatusChangedEvent(TextControlBox sender, bool IsModified)
        {
            if (Statusbar != null)
            {
                //if (appsettings.GetSettingsAsBool("ShowColorsForSaveStatusButton", true))
                //    SaveStatusDisplay.Foreground = IsModified ? DefaultValues.WrongInput_Color : DefaultValues.CorrectInput_Color;
                SaveStatusDisplay.Content = IsModified ? appsettings.GetResourceString("MainPage_Statusbar_Unsaved/Text") : appsettings.GetResourceString(" MainPage_Statusbar_Saved/Text");
            }
        }
        private void StatusbarButton_PointerEnterExit(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.Foreground = new SolidColorBrush(StatusbarForegroundColor);
                Statusbar.Background = new SolidColorBrush(StatusbarBackgroundColor);
            }
            //if (appsettings.GetSettingsAsBool("ShowColorsForSaveStatusButton", true))
            //  SaveStatusDisplay.Foreground = tabActions.GetTextBoxFromSelectedTabPage().IsModified ? DefaultValues.WrongInput_Color : DefaultValues.CorrectInput_Color;
            //else SaveStatusDisplay.Foreground = LineNumberDisplay.Foreground;
        }
        private void ZoomDisplay_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (CurrentlySelectedTabPage != null)
            {
                int delta = e.GetCurrentPoint((UIElement)sender).Properties.MouseWheelDelta;

                if (delta < 0)
                {
                    tabpagehelper.ZoomOut(CurrentlySelectedTabPage);
                }

                if (delta > 0)
                {
                    tabpagehelper.ZoomIn(CurrentlySelectedTabPage);
                }
            }
        }
        private void Textbox_WordCountChangedEvent(TextControlBox sender, int Words)
        {
            WordCountDisplay.Content = appsettings.GetResourceString("MainPage_Statusbar_Wordcount/Text") + ": " + Words;
        }

        //Zoom-flyout//
        private void ZoomFlyoutSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (tabactions != null && preventZoomOnFactorChanged == false)
            {
                if (e.OldValue != e.NewValue && ZoomFlyoutSlider.Value != 0)
                {
                    TextControlBox textbox = tabactions.GetTextBoxFromSelectedTabPage();
                    if (textbox != null)
                    {
                        textbox.SetFontZoomFactor(ZoomFlyoutSlider.Value);
                    }
                }
            }
        }
        
        //Rename-flyout//
        private async void TryRenameFile()
        {
            if (StringBuilder.IsValidFilename(RenameTextBox.Text))
            {
                RenameTextBox.BorderBrush = DefaultValues.CorrectInput_Color;
                if (CurrentlySelectedTabPage != null)
                {
                    if (RenameTextBox.Text != tabpagehelper.GetTabHeader(CurrentlySelectedTabPage))
                    {
                        if (await tabactions.RenameFile(RenameTextBox.Text))
                        {
                            RenameFlyout.Hide();
                        }
                    }
                }
            }
            else
            {
                RenameTextBox.BorderBrush = DefaultValues.WrongInput_Color;
                RenameFileButton.IsEnabled = false;
            }
        }
        private void RenameTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            e.Handled = true;
            if (TextTabControl != null && e.Key == VirtualKey.Enter)
            {
                TryRenameFile();
                RenameFlyout.Hide();
            }
            if (e.Key == VirtualKey.Escape)
            {
                HideFlyoutIfOpened(RenameFlyout);
            }
        }
        private void RenameFileButton_Click(object sender, RoutedEventArgs e)
        {
            TryRenameFile();
        }
        private void RenameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox tb = sender as TextBox;
                if (StringBuilder.IsValidFilename(tb.Text))
                {
                    tb.BorderBrush = DefaultValues.CorrectInput_Color;
                    RenameFileButton.IsEnabled = true;
                }
                else
                {
                    tb.BorderBrush = DefaultValues.WrongInput_Color;
                    RenameFileButton.IsEnabled = false;
                }
            }
        }
        private void RenameFlyout_Opened(object sender, object e)
        {
            TextControlBox textbox = tabactions.GetTextBoxFromSelectedTabPage();
            if (textbox != null)
            {
                RenameTextBox.Text = textbox.Header;
                string ext = Path.GetExtension(RenameTextBox.Text);
                RenameTextBox.Select(0, RenameTextBox.Text.Length - ext.Length);
            }
        }
        private void RenameTextBox_FocusEngaged(Control sender, FocusEngagedEventArgs args)
        {
            RenameTextBox.BorderBrush = new SolidColorBrush(Colors.Gray);
        }
        private void OpenRenameFlyout()
        {
            RenameFlyout.ShowAt(FileNameDisplay);
        }
        
        //Go to line-flyout / Go to line dialog//
        private bool DoGoToLine(TextBox sender)
        {
            if (sender is TextBox)
            {
                TextBox tb = sender;
                TextControlBox textbox = tabactions.GetTextBoxFromSelectedTabPage();
                if (StringBuilder.IsAllNumber(tb.Text) && textbox != null)
                {
                    if (!tb.Text.Equals("0", StringComparison.Ordinal) && tb.Text.Length != 0)
                    {
                        int EnteredLineNumber = Convert.ToInt(tb.Text);
                        if (EnteredLineNumber <= textbox.GetLinesCount && EnteredLineNumber > 0)
                        {
                            GoToLineWindow.BorderBrush = new SolidColorBrush(Colors.Gray);
                            textbox.GoToLine(EnteredLineNumber);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private void GoToLinebuttonClick(object sender, RoutedEventArgs e)
        {
            DoGoToLine(LineNumberTextBox);
        }
        private void LineNumberTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                DoGoToLine(sender as TextBox);
            }
        }
        private void LineNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                bool succed = false;
                TextBox tb = sender as TextBox;
                TextControlBox textbox = tabactions.GetTextBoxFromSelectedTabPage();
                if (StringBuilder.IsAllNumber(tb.Text) && textbox != null)
                {
                    if (!tb.Text.Equals("0", StringComparison.Ordinal) && tb.Text.Length != 0)
                    {
                        int EnteredLineNumber = Convert.ToInt(tb.Text);
                        if (EnteredLineNumber <= textbox.GetLinesCount && EnteredLineNumber > 0)
                        {
                            succed = true;
                        }
                    }
                }
                GoToLineWindow.BorderBrush = succed ? DefaultValues.CorrectInput_Color : DefaultValues.WrongInput_Color;

            }
        }
        
        //Encoding-flyout
        private void AddEncodingButtonsToStatusbar()
        {
            //if the Encodingflyout has more then the two default buttons return and don't add them again
            if (EncodingFlyout.Items.Count > 2)
                return;

            for (int i = 0; i < Encodings.AllEncodingNames.Count; i++)
            {
                var openwithitem = new MenuFlyoutItem
                {
                    Text = Encodings.AllEncodingNames[i]
                };
                openwithitem.Click += Openwithitem_Click;
                OpenWithEncodingButton.Items.Add(openwithitem);
                var item = new MenuFlyoutItem
                {
                    Text = Encodings.AllEncodingNames[i]
                };
                item.Click += EncodingButton_Click;
                EncodingFlyout.Items.Add(item);
            }
        }
        private void EncodingButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem selecteditem)
            {
                if (CurrentlySelectedTabPage_Textbox != null)
                {
                    CurrentlySelectedTabPage_Textbox.Encoding = Encodings.StringToEncoding(selecteditem.Text);
                    tabpagehelper.SetTabModified(CurrentlySelectedTabPage, true);
                }
            }
        }
        private async void Openwithitem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                LoadingProgressRing.Visibility = Visibility.Visible;

                await Task.Delay(100); //idk. why but without it the Progressbar is hidden
                await tabactions.OpenFileWithEncoding(CurrentlySelectedTabPage, Encodings.StringToEncoding(item.Text));
                LoadingProgressRing.Visibility = Visibility.Collapsed;
            }
        }

        //Functions to handle the secondary windows
        private async Task<int> GetViewIndexFromAppView(int ViewId)
        {
            int returnval = -1;
            for (int i = 0; i < OpenedSecondaryViews.Count; i++)
            {
                await OpenedSecondaryViews[i].CoreApplicationView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (ApplicationView.GetForCurrentView().Id == ViewId)
                    {
                        returnval = i;
                    }
                });
            }
            return returnval;
        }
        public async Task ExpandTabPageToNewView(muxc.TabViewItem TabPage)
        {
            var textcontrolbox = tabactions.GetTextBoxFromTabPage(TabPage);

            //Run code only if tabpage is a textbox:
            if (textcontrolbox == null) return;

            string TabPageName = TabPage.Name;
            string Header = textcontrolbox.Header;
            string tbtext = textcontrolbox.GetText();
            double tbZoomFactor = textcontrolbox._zoomFactor;
            TextWrapping wrapping = textcontrolbox.WordWrap;
            bool IsReadOnly = textcontrolbox.IsReadOnly;
            int SelectionStart = textcontrolbox.SelectionStart;
            ApplicationView appview = null;
            CoreApplicationView newView = CoreApplication.CreateNewView();

            int newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame frame = new Frame();
                frame.Navigate(typeof(TextboxViewPage));
                Window.Current.Content = frame;
                Window.Current.Activate();

                if (frame.Content is TextboxViewPage tbviewpage)
                {
                    tbviewpage.TabPageName = TabPageName;
                    tbviewpage.Title = Header;
                    tbviewpage.TextboxText = tbtext;
                    tbviewpage.SelectionStart = SelectionStart;
                    tbviewpage.Textwrapping = wrapping;
                    tbviewpage.ZoomFactor = tbZoomFactor;
                    tbviewpage.IsReadonly = IsReadOnly;
                }

                appview = ApplicationView.GetForCurrentView();
                newViewId = appview.Id;
                appview.Consolidated += TextboxViewPage_Consolidated;
            });

            TabPage.Visibility = Visibility.Collapsed;

            //if all tabs are in a secondary view, create a new tab
            if (tabactions.GetShownTabPages() < 1)
            {
                muxc.TabViewItem tab = tabactions.NewTab();
                if (tab != null)
                {
                    TextTabControl.SelectedItem = tab;
                    await tabactions.SaveAllTabChanges();
                    SetSettingsToTabPage(tab, TextBoxMargin());
                }
            }

            await tabactions.GetTextBoxFromTabPage(TabPage).SetText("Cleared the textbox");
            for (int i = 0; i < TextTabControl.TabItems.Count; i++)
            {
                if (TextTabControl.TabItems[i] is muxc.TabViewItem Tab)
                {
                    if (Tab.Visibility == Visibility.Visible)
                    {
                        TextTabControl.SelectedItem = Tab;
                        break;
                    }
                }
            }

            if (!await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId))
            {
                ShowInfobar(InfoBarMessages.CouldNotOpenInNewView, InfoBarMessages.CouldNotOpenInNewViewTitle,  muxc.InfoBarSeverity.Error);
                return;
            }
            OpenedSecondaryViews.Add(new OpenedSecondaryViewItem
            {
                ApplicationView = appview,
                CoreApplicationView = newView
            });
        }
        private async Task<bool> CloseExpandedView(ApplicationView sender, bool RemoveFromOpenedViewList = true)
        {
            int index = await GetViewIndexFromAppView(sender.Id);
            if (index < 0 || index >= OpenedSecondaryViews.Count)
                return false;

            TextboxViewPage tbvpage = null;
            string TabPageName = "";

            //Get the view by the index returned from the GetViewIndexFromAppView
            var view = OpenedSecondaryViews[index];
            await view.CoreApplicationView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Window.Current.Content is Frame frame)
                {
                    if (frame.Content is TextboxViewPage tbvp)
                    {
                        tbvpage = tbvp;
                        TabPageName = tbvpage.TabPageName;
                    }
                }
            });
            if (tbvpage == null)
                return false;
            double ZoomFactor = tbvpage.ZoomFactor;
            bool IsReadOnly = tbvpage.IsReadonly;
            string TextBoxText = tbvpage.TextboxText;
            int SelectionStart = tbvpage.SelectionStart;
            bool IsModified = tbvpage.IsModified;

            if (tbvpage != null)
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if (TextTabControl.FindName(TabPageName) is muxc.TabViewItem tabpage)
                    {
                        var textbox = tabactions.GetTextBoxFromTabPage(tabpage);
                        if (textbox != null)
                        {
                            await textbox.SetText(TextBoxText);
                            textbox.SetFontZoomFactor(ZoomFactor);
                            textbox.IsReadOnly = IsReadOnly;
                            textbox.SelectionStart = SelectionStart;
                            tabpagehelper.SetTabModified(tabpage, IsModified);
                        }
                        tabpage.Visibility = Visibility.Visible;
                        TextTabControl.SelectedItem = tabpage;
                    }
                });
                if (RemoveFromOpenedViewList)
                    return OpenedSecondaryViews.Remove(view);
                return true;
            }
            return false;
        }
        private async void TextboxViewPage_Consolidated(ApplicationView sender, ApplicationViewConsolidatedEventArgs args)
        {
            await CloseExpandedView(sender);
        }
    }
    
    public class SettingsNavigationParameter
    {
        public muxc.TabView Tabcontrol { get; set; }
        public MainPage Mainpage { get; set; }
        public string PageToNavigateTo { get; set; } = "";
    }
    public class OpenedSecondaryViewItem
    {
        public CoreApplicationView CoreApplicationView { get; set; }
        public ApplicationView ApplicationView { get; set; }
    }
    public class CommandLineLaunchNavigationParameter
    {
        public string Arguments { get; set; }
        public string CurrentDirectoryPath { get; set; }
    }
}
