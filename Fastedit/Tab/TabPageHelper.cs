using Fastedit.Dialogs;
using Fastedit.Helper;
using Fastedit.Settings;
using Fastedit.Storage;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Fastedit.Tab
{
    public static class TabPageHelper
    {
        public static MainPage mainPage = null;
        public static Thickness TabMargin = new Thickness(0);

        public static TextControlBox.TextControlBox CreateTextBox()
        {
            var textbox = new TextControlBox.TextControlBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = TabMargin
            };
            textbox.TextChanged += Textbox_TextChanged;
            textbox.ZoomChanged += Textbox_ZoomChanged;
            textbox.SelectionChanged += Textbox_SelectionChanged;
            return textbox;
        }

        //textbox events:
        private static void Textbox_SelectionChanged(TextControlBox.TextControlBox sender, TextControlBox.Text.SelectionChangedEventHandler args)
        {
            mainPage.UpdateStatubar();
        }
        private static void Textbox_ZoomChanged(TextControlBox.TextControlBox sender, int zoomFactor)
        {
            //apply the zoomfactor to the databaseitem
            if (sender.Tag is TabPageItem tab)
            {
                tab.DatabaseItem.ZoomFactor = zoomFactor;
                mainPage.UpdateStatubar();
            }
        }
        private static void Textbox_TextChanged(TextControlBox.TextControlBox sender)
        {
            if (sender.Tag is TabPageItem tab)
            {
                if (!tab.DatabaseItem.IsModified)
                {
                    tab.DatabaseItem.IsModified = true;
                    tab.UpdateHeader();
                }
            }
        }

        /// <summary>
        /// Check and load the tab if it has not loaded the text from the file into the textbox
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="progressWindow"></param>
        /// <returns></returns>
        public static async Task LoadUnloadedTab(TabPageItem tab, ProgressWindowItem progressWindow = null)
        {
            if (!tab.DataIsLoaded && tab != null)
            {
                progressWindow?.ShowProgress();
                progressWindow?.SetText("Loading file " + tab.DatabaseItem.FileName + "...");
                tab.textbox.LoadText(await TabDatabase.ReadTempFile(tab));
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

            SettingsUpdater.UpdateTab(tab);

            if (Select)
                tabView.SelectedItem = tab;
            return tab;
        }
        public static async Task LoadTabDatabase(TabView tabView, TabDatabase tabdatabase)
        {
            IEnumerable<TabPageItem> tabData;
            var res = TabDatabase.CheckOlddatabase(tabView);
            if (res != null)
                tabData = res;
            else
            {
                tabData = tabdatabase.LoadData(tabView);
                if (tabData.Count() == 0)
                {
                    AddNewTab(tabView, true);
                    return;
                }
            }

            int currentCount = 0;
            using (var sequence = tabData.GetEnumerator())
            {
                while (sequence.MoveNext())
                {
                    var tab = sequence.Current;
                    //Either load all tabs or only the selected
                    if (tab.DatabaseItem.HasOwnWindow ||
                        currentCount == tab.DatabaseItem.SelectedIndex ||
                        !DefaultValues.FastLoadTabs)
                    {
                        tab.textbox.LoadText(await TabDatabase.ReadTempFile(tab));
                        tab.Encoding = EncodingHelper.GetEncodingByIndex(tab.DatabaseItem.Encoding);
                        tab.DataIsLoaded = true;
                    }

                    //Show tab in his own window or add it to the tabView:
                    if (tab.DatabaseItem.HasOwnWindow)
                        await TabWindowHelper.ShowInNewWindow(tabView, tab);
                    else
                        tabView.TabItems.Add(tab);

                    currentCount++;
                }

                tabView.SelectedIndex = -1;
                await Task.Delay(20); //textbox sometimes does not load without the delay
                int selectingIndex = tabData.ElementAt(0).DatabaseItem.SelectedIndex;
                tabView.SelectedIndex = selectingIndex < 0 ? 0 : selectingIndex >= tabView.TabItems.Count ? tabView.TabItems.Count - 1 : selectingIndex;
            }
        }
        public static async Task SaveTabDatabase(TabDatabase tabDatabase, TabView tabView, ProgressWindowItem progressWindow = null, bool closeWindows = true)
        {
            progressWindow?.ShowProgress();

            var folder = await StorageFolder.GetFolderFromPathAsync(DefaultValues.DatabasePath);
            if (folder == null)
                return;

            //Close all windows to get the tabs back to the tabcontrol:
            //when saving the database without closing the app, only save the temp files
            if (closeWindows)
                await TabWindowHelper.CloseAllWindows();
            else //save the tempfiles for all the window instances:
            {
                foreach (var window in TabWindowHelper.AppWindows)
                {
                    await TabDatabase.SaveTempFile(folder, window.Value);
                }
            }

            //Create the file with the tab data:
            tabDatabase.SaveData(tabView.TabItems, tabView.SelectedIndex);

            //save the individual files
            for (int i = 0; i < tabView.TabItems.Count; i++)
            {
                if (tabView.TabItems[i] is TabPageItem tab)
                {
                    if (tab.DataIsLoaded)
                    {
                        progressWindow?.SetText("Saving database " + tab.DatabaseItem.FileName + "...");
                        await TabDatabase.SaveTempFile(folder, tab);
                    }
                }
            }

            progressWindow?.HideProgress();
        }
        /// <summary>
        /// Generate a unique tab header
        /// </summary>
        /// <param name="tabView"></param>
        /// <returns></returns>
        public static string GenerateUniqueHeader(TabView tabView)
        {
            int TabIndex = 0;
            string title = AppSettings.GetSettings(AppSettingsValues.Settings_NewTabTitle, DefaultValues.NewTabTitle);
            string extension = AppSettings.GetSettings(AppSettingsValues.Settings_NewTabExtension, DefaultValues.NewTabExtension);

            //check for any equal tab
            for (int i = 0; i < tabView.TabItems.Count; i++)
            {
                foreach (var window in TabWindowHelper.AppWindows)
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
                foreach (var window in TabWindowHelper.AppWindows)
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
                    await RemoveTab(tabView, tab);
                }
            }
            else if (SettingsTabPageHelper.IsSettingsPage(tabItem))
                SettingsTabPageHelper.CloseSettings(tabView);

            return false;
        }
        private static async Task<bool> RemoveTab(TabView tabView, TabPageItem tab)
        {
            //only add the file to the recylcbin when it has some content and was modified
            if (tab.DatabaseItem.IsModified && tab.textbox.CharacterCount > 0)
                if (!await RecycleBinDialog.MoveFileToRecycleBin(tab))
                    return false;

            tab.textbox.Unload();
            tabView.TabItems.Remove(tab);
            TabDatabase.DeleteTempFile(tab);
            return !tabView.TabItems.Contains(tab);
        }

        public static void TabsOrSpaces(TabView tabView, object tag)
        {
            if (tag == null)
                return;

            int number = ConvertHelper.ToInt(tag);
            for (int i = 0; i < tabView.TabItems.Count; i++)
            {
                if (tabView.TabItems[i] is TabPageItem tab)
                {
                    tab.textbox.UseSpacesInsteadTabs = number != -1;
                    if (number != -1)
                    {
                        tab.textbox.NumberOfSpacesForTab = number;
                    }
                }
            }

            AppSettings.SaveSettings(AppSettingsValues.Settings_SpacesPerTab, number);
            AppSettings.SaveSettings(AppSettingsValues.Settings_UseSpacesInsteadTabs, number != -1);
        }
        public static void UpdateSaveStatus(TabPageItem tab, bool IsModified, string FileToken = "", StorageFile file = null)
        {
            tab.DatabaseItem.IsModified = IsModified;
            tab.DatabaseItem.FileToken = FileToken;

            if (file != null)
            {
                tab.DatabaseItem.FilePath = file.Path;
                tab.SetHeader(file.Name);
                SelectCodeLanguageByFile(tab, file);
                return;
            }

            tab.UpdateHeader();
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

        public static async Task<bool> OpenFiles(TabView tabView, IReadOnlyList<IStorageItem> files)
        {
            if (files.Count == 0)
                return false;

            for (int i = 0; i < files.Count; i++)
            {
                //Only load the last tab, the others will be saved as temporary files and loaded when needed
                var tab = await OpenFileHelper.DoOpen(tabView, files[i] as StorageFile, i == files.Count - 1);
                if (tab == null)
                    return false;

                //select the last tab
                if (i == files.Count - 1)
                    tabView.SelectedItem = tab;
            }
            return true;
        }
        public static void SelectCodeLanguageByFile(TabPageItem tab, StorageFile file)
        {
            if (tab == null)
                return;

            string extension = Path.GetExtension(file.Path).ToLower();

            //search through the dictionary of codelanguages in the textbox
            foreach (var item in TextControlBox.TextControlBox.CodeLanguages)
            {
                for (int i = 0; i < item.Value.Filter.Length; i++)
                {
                    if (item.Value.Filter[i].Equals(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        tab.CodeLanguage = item.Value;
                        return;
                    }
                }
            }
        }
        public static void SelectNextTab(TabView tabView)
        {
            if (tabView.SelectedIndex < tabView.TabItems.Count - 1)
            {
                tabView.SelectedIndex++;
            }
        }
        public static void SelectPreviousTab(TabView tabView)
        {
            if (tabView.SelectedIndex > 0)
            {
                tabView.SelectedIndex--;
            }
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
    }
}
