using Fastedit.Dialogs;
using Fastedit.Settings;
using Fastedit.Tab;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace Fastedit.Storage
{
    public class SaveFileHelper
    {
        public static async Task<bool> WriteTextToFileAsync(StorageFile file, string text, Encoding encoding)
        {
            try
            {
                if (file == null)
                    return false;

                await FileIO.WriteTextAsync(file, "");
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    using (var writer = new StreamWriter(stream, encoding))
                    {
                        await writer.WriteAsync(text);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                try
                {
                    var bytestoWrite = encoding.GetBytes(text);
                    var buffer = encoding.GetPreamble().Concat(bytestoWrite).ToArray();
                    await PathIO.WriteBytesAsync(file.Path, buffer);
                    return true;
                }
                catch
                {
                    InfoMessages.NoAccesToSaveFile();
                }
            }
            return false;
        }
        public static async Task<bool> Save(TabPageItem tab)
        {
            if (tab == null)
                return false;

            //the textbox does not has any data when the tab is not loaded -> file will be emty: (#77)
            if (!tab.DataIsLoaded)
                await TabPageHelper.LoadUnloadedTab(tab);

            //file was already saved
            if (tab.DatabaseItem.FileToken.Length > 0)
            {
                StorageFile file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(tab.DatabaseItem.FileToken);
                if (file == null)
                    return await SaveFileAs(tab);
                else
                {
                    bool result = await WriteTextToFileAsync(file, tab.textbox.GetText(), tab.Encoding);
                    if (result)
                    {
                        TabPageHelper.UpdateSaveStatus(tab, false, tab.DatabaseItem.FileToken, file);
                    }
                    return result;
                }
            }
            else
            {
                return await SaveFileAs(tab);
            }
        }
        public static async Task<bool> SaveFileAs(TabPageItem tab)
        {
            if (tab == null)
                return false;

            var savePicker = new Windows.Storage.Pickers.FileSavePicker();

            StorageFile OpenedFile = null;
            if (tab.DatabaseItem.FileToken.Length > 0)
                OpenedFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(tab.DatabaseItem.FileToken);

            try
            {
                //Add the extension of the current file
                savePicker.FileTypeChoices.Add("Current extension", new List<string>() { OpenedFile != null ? OpenedFile.FileType : Path.GetExtension(tab.DatabaseItem.FileName) });
            }
            catch (ArgumentException) //not a valid file name:
            {
            }
            for (int i = 0; i < FileExtensions.FileExtentionList.Count; i++)
            {
                var item = FileExtensions.FileExtentionList[i];
                savePicker.FileTypeChoices.TryAdd(item.ExtensionName, item.Extension);
            }
            savePicker.SuggestedFileName = OpenedFile != null ? OpenedFile.DisplayName : tab.DatabaseItem.FileName;


            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);
                await WriteTextToFileAsync(file, tab.textbox.GetText(), tab.Encoding);
                Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    string token = StorageApplicationPermissions.FutureAccessList.Add(file);
                    TabPageHelper.UpdateSaveStatus(tab, false, token, file);
                    return true;
                }
            }
            return false;
        }

        public async static Task<bool> DragFileToPath(TabPageItem tab, TabViewTabDragStartingEventArgs args)
        {
            try
            {
                if (!Directory.Exists(DefaultValues.TemporaryFilesPath))
                    Directory.CreateDirectory(DefaultValues.TemporaryFilesPath);

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(DefaultValues.TemporaryFilesPath);

                StorageFile file = await folder.CreateFileAsync(tab.DatabaseItem.FileName, CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(file, tab.textbox.GetText());

                args.Data.SetStorageItems(new IStorageItem[] { file });
                return true;
            }
            catch (Exception ex)
            {
                InfoMessages.UnhandledException(ex.Message);
                return false;
            }
        }

        public async static Task<StorageFile> PickFile(string fileName, string extension, string extensionDisplayName)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.FileTypeChoices.Add(extensionDisplayName, new List<string>() { extension });

            return await savePicker.PickSaveFileAsync();
        }
    }
}
