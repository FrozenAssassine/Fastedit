using Fastedit.Core;
using Fastedit.Core.Tab;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit.ExternalData
{
    public class DatabaseImportExport
    {
        private MainPage mainpage = null;
        private muxc.TabView TextTabControl = null;
        private TabActions tabactions = null;
        private TabDataBase tabdatabase = new TabDataBase();

        public DatabaseImportExport(MainPage mainpage, muxc.TabView tabview)
        {
            this.mainpage = mainpage;
            this.TextTabControl = tabview;
            this.tabactions = new TabActions(tabview, mainpage);
        }

        public async Task<bool> CreateDatabaseBackup()
        {
            if(await tabactions.SaveAllTabChanges())
            {
                var databasefolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(DefaultValues.Database_FolderName, CreationCollisionOption.OpenIfExists);
                var backupfolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(DefaultValues.Backup_FolderName, CreationCollisionOption.OpenIfExists);
                if (databasefolder == null || backupfolder == null)
                    return false;
                else
                {
                    var backupedFiles = await backupfolder.GetFilesAsync();
                    for (int i = 0; i < backupedFiles.Count; i++)
                    {
                        var sf = backupedFiles[i] as StorageFile;
                        if (sf != null)
                        {
                            await sf.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        }
                    }
                    var files = await databasefolder.GetFilesAsync();
                    for (int i = 0; i< files.Count; i++)
                    {
                        if(files[i] is StorageFile file)
                        {
                            if (file != null)
                                if (await file.CopyAsync(backupfolder, file.Name, NameCollisionOption.ReplaceExisting) == null)
                                    return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        public async Task<bool> LoadDatabaseFromBackup()
        {
            return await tabactions.LoadTabs(true);
        }
    }
}
