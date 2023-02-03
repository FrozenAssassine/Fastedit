using Fastedit.Settings;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fastedit.Tab
{
    public class TabDatabase
    {
        string DatabaseName = "database.db";

        //Migrate the old database to the new one:
        public static TabPageItem[] CheckOlddatabase(TabView tabView)
        {
            string path = Path.Combine(DefaultValues.DatabasePath, "Tabs.tdb");
            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                TabPageItem[] newItems = new TabPageItem[lines.Length];
                for (int i = 0; i < lines.Length; i++)
                {
                    try
                    {
                        var oldItem = JsonConvert.DeserializeObject<OldDBItem>(lines[i]);
                        var resultItem = new TabItemDatabaseItem
                        {
                            FileToken = oldItem.TabToken,
                            FileName = oldItem.TabHeader,
                            IsModified = oldItem.TabModified,
                            FilePath = oldItem.TabPath,
                            Identifier = oldItem.TabName,
                            ZoomFactor = (int)oldItem.ZoomFactor,
                            SelectedIndex = oldItem.CurrentSelectedTabIndex,
                        };

                        newItems[i] = new TabPageItem(tabView)
                        {
                            DatabaseItem = resultItem,
                        };
                    }
                    catch
                    {
                        Debug.WriteLine("Database migration -> item parse exception");
                    }
                }
                File.Delete(path);
                return newItems;
            }

            return null;
        }

        public void SaveData(IList<object> TabItems, int SelectedIndex)
        {
            StringBuilder databaseBuilder = new StringBuilder();
            for (int i = 0; i < TabItems.Count; i++)
            {
                if (TabItems[i] is TabPageItem tab)
                {
                    tab.DatabaseItem.SelectedIndex = SelectedIndex;
                    databaseBuilder.AppendLine(JsonConvert.SerializeObject(tab.DatabaseItem));
                }
            }

            string path = Path.Combine(DefaultValues.DatabasePath, DatabaseName);
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(DefaultValues.DatabasePath);
            }

            Debug.WriteLine("Saving database...");
            File.WriteAllText(path, databaseBuilder.ToString());
            Debug.WriteLine("Database saved");
        }
        public IEnumerable<TabPageItem> LoadData(TabView tabView)
        {
            string databaseContent = "";
            string path = Path.Combine(DefaultValues.DatabasePath, DatabaseName);

            if (File.Exists(path))
                databaseContent = File.ReadAllText(path);

            var lines = databaseContent.Split("\n", StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                yield return new TabPageItem(tabView)
                {
                    DatabaseItem = JsonConvert.DeserializeObject<TabItemDatabaseItem>(lines[i]),
                };
            }
        }

        public static async Task SaveTempFile(StorageFolder folder, TabPageItem tab)
        {
            var file = await folder.CreateFileAsync(tab.DatabaseItem.Identifier, CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(file, tab.textbox.GetText());
        }
        public static void DeleteTempFile(TabPageItem tab)
        {
            string path = Path.Combine(DefaultValues.DatabasePath, tab.DatabaseItem.Identifier);
            if (File.Exists(path))
                File.Delete(path);
        }
        public static async Task<string> ReadTempFile(TabPageItem tab)
        {
            string path = Path.Combine(DefaultValues.DatabasePath, tab.DatabaseItem.Identifier);
            if (File.Exists(path))
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                return await FileIO.ReadTextAsync(file);
            }
            return "";
        }
    }
    public class TabItemDatabaseItem
    {
        public TabItemDatabaseItem() { }
        public TabItemDatabaseItem(TabItemDatabaseItem item)
        {
            this.IsModified = item.IsModified;
            this.ZoomFactor = item.ZoomFactor;
            this.SelectedIndex = item.SelectedIndex;
            this.Identifier = item.Identifier;
            this.CodeLanguage = item.CodeLanguage;
            this.FileName = item.FileName;
            this.FilePath = item.FilePath;
            this.FileToken = item.FileToken;
            this.SaveMode = item.SaveMode;
            this.Encoding = item.Encoding;
        }
        public bool IsModified { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Identifier { get; set; }
        public int SelectedIndex { get; set; }
        public int SaveMode { get; set; }
        public string FileToken { get; set; }
        public int ZoomFactor { get; set; }
        public string CodeLanguage { get; set; }
        public int Encoding { get; set; }

        public bool HasOwnWindow { get; set; }
    }
    public class OldDBItem
    {
        public string TabName { get; set; }
        public bool TabModified { get; set; }
        public string TabHeader { get; set; }
        public string TabToken { get; set; }
        public string TabPath { get; set; }
        public double ZoomFactor { get; set; }
        public int CurrentSelectedTabIndex { get; set; }
    }
}
