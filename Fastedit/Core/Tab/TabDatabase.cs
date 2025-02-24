using Fastedit.Core.Settings;
using Fastedit.Models;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace Fastedit.Core.Tab
{
    public class TabDatabase
    {
        string DatabaseName = "database.db";

        public void SaveData(IList<object> TabItems, int SelectedIndex)
        {
            StringBuilder databaseBuilder = new StringBuilder();
            for (int i = 0; i < TabItems.Count; i++)
            {
                if (TabItems[i] is TabPageItem tab)
                {
                    if (tab.IsLoaded)
                    {
                        tab.DatabaseItem.CharacterPos = tab.textbox.CursorPosition.CharacterPosition;
                        tab.DatabaseItem.LinePos = tab.textbox.CursorPosition.LineNumber;
                    }

                    tab.DatabaseItem.SelectedIndex = SelectedIndex;
                    databaseBuilder.AppendLine(JsonConvert.SerializeObject(tab.DatabaseItem));
                }
            }

            //save all windows:
            foreach (var window in TabWindowHelper.OpenWindows)
            {
                window.Value.DatabaseItem.SelectedIndex = SelectedIndex;
                databaseBuilder.AppendLine(JsonConvert.SerializeObject(window.Value.DatabaseItem));
            }

            string path = Path.Combine(DefaultValues.DatabasePath, DatabaseName);
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(DefaultValues.DatabasePath);
            }

            File.WriteAllText(path, databaseBuilder.ToString());
        }
        public IEnumerable<TabPageItem> LoadData(TabView tabView)
        {
            string path = Path.Combine(DefaultValues.DatabasePath, DatabaseName);
            string databaseContent = "";

            if (File.Exists(path))
                databaseContent = File.ReadAllText(path);

            var lines = databaseContent.Split("\n", StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                var dbItem = JsonConvert.DeserializeObject<TabItemDatabaseItem>(lines[i]);
                yield return new TabPageItem(tabView, dbItem);
            }
        }

        public static void SaveTempFile(TabPageItem tab)
        {
            File.WriteAllLines(Path.Combine(DefaultValues.DatabasePath, tab.DatabaseItem.Identifier), tab.textbox.Lines);
        }
        public static void DeleteTempFile(TabPageItem tab)
        {
            string path = Path.Combine(DefaultValues.DatabasePath, tab.DatabaseItem.Identifier);
            if (File.Exists(path))
                File.Delete(path);
        }
        public static string[] ReadTempFile(TabPageItem tab)
        {
            string path = Path.Combine(DefaultValues.DatabasePath, tab.DatabaseItem.Identifier);
            if (File.Exists(path))
            {
                return File.ReadAllLines(path);
            }
            return [];
        }
    }
}
