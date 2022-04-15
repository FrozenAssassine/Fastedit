using Fastedit.Controls.Textbox;
using Fastedit.Dialogs;
using Fastedit.Extensions;
using Fastedit.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using static System.Net.WebRequestMethods;
using muxc = Microsoft.UI.Xaml.Controls;
using Path = System.IO.Path;
using StringBuilder = Fastedit.Extensions.StringBuilder;

namespace Fastedit.Core.Tab
{
    public class TabActions
    {
        //Class implementations
        private readonly muxc.TabView TextTabControl;
        private readonly static AppSettings appsettings = new AppSettings();
        private readonly static TabDataBase tabdatabase = new TabDataBase();
        private readonly MainPage mainpage = null;
        private readonly static TabPageHelper tabpagehelper = new TabPageHelper();
        private readonly static SaveFileHelper savefilehelper = new SaveFileHelper();

        //Other variables
        public muxc.TabViewItem SettingsTabPage = null;
        private readonly DispatcherTimer CloseTabsTimerToSaveDatabase = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 5)
        };

        public void ShowInfobar(string Content, string Title = "", muxc.InfoBarSeverity severity = muxc.InfoBarSeverity.Warning, int Time = 5)
        {
            mainpage.ShowInfobar(Content, Title, severity, Time);
      
        }
        public void ShowInfobar(muxc.InfoBar infobar)
        {
            mainpage.ShowInfobar(infobar);
        }

        //Initialisation
        public TabActions(muxc.TabView TabControl, MainPage mainpage = null)
        {
            this.mainpage = mainpage;
            if (this.mainpage == null)
                this.mainpage = new MainPage();

            TextTabControl = TabControl;
            CloseTabsTimerToSaveDatabase.Tick += CloseTabsTimerToSaveDatabase_Tick;
        }

        public bool SettingsPageOpened()
        {
            return SettingsTabPage != null;
        }
        public int GetTabItemCount()
        {
            if (SettingsPageOpened())
            {
                return TextTabControl.TabItems.Count - 1;
            }
            else
            {
                return TextTabControl.TabItems.Count;
            }
        }
        public object[] GetTabItemArray()
        {
            if (SettingsPageOpened())
            {
                var lst = TextTabControl.TabItems.ToList();
                lst.Remove(SettingsTabPage);
                return lst.ToArray();
            }
            else
            {
                return TextTabControl.TabItems.ToArray();
            }
        }
        public IList<object> GetTabItems()
        {
            if (SettingsPageOpened())
            {
                var lst = TextTabControl.TabItems.ToList();
                lst.Remove(SettingsTabPage);
                return lst;
            }
            else
            {
                return TextTabControl.TabItems;
            }
        }
        //Get the number of actual visible tabpages:
        public int GetShownTabPages(bool WithSettingsTab = false)
        {
            int VisibleItemCount = 0;
            for (int i = 0; i < TextTabControl.TabItems.Count; i++)
            {
                if (TextTabControl.TabItems[i] is muxc.TabViewItem Tab)
                {
                    if (Tab.Visibility == Visibility.Visible)
                        VisibleItemCount++;
                    if (WithSettingsTab && SettingsTabPage != null)
                        VisibleItemCount++;
                }
            }
            return VisibleItemCount;
        }

        //Events
        private void TextControlBox_TextChangedevent(TextControlBox sender)
        {
            tabpagehelper.SetTabModified(GetSelectedTabPage(), true);
        }
        private void TabPage_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            TabPageFlyout.CreateFlyoutForTab(
                TextTabControl, mainpage, sender, tabpagehelper.GetTabReadOnly(sender as muxc.TabViewItem)
                ).ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }
        private void Tcb_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(sender as UIElement);
            if (point.Properties.IsXButton2Pressed)
            {
                SelectNextTab();
            }
            else if (point.Properties.IsXButton1Pressed)
            {
                SelectPreviousTab();
            }
        }

        //Get Textbox & Tabpage
        public TextControlBox GetTextBoxFromSelectedTabPage()
        {
            if (TextTabControl.SelectedIndex < GetTabItemCount() && TextTabControl.SelectedIndex > -1)
            {
                if (GetTabItemArray()[TextTabControl.SelectedIndex] is muxc.TabViewItem tab)
                {
                    if (tab.Content is TextControlBox tb)
                        return tb;
                }
            }
            return null;
        }
        public TextControlBox GetTextBoxFromTabPage(muxc.TabViewItem TabPage)
        {
            if (TabPage != null)
            {
                if (TabPage.Content is TextControlBox tb)
                    return tb;
            }
            return null;
        }
        public muxc.TabViewItem GetSelectedTabPage()
        {
            if (TextTabControl.SelectedItem is muxc.TabViewItem Tab)
                return Tab;
            return null;
        }
        public muxc.TabViewItem GetTabPageByName(string Name)
        {
            var control = TextTabControl.FindName(Name);
            if (control == null) return null;
            if (control is muxc.TabViewItem tab)
                return tab;
            return null;
        }

        //New Tab
        public muxc.TabViewItem NewTab()
        {
            var TabPage = NewTabPage();
            var tcb = NewTextBox();
            tcb.IsModified = false;
            tcb.Header = NewTabTitleName();
            tcb.IdentifierName = NewTabName();
            tcb.FilePath = string.Empty;
            tcb.FileToken = string.Empty;

            TabPage.Header = tcb.Header;
            TabPage.Content = tcb;
            TabPage.Name = $"{tcb.IdentifierName}.Tab";

            TextTabControl.TabItems.Add(TabPage);
            TextTabControl.SelectedItem = TabPage;
            return TabPage;
        }
        public muxc.TabViewItem NewAllParametersTab(TabDataForDatabase document, bool AddToTabControl = true, StorageFile file = null)
        {
            TextControlBox tcb = NewTextBox();
            muxc.TabViewItem TabPage = NewTabPage();
            
            tcb.TextBeforeLastSaved = string.Empty;
            tcb.FontSizeWithoutZoom = appsettings.GetSettingsAsInt("FontSize", DefaultValues.DefaultFontsize);
            tcb.IsModified = document.TabModified;
            tcb.Header = document.TabHeader;
            tcb.IdentifierName = document.TabName;
            tcb.TempFile = document.TabTemp;
            tcb.FilePath = document.TabPath;
            tcb.DataBaseName = document.DataBaseName;
            tcb.FileToken = document.TabToken;
            tcb.IsReadOnly = document.TabReadOnly;
            tcb.SetFontZoomFactor(document.ZoomFactor);
            tcb.WordWrap = document.WordWrap;
            tcb.TabSaveMode = document.TabSaveMode;
            tcb.tabdatafromdatabase = document;

            if (file != null)
            {
                tcb.Storagefile = file;
            }

            tcb.Encoding = Encodings.IntToEncoding(document.TabEncoding);
            TabPage.Content = tcb;
            TabPage.Name = $"{tcb.IdentifierName}.Tab";
            TabPage.Header = tcb.Header;
            if (document.TabReadOnly)
                TabPage.IconSource = new muxc.SymbolIconSource() { Symbol = Symbol.ProtectedDocument };
            else
                TabPage.IconSource =
                    new muxc.FontIconSource
                    {
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        Glyph = appsettings.GetSettingsAsString("TabIconId",
                        DefaultValues.DefaultTabIconId)
                    };

            tcb.Margin = mainpage.TextBoxMargin();
            if (AddToTabControl == true)
            {
                TextTabControl.TabItems.Add(TabPage);
                TextTabControl.SelectedItem = TabPage;
            }
            return TabPage;
        }
        private TextControlBox NewTextBox()
        {
            TextControlBox tcb = new TextControlBox();
            tcb.TextChangedevent += TextControlBox_TextChangedevent;
            tcb.PointerPressed += Tcb_PointerPressed;
            tcb.FilePath = string.Empty;
            tcb.FontFamily = new FontFamily(appsettings.GetSettingsAsString("FontFamily", DefaultValues.DefaultFontFamily));
            tcb.Encoding = Encoding.Default;
            tcb.TabSaveMode = TabSaveMode.SaveAsTemp;
            tcb.Storagefile = null;
            tcb.WordWrap = TextWrapping.NoWrap;
            tcb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tcb.VerticalAlignment = VerticalAlignment.Stretch;
            tcb.MarkdownPreview = false;
            tcb.IsHandWritingEnabled = appsettings.GetSettingsAsBool("HandwritingEnabled", DefaultValues.HandWritingEnabled);
            tcb.ShowSelectionFlyout = appsettings.GetSettingsAsBool("TextboxShowSelectionFlyout", false);
            //Designs
            tcb.FontSize = appsettings.GetSettingsAsInt("FontSize", DefaultValues.DefaultFontsize);
            tcb.LineNumberForeground = appsettings.GetSettingsAsColorWithDefault("LineNumberForegroundColor", DefaultValues.DefaultLineNumberForegroundColor);
            tcb.LineNumberBackground = appsettings.GetSettingsAsColorWithDefault("LineNumberBackgroundColor", DefaultValues.DefaultLineNumberBackgroundColor);
            tcb.ShowLineNumbers = appsettings.GetSettingsAsBool("ShowLineNumbers", true);
            tcb.Background = appsettings.GetSettingsAsColorWithDefault("TextBackgroundColor", DefaultValues.DefaultTextBackgroundColor);
            tcb.TextColor = appsettings.GetSettingsAsColorWithDefault("TextColor", DefaultValues.DefaultTextColor);
            tcb.TextSelectionColor = appsettings.GetSettingsAsColorWithDefault("TextSelectionColor", DefaultValues.DefaultTextSelectionColor);
            tcb.SpellChecking = appsettings.GetSettingsAsBool("Spellchecking", DefaultValues.SpellCheckingEnabled);
            return tcb;
        }
        private muxc.TabViewItem NewTabPage()
        {
            var TabPage = new muxc.TabViewItem();
            TabPage.RightTapped += TabPage_RightTapped;
            TabPage.IconSource =
                new muxc.FontIconSource
                {
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Glyph = appsettings.GetSettingsAsString("TabIconId", DefaultValues.DefaultTabIconId)
                };
            TabPage.CornerRadius = DefaultValues.DefaultTabCornerRadius;
            TabPage.HorizontalAlignment = HorizontalAlignment.Stretch;
            TabPage.VerticalAlignment = VerticalAlignment.Stretch;
            return TabPage;
        }

        private string NewTabName()
        {
            int count = 1;
            string TabName = "Tab" + count;

            if (GetTabItemCount() < 1)
            {
                return TabName;
            }
            else
            {
                while (count < GetTabItemCount() + 1)
                {
                    TabName = "Tab" + count;
                    count++;

                    if (GetTabPageByName(TabName + ".Tab") == null)
                    {
                        break;
                    }
                    else
                    {
                        TabName = "Tab" + count;
                    }
                }
                return TabName;
            }
        }
        private string NewTabTitleName()
        {
            var NewDocName = appsettings.GetSettingsAsString("NewTabTitleName", DefaultValues.NewDocName);
            string Extension = Path.GetExtension(NewDocName);
            string outname = StringBuilder.ReplaceLastOccurenceInString(NewDocName, Extension, "");
            int doccount = 0;

            for (int i = 0; i < GetTabItemCount(); i++)
            {
                if (TabHasTitle(outname + doccount))
                {
                    doccount++;
                }
                else
                {
                    break;
                }
            }
            return outname + doccount + Extension;
        }
        private bool TabHasTitle(string Title)
        {
            var TabItems = GetTabItems();
            for (int i = 0; i < TabItems.Count; i++)
            {
                if (TabItems[i] is muxc.TabViewItem tab)
                {
                    string Header = tab.Header.ToString();
                    string ext = Path.GetExtension(Header);
                    if (ext != null && ext != string.Empty)
                    {
                        if (Header.Remove(Header.Length - ext.Length) == Title)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void SelectNextTab()
        {
            int IndexToSelect = TextTabControl.SelectedIndex + 1;
            if (TextTabControl.TabItems.Count > IndexToSelect && IndexToSelect >= 0)
            {
                TextTabControl.SelectedIndex = IndexToSelect;
            }
        }
        public void SelectPreviousTab()
        {
            int IndexToSelect = TextTabControl.SelectedIndex - 1;
            if (TextTabControl.TabItems.Count > IndexToSelect && IndexToSelect >= 0)
            {
                TextTabControl.SelectedIndex = IndexToSelect;
            }
        }

        //Tabs:
        public async Task<bool> FileExist(muxc.TabViewItem Tab)
        {
            try
            {
                await StorageApplicationPermissions.FutureAccessList.GetFileAsync(tabpagehelper.GetTabToken(Tab));
                return true;
            }
            catch (FileNotFoundException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
            catch (ArgumentException) { return false; }
        }

        /// <summary>
        /// Moves the file to the inbuild recyclebin
        /// </summary>
        /// <param name="Tab">The file to move to</param>
        /// <returns>Wether the function succeed or failed</returns>
        private async Task<bool> MoveFileToRecycleBin(muxc.TabViewItem Tab)
        {
            if (GetTextBoxFromTabPage(Tab) is TextControlBox textbox)
            {
                StorageFolder recylcebinfolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(DefaultValues.RecycleBin_FolderName, CreationCollisionOption.OpenIfExists);
                StorageFile file = await recylcebinfolder.CreateFileAsync(textbox.Header, CreationCollisionOption.GenerateUniqueName);

                return await savefilehelper.WriteTextToFile(file, textbox.Encoding, textbox.GetText(), textbox.TabSaveMode);
            }
            return false;
        }
        public async Task<bool> OpenFileFromRecylceBin(StorageFile file, string Filename)
        {
            try
            {
                if (file != null)
                {
                    var (Succed, TabPage) = await DoOpenFile(TabPage: null, file: file, TabSaveMode.SaveAsTemp, SaveDatabase: false, false, "", false, true, true);
                    var textbox = tabpagehelper.GetTextBoxFromTabPage(TabPage);
                    if (textbox == null)
                        return false;

                    textbox.FilePath = "";
                    textbox.FileToken = "";
                    textbox.Storagefile = null;
                    textbox.DataBaseName = "";

                    if (!TextTabControl.TabItems.Contains(TabPage))
                    {
                        TextTabControl.TabItems.Add(TabPage);
                        TextTabControl.SelectedIndex = GetTabItemCount() - 1;
                    }
                    tabpagehelper.SetTabStorageFile(TabPage, file);
                    return Succed;
                }
            }
            catch (Exception e)
            {
                ShowInfobar(ErrorDialogs.OpenErrorDialog(e.Message));
            }
            return false;
        }

        //Close and Load Tabs
        /// <summary>
        /// Saves all tab data to the database
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SaveAllTabChanges(StorageFolder folder = null)
        {
            try
            {
                tabdatabase.DeleteAllTemporarySavedFiles(folder == null ? "" : folder.Path);
                int TabItemCount = GetTabItemCount();
                if (TabItemCount == 0)
                    return true;

                if (folder == null)
                    folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(DefaultValues.Database_FolderName, CreationCollisionOption.OpenIfExists);

                for (int i = 0; i < TabItemCount; i++)
                {
                    if (GetTabItems()[i] is muxc.TabViewItem Tab)
                    {
                        await SaveTempFile(Tab, folder);
                    }
                    else
                    {
                        ShowInfobar(ErrorDialogs.GetTabErrorDialog());
                    }
                }
                return await tabdatabase.SaveTabPages(TextTabControl, folder.Path);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in TabActions --> SaveAllTabChanges:" + "\n" + e.Message);
                return false;
            }
        }
        public async Task<bool> SaveAllTabChangesToBackup()
        {
            StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(DefaultValues.Backup_FolderName, CreationCollisionOption.OpenIfExists);
            return await SaveAllTabChanges(folder);
        }

        /// <summary>
        /// Close tabs and save to database on application exit
        /// </summary>
        /// <returns>the status of the function</returns>
        public async Task<bool> CloseTabs()
        {
            try
            {
                if (GetTabItemCount() == 0)
                    return await SaveAllTabChanges();

                //Check whether to save tabs, and reopen on restart or show asktosave-dialog and close them
                if (appsettings.GetSettingsAsBool("LoadRecentTabs", true))
                {
                    if (await SaveAllTabChanges() == false)
                    {
                        throw new Exception();
                    }
                    return true;
                }
                else
                {
                    var items = TextTabControl.TabItems.ToList();
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i] is muxc.TabViewItem Tab)
                        {
                            if(Tab.Content is TextControlBox tb)
                            {
                                if (tabpagehelper.GetIsModified(Tab))
                                {
                                    bool succed = await savefilehelper.ShowAskSaveDialogForTab(Tab, false);
                                    if (succed == false)
                                        return false;
                                }
                                if (await RemoveTab(Tab) == false)
                                    return false;
                            }
                        }
                    }
                    return true;
                }
            }
            catch
            {
                var mb = new InfoBox(AppSettings.GetResourceStringStatic("ErrorDialogs_SaveDataBaseError/Text"), appsettings.GetResourceString("ErrorDialogs_Title_DataBaseError/Text"))
                {
                    CloseButtonText = AppSettings.GetResourceStringStatic("ErrorDialogs_SaveDataBaseError_Close/Text"),
                    PrimaryButtonText = AppSettings.GetResourceStringStatic("ErrorDialogs_SaveDataBaseError_Retry/Text"),
                    SecondaryButtonText = AppSettings.GetResourceStringStatic("ErrorDialogs_SaveDataBaseError_CloseAnyway/Text"),
                };
                var dlgres = await mb.ShowAsync();
                if (dlgres == ContentDialogResult.Primary)
                {
                    return await CloseTabs();
                }
                else if (dlgres == ContentDialogResult.Secondary)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Loads the tabs from the database and retrives everything from the last saved season
        /// </summary>
        /// <param name="LoadFromBackup">Whether load the tabs from latest backup or from the default database folder</param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public async Task<bool> LoadTabs(bool LoadFromBackup, StorageFolder folder = null)
        {
            try
            {
                //Datalist for tab to save tabpages to 
                var tabdatafromdatabase = await tabdatabase.GetTabData(LoadFromBackup);
                if (tabdatafromdatabase == null)
                    return false;

                int LastSelectedTabIndex = 0;
                var MethodeSucced = true;

                TextTabControl.TabItems.Clear();
                string[] tempFilePaths = Directory.GetFiles(
                       Path.Combine(ApplicationData.Current.LocalFolder.Path,
                       LoadFromBackup ? DefaultValues.Backup_FolderName : DefaultValues.Database_FolderName)
                       );

                async Task<(string Text, Encoding encoding, bool succed)> getTabtext(TextControlBox textbox)
                {
                    for (int fc = 0; fc < tempFilePaths.Length; fc++)
                    {
                        if (Path.GetFileName(tempFilePaths[fc]) == textbox.TempFile)
                        {
                            return await ReadTextFromPathAsync(tempFilePaths[fc]);
                        }
                    }
                    return ("", null, false);
                }

                //Create a list with the tabs and store them into it
                List<muxc.TabViewItem> TabItemList = new List<muxc.TabViewItem>();
                for (int i = 0; i < tabdatafromdatabase.Count; i++)
                {
                    TabDataForDatabase DataItem = tabdatafromdatabase[i];

                    var Tab = NewAllParametersTab(DataItem, false);

                    if (Tab.Content is TextControlBox textbox)
                    {
                        var result = await getTabtext(textbox);
                        if (result.succed)
                        {
                            await textbox.ChangeText(result.Text);

                            //Set Selstart / Selend
                            textbox.SetSelection(DataItem.TabSelStart, DataItem.TabSelLenght);
                            textbox.MarkdownPreview = DataItem.Markdown;
                            if(DataItem.TabToken != string.Empty)
                            {
                                var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(DataItem.TabToken);
                                if (file != null)
                                    textbox.Storagefile = file;
                            }
                            tabpagehelper.SetTabModified(Tab, false);
                            
                            LastSelectedTabIndex = DataItem.CurrentSelectedTabIndex;
                            TabItemList.Add(Tab);
                        }
                        else
                        {
                            MethodeSucced = false;
                        }
                    }
                }

                //When an error happened
                if (!MethodeSucced)
                {
                    ShowInfobar(ErrorDialogs.LoadTabsError());
                    return false;
                }
                //Add the items from list to TabControl
                for (int i = 0; i < TabItemList.Count; i++)
                {
                    TextTabControl.TabItems.Add(TabItemList[i]);
                }

                //Select the recent opened tab
                if (TextTabControl.TabItems.Count <= LastSelectedTabIndex)
                {
                    TextTabControl.SelectedIndex = LastSelectedTabIndex - 1 > 0 ? LastSelectedTabIndex - 1 : 0;
                }
                else
                {
                    TextTabControl.SelectedIndex = LastSelectedTabIndex;
                }

                return MethodeSucced;
            }
            catch
            {
                ShowInfobar(ErrorDialogs.LoadTabsError());
            }
            return false;
        }

        /// <summary>
        /// Removes tab from TabView and clears all data from memory
        /// </summary>
        /// <param name="Tab"></param>
        /// <returns></returns>
        public async Task<bool> RemoveTab(muxc.TabViewItem Tab)
        {
            try
            {
                if (tabpagehelper.GetIsModified(Tab))
                {
                    await MoveFileToRecycleBin(Tab);
                }

                var token = tabpagehelper.GetTabToken(Tab);
                //Remove Token from futureaccesslist
                if (StorageApplicationPermissions.FutureAccessList.ContainsItem(token) && token.Length > 0)
                {
                    StorageApplicationPermissions.FutureAccessList.Remove(token);
                }

                return TextTabControl.TabItems.Remove(Tab);
            }
            catch (Exception e)
            {
                await new InfoBox("Remove tab error\n" + e.Message, "Error").ShowAsync();
                return false;
            }
        }
        private async Task<bool> CloseTab(muxc.TabViewItem Tab)
        {
            try
            {
                if (Tab.Visibility == Visibility.Collapsed)
                    return true;
                var textbox = GetTextBoxFromTabPage(Tab);
                if (textbox != null)
                {
                    //If the tab is newly created and has no content
                    if (textbox.GetText().Length == 0 && textbox.IsModified == false && textbox.FilePath.Length == 0 && textbox.DataBaseName.Length == 0)
                    {
                        return await RemoveTab(Tab);
                    }
                    else
                    {
                        var tabsavemode = textbox.TabSaveMode;
                        if (tabsavemode == TabSaveMode.SaveAsTemp)
                        {
                            ContentDialogResult dlgres;
                            //Choose whether use the AskSaveDialogNeverSaved or AskSaveDialog
                            if (textbox.FileToken.Length == 0)
                                dlgres = await SaveDialogs.AskSaveDialogNeverSaved(Tab);
                            else
                                dlgres = await SaveDialogs.AskSaveDialog(Tab);

                            //If Save-button is pressed:
                            if (dlgres == ContentDialogResult.Primary)
                            {
                                if (await savefilehelper.Save(Tab))
                                {
                                    return await RemoveTab(Tab);
                                }
                                return false;
                            }
                            //If don't-save-button is pressed:
                            else if (dlgres == ContentDialogResult.Secondary)
                            {
                                return await RemoveTab(Tab);
                            }
                            //If cancel-button is pressed:
                            return false;
                        }
                        else if (tabsavemode == TabSaveMode.SaveAsFile || tabsavemode == TabSaveMode.SaveAsDragDrop)
                        {
                            //If the file exists
                            if (await FileExist(Tab))
                            {
                                if (textbox.IsModified)
                                {
                                    bool res = await savefilehelper.ShowAskSaveDialogForTab(Tab, false);
                                    if (res)
                                    {
                                        return await RemoveTab(Tab);
                                    }
                                }
                                else
                                {
                                    return await RemoveTab(Tab);
                                }
                            }
                            //If the file doesn't exist
                            else
                            {
                                bool res = await savefilehelper.ShowAskSaveDialogForTab(Tab, true);
                                if (res)
                                {
                                    return await RemoveTab(Tab);
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in TabActions -> CloseTab\n" + e.Message);

                ShowInfobar(ErrorDialogs.CloseTabError());
            }
            return false;
        }
        public async Task CloseAllButThis(muxc.TabViewItem Tab)
        {
            List<object> tabs = TextTabControl.TabItems.ToList();
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i] != null && tabs[i] != Tab)
                    if (await CloseTab(tabs[i] as muxc.TabViewItem) == false)
                        return;
            }
            await SaveAllTabChanges();
        }
        public async Task CloseAllLeft(muxc.TabViewItem Tab)
        {
            int CurrentTabIndex = TextTabControl.TabItems.IndexOf(Tab);
            var tabrange = TextTabControl.TabItems.ToList().GetRange(0, CurrentTabIndex);
            for (int i = 0; i < tabrange.Count; i++)
            {
                if (tabrange[i] != null)
                {
                    if (await CloseTab(tabrange[i] as muxc.TabViewItem) == false)
                        return;
                }
            }

            await SaveAllTabChanges();
        }
        public async Task<bool> CloseAllRight(muxc.TabViewItem Tab)
        {
            int CurrentTabIndex = TextTabControl.TabItems.IndexOf(Tab);
            var TabItemCount = GetTabItemCount();
            if (CurrentTabIndex < 0 || CurrentTabIndex >= TabItemCount)
                return false;
            var tabrange = TextTabControl.TabItems.ToList().GetRange(CurrentTabIndex + 1, TabItemCount - CurrentTabIndex -1);
            for (int i = 0; i< tabrange.Count; i++)
            {
                if (tabrange[i] != null)
                {
                    if (await CloseTab(tabrange[i] as muxc.TabViewItem) == false)
                        return false;
                }
            }
            return await SaveAllTabChanges();
        }
        public async Task CloseAllWithoutSave()
        {
            var tabs = GetTabItems();
            for (int i = 0; i<tabs.Count; i++)
            {
                if(tabs[i] != null)
                    await RemoveTab(tabs[i] as muxc.TabViewItem);
            }

            await SaveAllTabChanges();
        }
        public async Task CloseAllTabs()
        {
            List<object> TabsToRemove = TextTabControl.TabItems.ToList();
            for (int i = 0; i < TabsToRemove.Count; i++)
            {
                var tab = TabsToRemove[i];
                if(tab != null)
                {
                    if (await CloseTab(tab as muxc.TabViewItem) == false)
                        return;
                }
            }
            await SaveAllTabChanges();
        }

        public async Task<bool> CloseTabAndSaveDataBase(muxc.TabViewItem Tab)
        {
            bool res = await CloseTab(Tab);
            CloseTabsTimerToSaveDatabase.Stop();
            CloseTabsTimerToSaveDatabase.Start();
            return res;
        }
        private async void CloseTabsTimerToSaveDatabase_Tick(object sender, object e)
        {
            if (sender is DispatcherTimer timer)
            {
                timer.Stop();
                await SaveAllTabChanges();
            }
        }

        /// <summary>
        /// Save the file to the temp-buffer
        /// </summary>
        /// <param name="TabPage"></param>
        /// <returns>wheater the save succed</returns>
        public async Task<bool> SaveTempFile(muxc.TabViewItem TabPage, StorageFolder folder)
        {
            try
            {
                if (TabPage.Content is TextControlBox textbox)
                {
                    StorageFile file = await folder.CreateFileAsync(GetTextBoxFromTabPage(TabPage).Name, CreationCollisionOption.OpenIfExists);
                    if (file != null)
                    {
                        System.IO.File.WriteAllText(file.Path, textbox.GetText());
                        textbox.TempFile = file.Name;
                        if (textbox.TabSaveMode == TabSaveMode.SaveAsTemp)
                            textbox.Storagefile = file;
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                await new InfoBox("Error while saving temporary file to database\n" + e.Message, "Error").ShowAsync();
            }
            return false;
        }

        /// <summary>
        /// The process to open a file
        /// When the TabPage parameter is null, it automatically creates a new tab
        /// When the Filetoken parameter is null it creates a new token
        /// </summary>
        /// <param name="TabPage">The tabpage to display the file, when null a new tab will be created</param>
        /// <param name="file">The file to read from</param>
        /// <param name="tabsavemode">The TabSaveMode</param>
        /// <param name="Locked">Whether the file is locked</param>
        /// <param name="SaveDatabase">Save database after opening or not</param>
        /// <param name="FileToken">The token of the file</param>
        /// <param name="IsModified">Whether the file is modified or not</param>
        /// <param name="CreateNewToken">Whether create a new token for the file or use the old one, when the token is emty it creates a new one</param>
        /// <param name="CheckForFileAlreadyOpened">Check whether the file is already opened or not</param>
        /// <param name="UpdateUI">Update the UI after opening</param>
        /// <returns>The status of the file and the TabPage, which is created in this methode</returns>
        public async Task<(bool Succed, muxc.TabViewItem TabPage)> DoOpenFile(
            muxc.TabViewItem TabPage, StorageFile file, TabSaveMode tabsavemode, bool SaveDatabase = true,
            bool Locked = false, string FileToken = "", bool IsModified = false, bool CreateNewToken = true,
            bool CheckForFileAlreadyOpened = true)
        {
            async Task<(bool, muxc.TabViewItem)> open()
            {
                var (Text, encoding, Succed) = await ReadTextFromFileAsync(file);
                if (Succed)
                {
                    if (FileToken.Length == 0 || CreateNewToken)
                    {
                        FileToken = StorageApplicationPermissions.FutureAccessList.Add(file);
                    }

                    if (TabPage == null)
                    {
                        TabPage = NewAllParametersTab(new TabDataForDatabase
                        {
                            TabName = NewTabName(),
                            TabHeader = file.Name,
                            TabToken = FileToken,
                            TabModified = false,
                            TabPath = file.Path,
                            DataBaseName = file.Name,
                            TabTemp = string.Empty,
                            TabReadOnly = false,
                            ZoomFactor = 100,
                            TabSaveMode = tabsavemode,
                            TabSelLenght = 0,
                            TabSelStart = 0,
                            TabEncoding = 0,
                            WordWrap = TextWrapping.NoWrap,
                        }, true, file);
                    }
                    var textbox = GetTextBoxFromTabPage(TabPage);
                    Debug.WriteLine("TabModified:" + textbox.IsModified);
                    if (textbox != null)
                    {
                        await Task.Run(() => textbox.SetText(Text));
                        textbox.Encoding = encoding;
                        textbox.TabSaveMode = tabsavemode;
                        textbox.FilePath = file.Path;
                        textbox.DataBaseName = file.Name;
                        textbox.FileToken = FileToken;
                        textbox.IsReadOnly = Locked;
                        textbox.Storagefile = file;
                        textbox.IsModified = IsModified;
                        if (SaveDatabase)
                            await SaveAllTabChanges();
                        return (true, TabPage);
                    }
                }
                return (false, null);
            }

            if (TabPage == null && CheckForFileAlreadyOpened)
            {
                muxc.TabViewItem tab = FileAlreadyOpened(file);
                if (tab != null)
                {
                    TextTabControl.SelectedItem = tab;
                    return (true, tab);
                }
            }
            return await open();
        }

        /// <summary>
        /// Open a file and create a new tab
        /// </summary>
        /// <param name="file">the Storagefile to open</param>
        public async Task<bool> OpenStorageFile(StorageFile file, TabSaveMode tabsavemode = TabSaveMode.SaveAsFile)
        {
            try
            {
                if (file != null)
                {
                    var (Succed, TabPage) = await DoOpenFile(TabPage: null, file: file, tabsavemode: tabsavemode, SaveDatabase: false, false, "", false, true, true);
                    if (Succed)
                    {
                        if (!TextTabControl.TabItems.Contains(TabPage))
                        {
                            TextTabControl.TabItems.Add(TabPage);
                            TextTabControl.SelectedItem = TabPage;
                        }
                        tabpagehelper.SetTabStorageFile(TabPage, file);
                        tabpagehelper.SetTabModified(TabPage, false);
                    }
                    return Succed;
                }
            }
            catch (Exception e)
            {
                ShowInfobar(ErrorDialogs.OpenErrorDialog(e.Message));
            }
            return false;
        }

        /// <summary>
        /// Open a file with open file picker
        /// </summary>
        /// <returns>Tab with the content of the opened file</returns>
        public async Task<List<muxc.TabViewItem>> OpenFile()
        {
            try
            {
                var picker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };

                //Add the fileextensions to the OpenFilePicker
                FileExtensions fileextentions = new FileExtensions();
                
                for(int i = 0; i<fileextentions.FileExtentionList.Count; i++)
                {
                    for(int j = 0; j<fileextentions.FileExtentionList[i].Extension.Count; j++)
                    {
                        picker.FileTypeFilter.Add(fileextentions.FileExtentionList[i].Extension[j]);               
                    }
                }
               
                List<muxc.TabViewItem> tabs = new List<muxc.TabViewItem>();
                var files = await picker.PickMultipleFilesAsync();
                for (int i = 0; i < files.Count; i++)
                {
                    if (files[i] != null)
                    {
                        //Open the file with the DoOpenFile function
                        var (Succed, TabPage) = await DoOpenFile(null, files[i], TabSaveMode.SaveAsFile, true);
                        if(Succed)
                            tabs.Add(TabPage);
                    }
                }
                return tabs;
            }
            catch
            {
                ShowInfobar(ErrorDialogs.OpenErrorDialog());
                return null;
            }
        }

        /// <summary>
        /// Chooses whether open a temporary saved file or a file from a path
        /// </summary>
        /// <param name="TabPage">The tabpage to set the text to</param>
        /// <param name="tabsavemode">The tabsavemode to choose between the filetypes</param>
        /// <param name="Token"></param>
        /// <returns>Whether the methode succed or failed</returns>
        public async Task<bool> LoadFilesFromDatabase(muxc.TabViewItem TabPage, StorageFolder folder = null)
        {
            try
            {
                if (folder == null)
                {
                    folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(DefaultValues.Database_FolderName, CreationCollisionOption.OpenIfExists);
                }

                var file = await folder.GetFileAsync(tabpagehelper.GetTabTempfile(TabPage));
                if (file == null)
                    return false;

                var (Text, encoding, Succed) = await ReadTextFromFileAsync(file);
                if (Succed)
                {
                    if (GetTextBoxFromTabPage(TabPage) is TextControlBox textbox)
                    {
                        await Task.Run(() => textbox.SetText(Text));
                        return true;
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                string FilePath = string.Empty;
                if (GetTextBoxFromTabPage(TabPage) is TextControlBox tb)
                {
                    FilePath = tb.FilePath;
                }

                ShowInfobar(ErrorDialogs.FileNotFoundExceptionErrorDialog(e, FilePath));

                TextTabControl.TabItems.Remove(TabPage);
            }
            catch (UnauthorizedAccessException e)
            {
                string FilePath = string.Empty;
                if (GetTextBoxFromTabPage(TabPage) is TextControlBox tb)
                {
                    FilePath = tb.FilePath;
                }

                ShowInfobar(ErrorDialogs.FileNoAccessExceptionErrorDialog(e, FilePath));
            }
            catch (Exception e)
            {
                ShowInfobar(e.Message, "Error", muxc.InfoBarSeverity.Error);
            }
            return false;
        }

        /// <summary>
        /// Open a file from backup-folder
        /// </summary>
        /// <param name="TabPage">Tabpage to set the text to</param>
        /// <param name="folder">the folder to read from</param>
        /// <returns></returns>
        public async Task<bool> OpenFileFromBackup(muxc.TabViewItem TabPage, StorageFolder folder = null)
        {
            try
            {
                if (folder == null)
                {
                    folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(DefaultValues.Backup_FolderName, CreationCollisionOption.OpenIfExists);
                }

                StorageFile file = await folder.GetFileAsync(tabpagehelper.GetTabName(TabPage) + ".txt");

                var (Text, encoding, Succed) = await ReadTextFromFileAsync(file);
                if (Succed)
                {
                    tabpagehelper.SetTabText(TabPage, Text);
                    tabpagehelper.SetTabHeader(TabPage, tabpagehelper.GetTabHeader(TabPage));
                    return true;
                }
                return false;
            }
            catch (FileNotFoundException e)
            {
                string FilePath = string.Empty;
                if (GetTextBoxFromTabPage(TabPage) is TextControlBox tb)
                {
                    FilePath = tb.FilePath;
                }

                ShowInfobar(ErrorDialogs.FileNotFoundExceptionErrorDialog(e, FilePath));
                return false;
            }
            catch (UnauthorizedAccessException e)
            {
                string FilePath = string.Empty;
                if (GetTextBoxFromTabPage(TabPage) is TextControlBox tb)
                {
                    FilePath = tb.FilePath;
                }

                ShowInfobar(ErrorDialogs.FileNoAccessExceptionErrorDialog(e, FilePath));
                return false;
            }
            catch (Exception e)
            {
                ShowInfobar(e.Message, "Error", muxc.InfoBarSeverity.Error);
                return false;
            }
        }

        public async Task<bool> OpenFileFromCommandLine(CommandLineLaunchNavigationParameter commandLine)
        {
            try
            {
                if (commandLine.Arguments.Length < 1)
                    return false;

                string path = StringBuilder.GetAbsolutePath(commandLine.Arguments, commandLine.CurrentDirectoryPath);
                if (path == null || path.Length < 1)
                    return false;
                
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                if (file != null)
                {
                    return await OpenStorageFile(file, TabSaveMode.SaveAsFile);
                }
            }
            catch (FileNotFoundException)
            {
                ShowInfobar(InfoBarMessages.FileNotFound, InfoBarMessages.FileNotFoundTitle, muxc.InfoBarSeverity.Error);
            }
            catch (UnauthorizedAccessException)
            {
                ShowInfobar(InfoBarMessages.FileNoAccess, InfoBarMessages.FileNoAccessTitle, muxc.InfoBarSeverity.Error);
            }
            catch (ArgumentException)
            {
                ShowInfobar(InfoBarMessages.FileInvalidPath, InfoBarMessages.FileInvalidPathTitle, muxc.InfoBarSeverity.Error);
            }
            return false;
        }

        public async Task<bool> OpenFilesFromEvent(FileActivatedEventArgs e)
        {
            bool res = true;
            for (int i = 0; i < e.Files.Count; i++)
            {
                if (e.Files[i] is StorageFile file)
                {
                    if (await OpenStorageFile(file) == false)
                    {
                        res = false;
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Check it file is already opened
        /// </summary>
        /// <param name="file">the file which is compared to the other tabs</param>
        /// <param name="OtherTabList">Another tablist when using a custom one</param>
        /// <param name="CheckTextboxContent">Compare also the text</param>
        /// <param name="TextBoxContent">The text</param>
        /// <returns>null when the file is not already opened; the tab in which the file is already opened</returns>
        public muxc.TabViewItem FileAlreadyOpened(StorageFile file, List<muxc.TabViewItem> OtherTabList = null, bool CheckTextboxContent = false, string TextBoxContent = "")
        {
            muxc.TabViewItem DoCheck(muxc.TabViewItem Tab)
            {
                if (file.Path == tabpagehelper.GetTabFilepath(Tab))
                {
                    if (CheckTextboxContent)
                    {
                        if (GetTextBoxFromTabPage(Tab).GetText() == TextBoxContent)
                        {
                            return Tab;
                        }
                        return null;
                    }

                    return Tab;
                }
                return null;
            }

            if (OtherTabList == null)
            {
                for (int i = 0; i < TextTabControl.TabItems.Count; i++)
                {
                    if (TextTabControl.TabItems[i] is muxc.TabViewItem Tab)
                    {
                        if (DoCheck(Tab) != null)
                        {
                            return Tab;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < OtherTabList.Count; i++)
                {
                    if (OtherTabList[i] is muxc.TabViewItem Tab)
                    {
                        if (DoCheck(Tab) != null)
                        {
                            return Tab;
                        }
                    }
                }
            }
            return null;
        }

        public async Task<bool> OpenFileWithEncoding(muxc.TabViewItem tab, Encoding enc)
        {
            try
            {
                if (tabpagehelper.GetIsModified(tab))
                {
                    var dlgres = await SaveDialogs.AskSaveDialog(tab);
                    if (dlgres == ContentDialogResult.Primary)
                    {
                        var saveres = await savefilehelper.Save(tab);
                        if (!saveres)
                            return false;
                    }
                    else if (dlgres == ContentDialogResult.None)
                        return false;
                }

                if (tab == null)
                    return false;

                var tb = tabpagehelper.GetTextBoxFromTabPage(tab);
                if (tb == null)
                    return false;

                var res = await ReadTextFromFileAsync(tb.Storagefile, enc);
                if (res.Succed)
                {
                    tb.Encoding = enc;
                    await tb.SetText(res.Text);
                    tabpagehelper.SetTabModified(tab, false);
                    return true;
                }
            }
            catch (Exception ex)
            {
                ShowInfobar(ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Rename the selected file
        /// </summary>
        /// <param name="name">The new name</param>
        /// <returns>wheter the rename function succed or not</returns>
        public async Task<bool> RenameFile(string name)
        {
            try
            {
                muxc.TabViewItem Tabpage = GetSelectedTabPage();
                if (Tabpage != null)
                {
                    if (name.Length < 1 && !StringBuilder.IsValidFilename(name))
                    {
                        var infoBox = new InfoBox("The filename is invalid!");
                        await infoBox.ShowAsync();
                        return false;
                    }
                    else
                    {
                        if (GetTextBoxFromTabPage(Tabpage) is TextControlBox textbox)
                        {
                            if (tabpagehelper.GetTabSaveMode(Tabpage) != TabSaveMode.SaveAsTemp)
                            {
                                await textbox.Storagefile.RenameAsync(name, NameCollisionOption.FailIfExists);
                                tabpagehelper.SetTabHeader(Tabpage, name);
                                textbox.FilePath = textbox.Storagefile.Path;
                                return true;
                            }
                            else
                            {
                                tabpagehelper.SetTabHeader(Tabpage, name);
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                await new InfoBox(e.Message, "Error").ShowAsync();
            }
            return false;
        }

        /// <summary>
        /// Read text from Storagefile
        /// </summary>
        /// <param name="file">The file to read from</param>
        /// <returns>the text read from the file and the encoding of the text</returns>
        public async Task<(string Text, Encoding encoding, bool Succed)> ReadTextFromFileAsync(StorageFile file, Encoding encoding = null)
        {
            try
            {
                if (file == null)
                    return ("", Encoding.Default, false);

                using (var stream = (await file.OpenReadAsync()).AsStreamForRead())
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    string text = "";
                    if(encoding == null)
                    {
                        encoding = Encodings.DetectTextEncoding(buffer, out text);
                    }
                    text = encoding.GetString(buffer, 0, buffer.Length);
                    return (text, encoding, true);
                }
            }
            catch (Exception e)
            {
                ShowInfobar(e.Message + "\nFile: " + tabpagehelper.GetPathFromStorageFile(file));
                return ("", Encoding.Default, false);
            }
        }
        public async Task<(string Text, Encoding encoding, bool Succed)> ReadTextFromPathAsync(string Path)
        {
            try
            {
                Encoding encoding = Encoding.Default;
                using (var stream = new StreamReader(Path))
                {
                    byte[] buffer = new byte[stream.BaseStream.Length];
                    stream.BaseStream.Read(buffer, 0, buffer.Length);
                    string text = "";
                    text = encoding.GetString(buffer, 0, buffer.Length);
                    return (text, encoding, true);
                }
            }
            catch (Exception e)
            {
                await new InfoBox(e.Message + "\nFile: " + Path).ShowAsync();
                return ("", Encoding.Default, false);
            }
        }
    }
}
