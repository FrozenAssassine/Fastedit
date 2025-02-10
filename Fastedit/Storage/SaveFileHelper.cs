using Fastedit.Dialogs;
using Fastedit.Settings;
using Fastedit.Tab;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fastedit.Storage
{
    public class SaveFileHelper
    {
        public static string GenerateUniqueNameFromPath(string filePath)
        {
            int count = 0;
            string path = filePath;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            string pathWithoutFile = Path.GetDirectoryName(filePath);

            while (File.Exists(path))
            {
                path = Path.Join($"{fileNameWithoutExtension}({count++}){extension}");
            }
            return $"{fileNameWithoutExtension}({count++}){extension}";
        }

        public static async Task<bool> WriteTextToFileAsync(string path, string text, Encoding encoding)
        {
            if (string.IsNullOrWhiteSpace(path) || text == null || encoding == null)
                return false;

            try
            {
                // Open the file stream with async enabled
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                using (var writer = new StreamWriter(stream, encoding))
                {
                    // Write the text asynchronously
                    await writer.WriteAsync(text);
                }
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                InfoMessages.NoAccesToSaveFile();
            }
            catch (Exception ex)
            {
                InfoMessages.UnhandledException(ex.Message);
            }

            return false;
        }
        public static async Task<bool> Save(TabPageItem tab)
        {
            if (tab == null)
                return false;

            //the textbox does not has any data when the tab is not loaded -> file will be emty: (#77)
            if (!tab.DataIsLoaded)
                TabPageHelper.LoadUnloadedTab(tab);

            if (tab.DatabaseItem.WasNeverSaved)
                return await SaveFileAs(tab);

            bool result = await WriteTextToFileAsync(tab.DatabaseItem.FilePath, tab.textbox.GetText(), tab.Encoding);
            if (result)
            {
                TabPageHelper.UpdateSaveStatus(tab, false);
            }
            return result;
        }
        public static async Task<bool> SaveFileAs(TabPageItem tab)
        {
            if (tab == null)
                return false;


            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.FileTypeChoices.Add("Current extension", [Path.GetExtension(tab.DatabaseItem.FileName)]);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, App.m_window.WindowHandle);

            for (int i = 0; i < FileExtensions.FileExtentionList.Count; i++)
            {
                var item = FileExtensions.FileExtentionList[i];
                savePicker.FileTypeChoices.TryAdd(item.ExtensionName, item.Extension);
            }
            savePicker.SuggestedFileName = tab.DatabaseItem.FileName;

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                await WriteTextToFileAsync(file.Path, tab.textbox.GetText(), tab.Encoding);

                tab.DatabaseItem.FilePath = file.Path;
                tab.DatabaseItem.FileName = file.Name;

                TabPageHelper.UpdateSaveStatus(tab, false);
                return true;
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
                await FileIO.WriteLinesAsync(file, tab.textbox.Lines);

                args.Data.SetStorageItems([file]);
                return true;
            }
            catch (Exception ex)
            {
                InfoMessages.UnhandledException(ex.Message);
                return false;
            }
        }

        public async static Task<string> PickFile(string extension, string extensionDisplayName)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.FileTypeChoices.Add(extensionDisplayName, new List<string>() { extension });
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, App.m_window.WindowHandle);

            var file = await savePicker.PickSaveFileAsync();
            return file == null ? "" : file.Path;
        }
    }
}
