using Fastedit.Core.Settings;
using Fastedit.Core.Tab;
using Fastedit.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fastedit.Storage;

public class OpenFileHelper
{
    private static IEnumerable<string> GetLines(StreamReader reader)
    {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            yield return line;
        }
    }
    public static (string[] lines, Encoding encoding, bool succeeded) ReadLinesFromFile(string path, Encoding encoding = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return (null, null, false);

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 65536, useAsync: false);
            using (var reader = new StreamReader(stream, encoding ?? Encoding.Default, detectEncodingFromByteOrderMarks: true))
            {
                var lines = GetLines(reader).ToArray();

                encoding ??= reader.CurrentEncoding;

                return (lines, encoding, true);
            }
        }
        catch (UnauthorizedAccessException)
        {
            InfoMessages.NoAccessToReadFile();
            return (null, null, false);
        }
        catch (Exception ex)
        {
            InfoMessages.UnhandledException(ex.Message);
        }
        return (null, null, false);
    }

    public static async Task<(string text, Encoding encoding, bool succeeded)> ReadTextFromFileAsync(string path, Encoding encoding = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return ("", Encoding.Default, false);

        try
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            using (var reader = new StreamReader(stream, encoding ?? Encoding.Default, detectEncodingFromByteOrderMarks: true))
            {
                string text = await reader.ReadToEndAsync();

                encoding ??= reader.CurrentEncoding;

                return (text, encoding, true);
            }
        }
        catch (UnauthorizedAccessException)
        {
            InfoMessages.NoAccessToReadFile();
        }
        catch (Exception ex)
        {
            InfoMessages.UnhandledException(ex.Message);
        }

        return ("", Encoding.Default, false);
    }

    private static bool DoOpenTab(TabPageItem tab, string path, bool load = true)
    {
        if (path == null)
            return false;

        var res = ReadLinesFromFile(path);
        if (!res.succeeded)
            return false;

        tab.DatabaseItem.FilePath = path;
        tab.DatabaseItem.FileName = Path.GetFileName(path);
        tab.Encoding = res.encoding;

        if (load)
            tab.textbox.LoadLines(res.lines);

        TabPageHelper.SelectHighlightLanguageByPath(tab);

        tab.textbox.GoToLine(0);
        tab.textbox.ScrollLineIntoView(0);
        tab.DataIsLoaded = load;
        tab.DatabaseItem.IsModified = false;
        tab.SetHeader(tab.DatabaseItem.FileName);

        if (!load)
        {
            File.Copy(path, Path.Combine(DefaultValues.DatabasePath, tab.DatabaseItem.Identifier));
        }

        return true;
    }
    public static TabPageItem DoOpen(TabView tabView, string path, bool load = true, bool select = false)
    {
        var tab = TabPageHelper.AddNewTab(tabView, false);
        if (!DoOpenTab(tab, path, load))
        {
            tabView.TabItems.Remove(tab);
            return null;
        }

        if(select)
            tabView.SelectedItem = tab;

        return tab;
    }
    public static async Task<bool> OpenFile(TabView tabView)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
        picker.FileTypeFilter.Add("*");
        WinRT.Interop.InitializeWithWindow.Initialize(picker, App.m_window.WindowHandle);

        bool res = true;
        var files = await picker.PickMultipleFilesAsync();
        foreach (var file in files)
        {
            var tab = DoOpen(tabView, file.Path);
            if (tab != null)
                tabView.SelectedItem = tab;
            else
                res = false;
        }
        return res;
    }

    public static bool ReopenWithEncoding(TabPageItem tab, Encoding encoding)
    {
        //File has not been saved:
        if (tab.DatabaseItem.WasNeverSaved)
            return false;

        var res = ReadLinesFromFile(tab.DatabaseItem.FilePath, encoding);
        if (res.succeeded)
        {
            tab.Encoding = res.encoding;
            tab.textbox.LoadLines(res.lines);
            return true;
        }
        return false;
    }

    public static async Task<bool> OpenFileForTab(TabPageItem tab, Window window = null)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
        picker.FileTypeFilter.Add("*");
        WinRT.Interop.InitializeWithWindow.Initialize(picker, window != null ? WinRT.Interop.WindowNative.GetWindowHandle(window) : App.m_window.WindowHandle);

        var file = await picker.PickSingleFileAsync();
        if (file == null)
            return false;

        DoOpenTab(tab, file.Path);
        return tab != null;
    }

    public static async Task<string> PickFile(string extension)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
        picker.FileTypeFilter.Add(extension);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, App.m_window.WindowHandle);

        var file = await picker.PickSingleFileAsync();
        if (file == null)
            return "";

        return file.Path;
    }
}
