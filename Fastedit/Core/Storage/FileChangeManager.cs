using Fastedit.Controls;
using Fastedit.Core.Tab;
using Fastedit.Dialogs;
using Fastedit.Helper;
using Fastedit.Storage;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TextControlBoxNS;

namespace Fastedit.Core.Storage;

public static class FileChangeManager
{
    private static readonly Dictionary<TabPageItem, TabFileWatcher> _watchers = new();
    private static readonly object _lock = new();

    public static void StartWatching(TabPageItem tab)
    {
        if (tab == null || !AppSettings.DetectExternalFileChanges)
            return;

        lock (_lock)
        {
            if (_watchers.TryGetValue(tab, out var existing))
            {
                existing.Dispose();
                _watchers.Remove(tab);
            }

            string filePath = tab.DatabaseItem?.FilePath;
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;

            try
            {
                tab.LastKnownWriteTimeUtc = File.GetLastWriteTimeUtc(filePath);
                var watcher = new TabFileWatcher(tab, filePath);
                _watchers[tab] = watcher;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FileChangeManager] Error starting watcher for {filePath}: {ex.Message}");
            }
        }
    }

    public static void StopWatching(TabPageItem tab)
    {
        if (tab == null)
            return;

        lock (_lock)
        {
            if (_watchers.TryGetValue(tab, out var watcher))
            {
                watcher.Dispose();
                _watchers.Remove(tab);
            }
        }
    }

    public static void UpdateWatchedPath(TabPageItem tab, string newPath)
    {
        if (tab == null)
            return;

        StopWatching(tab);
        if (!string.IsNullOrWhiteSpace(newPath) && File.Exists(newPath))
        {
            StartWatching(tab);
        }
    }

    public static void NotifySavingStarted(TabPageItem tab)
    {
        if (tab == null)
            return;

        tab.IsSelfSaving = true;
    }

    public static void NotifySavingFinished(TabPageItem tab)
    {
        if (tab == null)
            return;

        string filePath = tab.DatabaseItem?.FilePath;
        if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
        {
            tab.LastKnownWriteTimeUtc = File.GetLastWriteTimeUtc(filePath);
        }

        // Keep suppression active briefly to absorb any delayed FileSystemWatcher Changed events
        _ = Task.Run(async () =>
        {
            await Task.Delay(600);
            tab.IsSelfSaving = false;
        });
    }

    public static bool HasExternalConflict(TabPageItem tab)
    {
        if (tab == null || !AppSettings.DetectExternalFileChanges || tab.DatabaseItem == null || string.IsNullOrWhiteSpace(tab.DatabaseItem.FilePath))
            return false;

        string filePath = tab.DatabaseItem.FilePath;
        if (!File.Exists(filePath))
            return false;

        try
        {
            var diskWriteTime = File.GetLastWriteTimeUtc(filePath);
            return diskWriteTime > tab.LastKnownWriteTimeUtc;
        }
        catch
        {
            return false;
        }
    }

    public static void CheckTabForExternalChanges(TabPageItem tab)
    {
        if (tab == null || !AppSettings.DetectExternalFileChanges || tab.DatabaseItem == null || string.IsNullOrWhiteSpace(tab.DatabaseItem.FilePath))
            return;

        if (tab.IsSelfSaving)
            return;

        string filePath = tab.DatabaseItem.FilePath;
        if (!File.Exists(filePath))
        {
            HandleFileDeleted(tab);
            return;
        }

        try
        {
            var currentWriteTime = File.GetLastWriteTimeUtc(filePath);
            if (currentWriteTime > tab.LastKnownWriteTimeUtc)
            {
                _ = ProcessExternalChangeAsync(tab, filePath, currentWriteTime);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FileChangeManager] CheckTabForExternalChanges error: {ex.Message}");
        }
    }

    public static void CheckForExternalChanges()
    {
        TabPageItem[] activeTabs;
        lock (_lock)
        {
            activeTabs = _watchers.Keys.ToArray();
        }

        foreach (var tab in activeTabs)
        {
            CheckTabForExternalChanges(tab);
        }
    }

    public static async Task ProcessExternalChangeAsync(TabPageItem tab, string filePath, DateTime diskWriteTime)
    {
        if (tab == null || tab.IsSelfSaving)
            return;

        if (!File.Exists(filePath))
        {
            HandleFileDeleted(tab);
            return;
        }

        var readResult = await OpenFileHelper.ReadLinesFromFileWithRetryAsync(filePath, tab.Encoding);
        if (!readResult.succeeded || readResult.lines == null)
            return;

        // Check if disk content is actually identical to current editor content
        bool isIdentical = AreLinesEqual(tab.textbox?.Lines, readResult.lines);
        if (isIdentical)
        {
            tab.LastKnownWriteTimeUtc = diskWriteTime;
            return;
        }

        if (tab.DatabaseItem.IsModified)
        {
            // Tab is modified in Fastedit:
            // Do NOT show dialog while typing or if external writes occur (e.g. log file).
            // The conflict dialog will ONLY appear when the user tries to Save.
            tab.HasPendingExternalConflict = true;
            return;
        }
        else
        {
            if (AppSettings.AutoReloadUnmodifiedFiles)
            {
                // Tab is clean: Seamlessly reload from disk (like VS Code)
                await ReloadTabFromDiskAsync(tab, readResult, diskWriteTime);
                ShowReloadNotification(tab.DatabaseItem.FileName ?? Path.GetFileName(filePath));
            }
            else
            {
                tab.HasPendingExternalConflict = true;
            }
        }
    }

    public static async Task ReloadTabFromDiskAsync(
        TabPageItem tab,
        (string[] lines, System.Text.Encoding encoding, bool succeeded, bool mixedLineEndings, LineEnding lineEnding)? diskData = null,
        DateTime? diskWriteTime = null)
    {
        if (tab == null || string.IsNullOrWhiteSpace(tab.DatabaseItem.FilePath))
            return;

        string filePath = tab.DatabaseItem.FilePath;
        var data = diskData ?? await OpenFileHelper.ReadLinesFromFileWithRetryAsync(filePath, tab.Encoding);
        if (!data.succeeded || data.lines == null)
            return;

        // Preserve cursor position if possible
        int line = tab.textbox.CursorPosition.LineNumber;
        int character = tab.textbox.CursorPosition.CharacterPosition;

        tab.LoadLines(data.lines, true, data.lineEnding);

        int maxLines = tab.textbox.NumberOfLines;
        line = Math.Clamp(line, 0, Math.Max(0, maxLines - 1));
        tab.textbox.SetCursorPosition(line, character);

        tab.Encoding = data.encoding;
        tab.LineEnding = data.lineEnding;
        tab.DatabaseItem.IsModified = false;
        tab.HasPendingExternalConflict = false;
        tab.LastKnownWriteTimeUtc = diskWriteTime ?? (File.Exists(filePath) ? File.GetLastWriteTimeUtc(filePath) : DateTime.UtcNow);

        tab.UpdateHeader();
        TabPageHelper.SelectHighlightLanguageByPath(tab);
        TabPageHelper.mainPage?.TextStatusBar?.UpdateAll();
    }

    private static InfoBar _activeReloadInfoBar = null;

    private static void ShowReloadNotification(string fileName)
    {
        if (_activeReloadInfoBar == null || !_activeReloadInfoBar.IsOpen)
        {
            _activeReloadInfoBar = new InfoBar();
            _activeReloadInfoBar.Show("File Updated", $"'{fileName}' was updated from disk.", InfoBarSeverity.Informational, 3);
        }
    }

    public static void HandleFileDeleted(TabPageItem tab)
    {
        if (tab == null)
            return;

        tab.DatabaseItem.IsModified = true;
        tab.UpdateHeader();

        string fileName = tab.DatabaseItem.FileName ?? "File";

        var saveButton = new Button
        {
            Content = "Save to recreate"
        };
        saveButton.Click += async delegate
        {
            await SaveFileHelper.Save(tab, forceOverwrite: true);
        };

        new InfoBar().Show(
            "File Deleted",
            $"'{fileName}' was deleted on disk. Editor content is preserved. Save to recreate the file.",
            saveButton,
            InfoBarSeverity.Warning,
            10
        );
    }

    public static void HandleFileRenamed(TabPageItem tab, string oldFullPath, string newFullPath)
    {
        if (tab == null || string.IsNullOrWhiteSpace(newFullPath))
            return;

        tab.DatabaseItem.FilePath = newFullPath;
        tab.DatabaseItem.FileName = Path.GetFileName(newFullPath);
        tab.SetHeader(tab.DatabaseItem.FileName);
        tab.UpdateHeader();

        TabPageHelper.SelectHighlightLanguageByPath(tab);
        TabPageHelper.mainPage?.TextStatusBar?.UpdateFile();

        // Update watcher with new path
        UpdateWatchedPath(tab, newFullPath);

        new InfoBar().Show(
            "File Renamed",
            $"File was renamed to '{tab.DatabaseItem.FileName}' on disk.",
            InfoBarSeverity.Informational,
            4
        );
    }

    public static bool AreLinesEqual(IEnumerable<string> editorLines, string[] diskLines)
    {
        if (editorLines == null && diskLines == null) return true;
        if (editorLines == null || diskLines == null) return false;

        if (editorLines is IReadOnlyList<string> list)
        {
            if (list.Count != diskLines.Length) return false;
            for (int i = 0; i < diskLines.Length; i++)
            {
                if (!string.Equals(list[i], diskLines[i], StringComparison.Ordinal))
                    return false;
            }
            return true;
        }

        using var e1 = editorLines.GetEnumerator();
        int index = 0;
        while (e1.MoveNext())
        {
            if (index >= diskLines.Length) return false;
            if (!string.Equals(e1.Current, diskLines[index], StringComparison.Ordinal))
                return false;
            index++;
        }
        return index == diskLines.Length;
    }

    public static void StartAll(TabView tabView)
    {
        if (tabView == null || !AppSettings.DetectExternalFileChanges)
            return;

        foreach (var item in tabView.TabItems)
        {
            if (item is TabPageItem tab)
            {
                StartWatching(tab);
            }
        }
    }

    public static void StopAll()
    {
        lock (_lock)
        {
            foreach (var watcher in _watchers.Values)
            {
                watcher.Dispose();
            }
            _watchers.Clear();
        }
    }
}

public class TabFileWatcher : IDisposable
{
    private readonly TabPageItem _tab;
    private readonly string _filePath;
    private FileSystemWatcher _watcher;
    private DispatcherTimer _debounceTimer;
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _isDisposed = false;

    public TabFileWatcher(TabPageItem tab, string filePath)
    {
        _tab = tab;
        _filePath = filePath;
        _dispatcherQueue = tab.DispatcherQueue ?? MainWindow.UIDispatcherQueue ?? DispatcherQueue.GetForCurrentThread();

        InitWatcher();
    }

    private void InitWatcher()
    {
        try
        {
            string dir = Path.GetDirectoryName(_filePath);
            string file = Path.GetFileName(_filePath);

            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                return;

            _watcher = new FileSystemWatcher(dir, file)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnWatcherChanged;
            _watcher.Created += OnWatcherChanged;
            _watcher.Deleted += OnWatcherDeleted;
            _watcher.Renamed += OnWatcherRenamed;
            _watcher.Error += OnWatcherError;

            // Debounce timer on UI thread
            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _debounceTimer.Tick += DebounceTimer_Tick;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TabFileWatcher] Failed to init watcher for {_filePath}: {ex.Message}");
        }
    }

    private void OnWatcherChanged(object sender, FileSystemEventArgs e)
    {
        if (_isDisposed || _tab.IsSelfSaving)
            return;

        _dispatcherQueue?.TryEnqueue(() =>
        {
            if (_isDisposed || _tab.IsSelfSaving)
                return;

            _debounceTimer?.Stop();
            _debounceTimer?.Start();
        });
    }

    private void DebounceTimer_Tick(object sender, object e)
    {
        _debounceTimer?.Stop();

        if (_isDisposed || _tab.IsSelfSaving)
            return;

        if (!File.Exists(_filePath))
        {
            FileChangeManager.HandleFileDeleted(_tab);
            return;
        }

        try
        {
            var diskWriteTime = File.GetLastWriteTimeUtc(_filePath);
            if (diskWriteTime <= _tab.LastKnownWriteTimeUtc)
                return;

            _ = FileChangeManager.ProcessExternalChangeAsync(_tab, _filePath, diskWriteTime);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TabFileWatcher] Error in debounce tick: {ex.Message}");
        }
    }

    private void OnWatcherDeleted(object sender, FileSystemEventArgs e)
    {
        if (_isDisposed || _tab.IsSelfSaving)
            return;

        _dispatcherQueue?.TryEnqueue(() =>
        {
            if (_isDisposed || _tab.IsSelfSaving)
                return;

            // Wait a brief moment to differentiate between atomic replace (delete + create) and real delete
            _ = Task.Run(async () =>
            {
                await Task.Delay(250);
                _dispatcherQueue?.TryEnqueue(() =>
                {
                    if (_isDisposed || _tab.IsSelfSaving)
                        return;

                    if (!File.Exists(_filePath))
                    {
                        FileChangeManager.HandleFileDeleted(_tab);
                    }
                    else
                    {
                        // File was recreated / replaced atomically
                        var diskWriteTime = File.GetLastWriteTimeUtc(_filePath);
                        if (diskWriteTime > _tab.LastKnownWriteTimeUtc)
                        {
                            _ = FileChangeManager.ProcessExternalChangeAsync(_tab, _filePath, diskWriteTime);
                        }
                    }
                });
            });
        });
    }

    private void OnWatcherRenamed(object sender, RenamedEventArgs e)
    {
        if (_isDisposed || _tab.IsSelfSaving)
            return;

        _dispatcherQueue?.TryEnqueue(() =>
        {
            if (_isDisposed || _tab.IsSelfSaving)
                return;

            FileChangeManager.HandleFileRenamed(_tab, e.OldFullPath, e.FullPath);
        });
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        Debug.WriteLine($"[TabFileWatcher] Error event: {e.GetException()?.Message}");
    }

    public void Dispose()
    {
        _isDisposed = true;

        if (_debounceTimer != null)
        {
            _debounceTimer.Stop();
            _debounceTimer.Tick -= DebounceTimer_Tick;
            _debounceTimer = null;
        }

        if (_watcher != null)
        {
            try
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= OnWatcherChanged;
                _watcher.Created -= OnWatcherChanged;
                _watcher.Deleted -= OnWatcherDeleted;
                _watcher.Renamed -= OnWatcherRenamed;
                _watcher.Error -= OnWatcherError;
                _watcher.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TabFileWatcher] Dispose error: {ex.Message}");
            }
            finally
            {
                _watcher = null;
            }
        }
    }
}
