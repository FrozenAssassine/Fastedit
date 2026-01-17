using Fastedit.Core.Settings;
using Fastedit.Core.Tab;
using Fastedit.Dialogs;
using Fastedit.Helper;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TextControlBoxNS;
using Windows.Storage;

namespace Fastedit.Storage;

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

    public static async Task<bool> WriteLinesToFile(string path, IEnumerable<string> lines, Encoding encoding, LineEnding lineEnding)
    {
        if (string.IsNullOrWhiteSpace(path) || lines == null || encoding == null)
            return false;

        string lineEndingStr = LineEndingHelper.GetLineEndingString(lineEnding);

        try
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 65536, useAsync: true))
            using (var writer = new StreamWriter(stream, encoding))
            {
                foreach (var line in lines)
                {
                    await writer.WriteAsync(line.AsMemory());
                    await writer.WriteAsync(lineEndingStr.AsMemory());
                }

                await writer.FlushAsync();
            }

            return true;
        }
        catch (UnauthorizedAccessException)
        {
            InfoMessages.NoAccessToSaveFile();
        }
        catch (Exception ex)
        {
            InfoMessages.UnhandledException(ex.Message);
        }

        return false;
    }

    public static async Task<bool> WriteTextToFileAsync(string path, string text, Encoding encoding)
    {
        if (string.IsNullOrWhiteSpace(path) || text == null || encoding == null)
            return false;

        try
        {
            // Open the file stream with async enabled
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 65536, useAsync: true))
            using (var writer = new StreamWriter(stream, encoding))
            {
                // Write the text asynchronously
                await writer.WriteAsync(text);
            }
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            InfoMessages.NoAccessToSaveFile();
        }
        catch (Exception ex)
        {
            InfoMessages.UnhandledException(ex.Message);
        }

        return false;
    }
    public static async Task<bool> Save(TabPageItem tab, Window window = null)
    {
        if (tab == null)
            return false;

        //the textbox does not has any data when the tab is not loaded -> file will be emty: (#77)
        if (!tab.DataIsLoaded)
            TabPageHelper.LoadUnloadedTab(tab);

        if (tab.DatabaseItem.WasNeverSaved)
            return await SaveFileAs(tab, window);

        bool result = await WriteLinesToFile(tab.DatabaseItem.FilePath, tab.textbox.Lines, tab.Encoding, tab.LineEnding);
        if (result)
        {
            TabPageHelper.UpdateSaveStatus(tab, false);
        }
        return result;
    }
    public static async Task<bool> SaveFileAs(TabPageItem tab, Window window = null)
    {
        if (tab == null)
            return false;


        var savePicker = new Windows.Storage.Pickers.FileSavePicker();
        savePicker.FileTypeChoices.Add("All Files", ["."]);
        savePicker.FileTypeChoices.Add("Current extension", [Path.GetExtension(tab.DatabaseItem.FileName)]);
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker,
            window != null ? WinRT.Interop.WindowNative.GetWindowHandle(window) : App.m_window.WindowHandle
            );

        for (int i = 0; i < FileExtensions.FileExtentionList.Count; i++)
        {
            var item = FileExtensions.FileExtentionList[i];
            savePicker.FileTypeChoices.TryAdd(item.ExtensionName, item.Extension);
        }
        savePicker.SuggestedFileName = tab.DatabaseItem.FileName;
        savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
        StorageFile file = await savePicker.PickSaveFileAsync();
        if (file != null)
        {
            await WriteLinesToFile(file.Path, tab.textbox.Lines, tab.Encoding, tab.LineEnding);

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

    public async static Task<string> PickFile(string extension, string extensionDisplayName, string fileName = "")
    {
        var savePicker = new Windows.Storage.Pickers.FileSavePicker();
        savePicker.FileTypeChoices.Add(extensionDisplayName, new List<string>() { extension });
        savePicker.SuggestedFileName = fileName;
        savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, App.m_window.WindowHandle);

        var file = await savePicker.PickSaveFileAsync();
        return file == null ? "" : file.Path;
    }
}
