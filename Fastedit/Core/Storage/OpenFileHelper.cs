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
using TextControlBoxNS;

namespace Fastedit.Core.Storage;

public class OpenFileHelper
{
    private static (IEnumerable<string> lines, bool mixedEndings, LineEnding lineEnding) GetLinesAndDetectMixed(StreamReader reader)
    {
        var sb = new StringBuilder();
        var lines = new List<string>();
        bool seenCRLF = false;
        bool seenLF = false;
        bool seenCR = false;

        int c;
        while ((c = reader.Read()) != -1)
        {
            if (c == '\r')
            {
                int next = reader.Peek();
                if (next == '\n')
                {
                    reader.Read();
                    seenCRLF = true;
                }
                else
                {
                    seenCR = true;
                }

                lines.Add(sb.ToString());
                sb.Clear();
            }
            else if (c == '\n')
            {
                seenLF = true;
                lines.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append((char)c);
            }
        }

        if (sb.Length > 0)
            lines.Add(sb.ToString());

        bool mixed = (seenCRLF ? 1 : 0) + (seenLF ? 1 : 0) + (seenCR ? 1 : 0) > 1;

        LineEnding ending = LineEnding.CRLF;
        if (!mixed)
        {
            if (seenCRLF) ending = LineEnding.CRLF;
            else if (seenLF) ending = LineEnding.LF;
            else if (seenCR) ending = LineEnding.CR;
        }

        return (lines, mixed, ending);
    }

    public static (string[] lines, Encoding encoding, bool succeeded, bool mixedLineEndings, LineEnding lineEnding) ReadLinesFromFile(string path, Encoding encoding = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return (null, null, false, false, LineEnding.CRLF);

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 65536, useAsync: false);
            using (var reader = new StreamReader(stream, encoding ?? Encoding.Default, detectEncodingFromByteOrderMarks: true))
            {
                var getLinesResult = GetLinesAndDetectMixed(reader);

                encoding ??= reader.CurrentEncoding;

                return (getLinesResult.lines.ToArray(), encoding, true, getLinesResult.mixedEndings, getLinesResult.lineEnding);
            }
        }
        catch (UnauthorizedAccessException)
        {
            InfoMessages.NoAccessToReadFile();
            return (null, null, false, false, LineEnding.CRLF);
        }
        catch (Exception ex)
        {
            InfoMessages.UnhandledException(ex.Message);
        }
        return (null, null, false, false, LineEnding.CRLF);
    }

    private static async Task<bool> DoOpenTab(TabPageItem tab, string path, bool load = true)
    {
        if (path == null)
            return false;

        var res = ReadLinesFromFile(path);
        if (!res.succeeded)
            return false;

        LineEnding lineEnding;
        if (res.mixedLineEndings)
        {
            var mixedDialogRes = await MixedLineEndingsWarningDialog.Show();
            if (mixedDialogRes.confirmed)
                lineEnding = mixedDialogRes.lineEnding;
            else
                return false;
        }
        else
            lineEnding = res.lineEnding;

        tab.DatabaseItem.FilePath = path;
        tab.DatabaseItem.FileName = Path.GetFileName(path);
        tab.Encoding = res.encoding;
        tab.LineEnding = lineEnding;

        tab.textbox.Loaded += (sender) =>
        {
            if (load)
            {
                tab.LoadLines(res.lines, true, lineEnding);
            }

            TabPageHelper.SelectHighlightLanguageByPath(tab);
            tab.textbox.GoToLine(0);
            tab.textbox.ScrollLineIntoView(0);
        };

        tab.DataIsLoaded = load;
        tab.DatabaseItem.IsModified = false;
        tab.SetHeader(tab.DatabaseItem.FileName);

        if (!load)
        {
            File.Copy(path, Path.Combine(DefaultValues.DatabasePath, tab.DatabaseItem.Identifier));
        }

        return true;
    }

    public static async Task<TabPageItem> DoOpenAsync(TabView tabView, string path, bool load = true, bool select = false)
    {
        var tab = TabPageHelper.AddNewTab(tabView, false);
        if (!await DoOpenTab(tab, path, load))
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
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;

        WinRT.Interop.InitializeWithWindow.Initialize(picker, App.m_window.WindowHandle);

        bool res = true;
        var files = await picker.PickMultipleFilesAsync();
        foreach (var file in files)
        {
            var tab = await DoOpenAsync(tabView, file.Path);
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
            tab.LoadLines(res.lines);
            return true;
        }
        return false;
    }

    public static async Task<bool> OpenFileForTab(TabPageItem tab, Window window = null)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
        picker.FileTypeFilter.Add("*");
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
        WinRT.Interop.InitializeWithWindow.Initialize(picker, window != null ? WinRT.Interop.WindowNative.GetWindowHandle(window) : App.m_window.WindowHandle);

        var file = await picker.PickSingleFileAsync();
        if (file == null)
            return false;

        await DoOpenTab(tab, file.Path);
        return tab != null;
    }

    public static async Task<string> PickFile(string extension)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
        picker.FileTypeFilter.Add(extension);
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
        WinRT.Interop.InitializeWithWindow.Initialize(picker, App.m_window.WindowHandle);

        var file = await picker.PickSingleFileAsync();
        if (file == null)
            return "";

        return file.Path;
    }
}
