using Fastedit.Controls.Textbox;
using Fastedit.Dialogs;
using Fastedit.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit.Core.Tab
{
    public class TabDataBase
    {
        public string DataBaseName = "Tabs.tdb";

        private async Task<string> ReadFromFile(string Path)
        {
            using (var reader = new StreamReader(Path))
            {
                var txt = await reader.ReadToEndAsync();
                reader.Close();
                reader.Dispose();
                return txt;
            }
        }
        private async Task<bool> WriteToFile(string Path, string text)
        {
            try
            {
                File.WriteAllText(Path, text);
                return true;
            }
            catch (Exception e)
            {
                await new InfoBox("Could not write to file\n" + Path).ShowAsync();
                Debug.WriteLine("Exception in TabDataBase --> WriteToFile:" + "\n" + e.Message);
                return false;
            }
        }

        //Create the data to save
        public string CreateTabDataBase(muxc.TabView TabView)
        {
            string outputtext = string.Empty;
            int CurrentSelectedTabPageIndex = TabView.SelectedIndex;

            for (int i = 0; i < TabView.TabItems.Count; i++)
            {
                if (TabView.TabItems[i] is muxc.TabViewItem Tab)
                {
                    if (Tab.Content is TextControlBox tb)
                    {
                        tb.tabdatafromdatabase.CurrentSelectedTabIndex = CurrentSelectedTabPageIndex;
                        outputtext += JsonConvert.SerializeObject(tb.tabdatafromdatabase) + "\n";
                    }
                }
            }
            return outputtext;
        }
        //Creates the database-file in a specified folder
        public async Task<bool> SaveTabPageData(muxc.TabView TabView)
        {
            try
            {
                if (TabView.TabItems.Count == 0)
                    return true;
                
                var FilePath = Path.Combine(DefaultValues.LocalFolderPath, DefaultValues.Database_FolderName, DataBaseName);
                return await WriteToFile(FilePath, CreateTabDataBase(TabView));
            }
            catch (Exception e)
            {
                await new InfoBox(e.Message).ShowAsync();
                return false;
            }
        }

        //Gets the data from the default App-Folder or from custom folder
        public async Task<List<TabDataForDatabase>> GetTabData(bool FromBackup)
        {
            try
            {
                List<TabDataForDatabase> databaseData = new List<TabDataForDatabase>();

                string DataBaseFileName =
                    Path.Combine(DefaultValues.LocalFolderPath, FromBackup ? DefaultValues.Backup_FolderName : DefaultValues.Database_FolderName, DataBaseName);

                if (File.Exists(DataBaseFileName))
                {
                    var lines = (await ReadFromFile(DataBaseFileName)).Split("\n", StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        databaseData.Add(JsonConvert.DeserializeObject<TabDataForDatabase>(lines[i]));
                    }
                    return databaseData;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in TabDataBase --> GetTabData:" + "\n" + e.Message);
            }
            return null;
        }

        //Delets all files from backup folder
        public void DeleteAllTemporarySavedFiles(string FolderPath = "")
        {
            if (FolderPath == "")
            {
                FolderPath = Path.Combine(DefaultValues.LocalFolderPath, DefaultValues.Database_FolderName);
            }

            DirectoryInfo dr = new DirectoryInfo(FolderPath);

            if (dr.Exists)
            {
                var FilePath = Directory.GetFiles(FolderPath);
                for (int i = 0; i < FilePath.Length; i++)
                {
                    if (FilePath[i] != null)
                    {
                        File.Delete(FilePath[i]);
                    }
                }
            }
        }
    }

    public class TabDataForDatabase
    {
        public string TabName { get; set; }
        public bool TabModified { get; set; }
        public string TabHeader { get; set; }
        public string TabToken { get; set; }
        public string TabPath { get; set; }
        public string DataBaseName { get; set; }
        public string TabTemp { get; set; }
        public bool TabReadOnly { get; set; }
        public int TabSelStart { get; set; }
        public int TabSelLenght { get; set; }
        public TabSaveMode TabSaveMode { get; set; }
        public int TabEncoding { get; set; }
        public double ZoomFactor { get; set; }
        public TextWrapping WordWrap { get; set; }
        public int CurrentSelectedTabIndex { get; set; }
        public bool Markdown { get; set; }
        public bool MarkdownMode2 { get; set; }
        public bool MarkdownIsColumn { get; set; }
        //public bool IsInSecondaryView { get; set; }
    }
}
