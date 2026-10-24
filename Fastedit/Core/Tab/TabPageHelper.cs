using Fastedit.Dialogs;
using Fastedit.Helper;
using Fastedit.Storage;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.UI.Xaml;
using TextControlBoxNS;
using System.Diagnostics;
using Fastedit.Core.Settings;
using Fastedit.Core.Storage;
using Fastedit.Views;

namespace Fastedit.Core.Tab;

public static class TabPageHelper
{
    public static MainPage mainPage = null;
    public static Thickness TabMargin = new Thickness(0);

    public static TextControlBox CreateTextBox()
    {
        var textbox = new TextControlBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = TabMargin,
            ControlW_SelectWord = false,
            ShowWhitespaceCharacters = true,
            DoAutoPairing = true,
            AutoPairOnlyOnSelection = true,
        };
        textbox.TextChanged += Textbox_TextChanged;
        textbox.ZoomChanged += Textbox_ZoomChanged;
        textbox.SelectionChanged += Textbox_SelectionChanged;
        textbox.TabsSpacesChanged += Textbox_TabsSpacesChanged;
        textbox.LinkClicked += Textbox_LinkClicked;
        textbox.LineEndingChanged += Textbox_LineEndingChanged;
        return textbox;
    }

    private static void Textbox_LineEndingChanged(TextControlBox sender, LineEnding lineEnding)
    {
        mainPage.TextStatusBar.UpdateLineEndings();
        mainPage.SelectLineEndingMenubarItem();
    }

    private static async void Textbox_LinkClicked(TextControlBox sender, string url)
    {
        await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
    }

    private static void Textbox_TabsSpacesChanged(TextControlBox sender, bool spacesInsteadTabs, int spaces)
    {
        mainPage.TextStatusBar.UpdateTabsSpaces();
        mainPage.SelectTabSpacesMenubarItem();
    }

    //textbox events:
    private static void Textbox_SelectionChanged(TextControlBox sender, TextControlBoxNS.SelectionChangedEventHandler args)
    {
        mainPage.TextStatusBar.UpdateSelectionChanged();
    }
    private static void Textbox_ZoomChanged(TextControlBox sender, int zoomFactor)
    {
        //apply the zoomfactor to the databaseitem
        if (sender.Tag is TabPageItem tab)
        {
            tab.DatabaseItem.ZoomFactor = zoomFactor;
            mainPage.TextStatusBar.UpdateZoom();
        }
    }
    private static void Textbox_TextChanged(TextControlBox sender)
    {
        if (sender.Tag is TabPageItem tab)
        {
            tab.NeedsBackup = true;
            if (!tab.DatabaseItem.IsModified)
            {
                tab.DatabaseItem.IsModified = true;
                tab.UpdateHeader();
            }
            mainPage.TextStatusBar.UpdateText();
        }
    }

    /// <summary>
    /// Check and load the tab if it has not loaded the text from the file into the textbox
    /// </summary>
    /// <param name="tab"></param>
    /// <param name="progressWindow"></param>
    /// <returns></returns>
    public static void LoadUnloadedTab(TabPageItem tab, ProgressWindowItem progressWindow = null)
    {
        if (!tab.DataIsLoaded && tab != null)
        {
            progressWindow?.ShowProgress();
            progressWindow?.SetText("Loading file " + tab.DatabaseItem.FileName + "...");
            
            tab.textbox.LoadLines(TabDatabase.ReadTempFile(tab), true, tab.LineEnding);
            tab.SetTabsSpaces(tab.DatabaseItem.TabsSpaces);

            tab.Encoding = EncodingHelper.GetEncodingByIndex(tab.DatabaseItem.Encoding);
            progressWindow?.HideProgress();
            tab.DataIsLoaded = true;
        }
    }
    public static TabPageItem AddNewTab(TabView tabView, bool Select)
    {
        var tab = new TabPageItem(tabView);
        tab.SetHeader(GenerateUniqueHeader(tabView));
        tab.DatabaseItem.Identifier = GenerateUniqueIdentifier(tabView);
        tab.DataIsLoaded = true;
        tabView.TabItems.Add(tab);
        tab.textbox.Loaded += (sender) =>
        {
            tab.textbox.Focus(FocusState.Programmatic);
            SettingsUpdater.UpdateTab(tab);
        };

        if (Select)
            tabView.SelectedItem = tab;
        return tab;
    }


    public static async Task LoadTabDatabase(TabView tabView, TabDatabase tabdatabase)
    {
        var tabData = tabdatabase.LoadData(tabView).ToList();
        var ownWindowTabs = new List<TabPageItem>();
        int currentCount = 0;
        foreach (var tab in tabData)
        {
            //Either load all tabs or only the selected
            if (tab.DatabaseItem.HasOwnWindow ||
                currentCount == tab.DatabaseItem.SelectedIndex ||
                !DefaultValues.FastLoadTabs)
            {
                //when loading back from the database file, we apply everything like it was on app closing
                tab.textbox.LoadLines(TabDatabase.ReadTempFile(tab), true, tab.LineEnding);
                tab.SetTabsSpaces(tab.DatabaseItem.TabsSpaces);

                tab.Encoding = EncodingHelper.GetEncodingByIndex(tab.DatabaseItem.Encoding);
                tab.DataIsLoaded = true;
            }

            //focus the last selected tab
            tab.textbox.Loaded += (sender) =>
            {
                tab.textbox.Focus(FocusState.Programmatic);

                if (tab.DataIsLoaded)
                    tab.textbox.SetCursorPosition(tab.DatabaseItem.LinePos, tab.DatabaseItem.CharacterPos);
            };

            //tabs with their own window are first loaded in the main window and moved out afterwards,
            //like dragging a tab out. A textbox that is first loaded in a separate window crashes the app when that window closes
            tabView.TabItems.Add(tab);
            if (tab.DatabaseItem.HasOwnWindow)
                ownWindowTabs.Add(tab);

            if (!string.IsNullOrEmpty(tab.DatabaseItem?.FilePath) && File.Exists(tab.DatabaseItem.FilePath))
            {
                FileChangeManager.StartWatching(tab);
            }

            currentCount++;
        }

        if (currentCount == 0)
            return;

        tabView.SelectedIndex = -1;
        await Task.Delay(20); //textbox sometimes does not load without the delay

        foreach (var tab in ownWindowTabs)
        {
            tabView.SelectedItem = tab;
            await WaitForTextboxLoaded(tab);
            TabWindowHelper.ShowInNewWindow(tabView, tab);
        }

        if (tabView.TabItems.Count == 0)
            return;

        int selectingIndex = tabData[0].DatabaseItem.SelectedIndex;
        tabView.SelectedIndex = Math.Clamp(selectingIndex, 0, tabView.TabItems.Count - 1);
    }

    private static async Task WaitForTextboxLoaded(TabPageItem tab)
    {
        if (tab.textbox.IsLoaded)
            return;

        var loaded = new TaskCompletionSource();
        void OnLoaded(object sender, RoutedEventArgs e) => loaded.TrySetResult();
        ((FrameworkElement)tab.textbox).Loaded += OnLoaded;
        await Task.WhenAny(loaded.Task, Task.Delay(1000));
        ((FrameworkElement)tab.textbox).Loaded -= OnLoaded;
    }

    public static bool SaveTabDatabase(TabDatabase tabDatabase, TabView tabView, ProgressWindowItem progressWindow = null, bool closeWindows = true)
    {
        progressWindow?.ShowProgress();

        bool anyFailed = false;

        //Close all windows to get the tabs back to the tabcontrol:
        //when saving the database without closing the app, only save the temp files
        if (closeWindows)
            TabWindowHelper.CloseAllWindows();
        else //save the tempfiles for all the window instances:
        {
            foreach (var window in TabWindowHelper.OpenWindows)
            {
                if (!TabDatabase.SaveTempFile(window.Value))
                    anyFailed = true;
            }
        }

        //Create the file with the tab data:
        bool saveDBFileRes = tabDatabase.SaveData(tabView.TabItems, tabView.SelectedIndex);

        //save the individual files
        for (int i = 0; i < tabView.TabItems.Count; i++)
        {
            if (tabView.TabItems[i] is TabPageItem tab)
            {
                if (!tab.DataIsLoaded)
                    continue;

                progressWindow?.SetText("Saving database " + tab.DatabaseItem.FileName + "...");
                if (!TabDatabase.SaveTempFile(tab))
                    anyFailed = true;
                else
                    tab.NeedsBackup = false;
            }
        }
        progressWindow?.HideProgress();
        lastBackupTabCount = tabView.TabItems.Count + TabWindowHelper.OpenWindows.Count;

        return saveDBFileRes && !anyFailed;
    }

    private static int lastBackupTabCount = -1;

    //auto save: only back up tabs whose text changed since the last backup
    public static void BackupChangedTabs(TabDatabase tabDatabase, TabView tabView)
    {
        var changedTabs = tabView.TabItems.OfType<TabPageItem>()
            .Concat(TabWindowHelper.OpenWindows.Values)
            .Where(x => x.NeedsBackup && x.DataIsLoaded)
            .ToList();

        int tabCount = tabView.TabItems.Count + TabWindowHelper.OpenWindows.Count;
        if (changedTabs.Count == 0 && tabCount == lastBackupTabCount)
            return;

        foreach (var tab in changedTabs)
        {
            if (TabDatabase.SaveTempFile(tab))
                tab.NeedsBackup = false;
        }

        if (tabDatabase.SaveData(tabView.TabItems, tabView.SelectedIndex))
            lastBackupTabCount = tabCount;
    }

    public static string GenerateUniqueHeader(TabView tabView)
    {
        int TabIndex = 0;
        string title = AppSettings.NewTabTitle;
        string extension = AppSettings.NewTabExtension;

        //check for any equal tab
        for (int i = 0; i < tabView.TabItems.Count; i++)
        {
            foreach (var window in TabWindowHelper.OpenWindows)
            {
                if (window.Value.HasHeader(title + TabIndex + extension))
                {
                    TabIndex++;
                }
            }

            if (tabView.TabItems[i] is TabPageItem tab)
            {
                if (tab.HasHeader(title + TabIndex + extension))
                {
                    TabIndex++;
                    i = -1; //go through the loop again
                }
            }
        }

        return title + TabIndex + extension;
    }
    /// <summary>
    /// Generate an unique id for every tabpage
    /// </summary>
    /// <param name="tabView"></param>
    /// <returns>The id generated</returns>
    public static string GenerateUniqueIdentifier(TabView tabView)
    {
        int TabIndex = 0;

        //check for any equal tab
        for (int i = 0; i < tabView.TabItems.Count; i++)
        {
            foreach (var window in TabWindowHelper.OpenWindows)
            {
                if (window.Value.DatabaseItem.Identifier.Equals("Tab" + TabIndex, StringComparison.Ordinal))
                {
                    TabIndex++;
                }
            }

            if (tabView.TabItems[i] is TabPageItem tab)
            {
                if (tab.DatabaseItem.Identifier.Equals("Tab" + TabIndex, StringComparison.Ordinal))
                {
                    TabIndex++;
                    i = -1; //go through the loop again
                }
            }
        }
        return "Tab" + TabIndex;
    }
    public static async Task<bool> CloseTab(TabView tabView, object tabItem)
    {
        if (tabItem is TabPageItem tab)
        {
            if (!tab.DatabaseItem.IsModified || await AskSaveDialog.Show(tab))
            {
                return RemoveTab(tabView, tab);
            }
        }
        else if (SettingsTabPageHelper.IsSettingsPage(tabItem))
        {
            SettingsTabPageHelper.CloseSettings(tabView);
            return true;
        }

        return false;
    }
    private static bool RemoveTab(TabView tabView, TabPageItem tab)
    {
        //only add the file to the recyclebin if it has text inside and was modified
        if (tab.DatabaseItem.IsModified && tab.textbox.CharacterCount() > 0)
            if (!RecycleBinManager.MoveFileToRecycleBin(tab))
                return false;

        if(mainPage.SearchControl.currentTab == tab)
            mainPage.SearchControl.Close();

        FileChangeManager.StopWatching(tab);

        tabView.TabItems.Remove(tab);
        TabDatabase.DeleteTempFile(tab);
        return !tabView.TabItems.Contains(tab);
    }

    public static void UpdateSaveStatus(TabPageItem tab, bool IsModified)
    {
        tab.DatabaseItem.IsModified = IsModified;
        tab.SetHeader(tab.DatabaseItem.FileName);
        SelectHighlightLanguageByPath(tab);
        tab.UpdateHeader();
        mainPage.TextStatusBar.UpdateFile();
    }

    public static void UpdateStatusbarForTab(TabPageItem tab)
    {
        if (tab == null)
            return;

        if (mainPage != null && (mainPage.currentlySelectedTabPage == tab || ReferenceEquals(mainPage.tabView?.SelectedItem, tab)))
        {
            if (mainPage.TextStatusBar != null)
            {
                mainPage.TextStatusBar.tabPage = tab;
                mainPage.TextStatusBar.UpdateAll();
            }
        }
        else
        {
            var windowEntry = TabWindowHelper.OpenWindows.FirstOrDefault(x => x.Value == tab);
            if (windowEntry.Key?.Content is TabWindowPage page)
            {
                if (page.Statusbar != null)
                {
                    page.Statusbar.tabPage = tab;
                    page.Statusbar.UpdateAll();
                }
            }
        }
    }
    public static async Task<bool> SaveFile(TabPageItem tab)
    {
        if (tab == null)
            return false;

        return await SaveFileHelper.Save(tab);
    }
    public static async Task<bool> SaveFileAs(TabPageItem tab)
    {
        if (tab == null)
            return false;

        return await SaveFileHelper.SaveFileAs(tab);
    }
    public static async Task<bool> OpenFile(TabView tabView)
    {
        return await OpenFileHelper.OpenFile(tabView);
    }

    public static async Task<bool> OpenAndShowFile(TabView tabView, string filePath, bool select = false)
    {
        return await OpenFileHelper.DoOpenAsync(tabView, filePath, true, select) != null;
    }

    public static async Task<bool> OpenFiles(TabView tabView, IReadOnlyList<IStorageItem> files)
    {
        if (files.Count == 0)
            return false;

        for (int i = 0; i < files.Count; i++)
        {
            if (files[i] == null)
                continue;

            //Only load the last tab, the others will be saved as temporary files and loaded when needed
            var tab = await OpenFileHelper.DoOpenAsync(tabView, (files[i] as StorageFile).Path, i == files.Count - 1);
            if (tab == null)
                return false;

            //select the last tab
            if (i == files.Count - 1)
                tabView.SelectedItem = tab;
        }
        return true;
    }
    public static void SelectHighlightLanguageByPath(TabPageItem tab)
    {
        if (tab == null)
            return;

        string extension = Path.GetExtension(tab.DatabaseItem.FilePath).ToLower();
        //search through the dictionary of codelanguages in the textbox
        foreach (var item in TextControlBox.SyntaxHighlightings)
        {
            for (int i = 0; i < item.Value?.Filter.Length; i++)
            {
                if (item.Value.Filter[i].Equals(extension, StringComparison.OrdinalIgnoreCase))
                {
                    tab.HighlightLanguage = item.Key;
                    return;
                }
            }
        }

        tab.HighlightLanguage = AppSettings.UseGenericHighlightForUnknownFiles ? SyntaxHighlightID.Generic : SyntaxHighlightID.None;
    }
    public static void SelectNextTab(TabView tabView, bool wrapAround = false)
    {
        if (tabView.SelectedIndex < tabView.TabItems.Count - 1)
        {
            tabView.SelectedIndex++;
        }
        else if (wrapAround && tabView.TabItems.Count > 0)
            tabView.SelectedIndex = 0;
    }
    public static void SelectPreviousTab(TabView tabView, bool wrapAround = false)
    {
        if (tabView.SelectedIndex > 0)
        {
            tabView.SelectedIndex--;
        }
        else if (wrapAround && tabView.TabItems.Count > 0)
            tabView.SelectedIndex = tabView.TabItems.Count - 1;
    }
    public static async Task SaveAll(TabView tabView)
    {
        foreach (var currentTab in tabView.TabItems)
        {
            if (currentTab is TabPageItem tab)
            {
                await SaveFile(tab);
            }
        }
    }
    public static async Task CloseAll(TabView tabView)
    {
        while (tabView.TabItems.Count > 0)
        {
            if (!await CloseTab(tabView, tabView.TabItems[tabView.TabItems.Count - 1]))
                return; //Cancelled by user
        }
    }
}
