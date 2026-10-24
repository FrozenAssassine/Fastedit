using Fastedit.Core.Tab;
using Fastedit.Helper;
using Fastedit.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Fastedit.Dialogs;

public enum FileConflictResolution
{
    Reload,
    Overwrite,
    Ignore
}

public class FileConflictDialog
{
    private static readonly SemaphoreSlim _dialogSemaphore = new SemaphoreSlim(1, 1);

    public static async Task<FileConflictResolution> ShowAsync(
        TabPageItem tab,
        (string[] lines, System.Text.Encoding encoding, bool succeeded, bool mixedLineEndings, TextControlBoxNS.LineEnding lineEnding)? diskData = null,
        XamlRoot root = null)
    {
        if (tab == null || string.IsNullOrWhiteSpace(tab.DatabaseItem.FilePath))
            return FileConflictResolution.Ignore;

        // Prevent showing multiple concurrent ContentDialogs in WinUI 3
        if (!await _dialogSemaphore.WaitAsync(0))
        {
            // Another conflict dialog is currently open, avoid crashing WinUI 3
            return FileConflictResolution.Ignore;
        }

        try
        {
            var targetRoot = root ?? tab.XamlRoot ?? App.m_window?.Content?.XamlRoot;
            if (targetRoot == null)
                return FileConflictResolution.Ignore;

            string fileName = tab.DatabaseItem.FileName ?? Path.GetFileName(tab.DatabaseItem.FilePath);

            var contentPanel = new StackPanel { Spacing = 12 };
            contentPanel.Children.Add(new TextBlock
            {
                Text = $"The file \"{fileName}\" has been changed on disk by another program.",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap
            });
            contentPanel.Children.Add(new TextBlock
            {
                Text = "You also have unsaved modifications in Fastedit.\n\n" +
                       "• Reload: Discard your local changes and load the disk version.\n" +
                       "• Overwrite: Keep your local changes and overwrite the file on disk.\n" +
                       "• Ignore: Keep your changes in the editor without modifying disk.",
                TextWrapping = TextWrapping.Wrap
            });

            var dialog = new ContentDialog
            {
                XamlRoot = targetRoot,
                Background = DialogHelper.ContentDialogBackground(),
                Foreground = DialogHelper.ContentDialogForeground(),
                RequestedTheme = DialogHelper.DialogDesign,
                Title = $"File Conflict - {fileName}",
                Content = contentPanel,
                PrimaryButtonText = "Reload from disk",
                SecondaryButtonText = "Overwrite disk",
                CloseButtonText = "Ignore",
                DefaultButton = ContentDialogButton.Primary
            };

            var dlgRes = await dialog.ShowAsync();
            tab.HasPendingExternalConflict = false;

            if (dlgRes == ContentDialogResult.Primary)
            {
                await Fastedit.Core.Storage.FileChangeManager.ReloadTabFromDiskAsync(tab, diskData);
                return FileConflictResolution.Reload;
            }
            else if (dlgRes == ContentDialogResult.Secondary)
            {
                return FileConflictResolution.Overwrite;
            }
            else
            {
                // Acknowledged by user; update timestamp to current disk timestamp to avoid immediate re-prompt
                if (File.Exists(tab.DatabaseItem.FilePath))
                {
                    tab.LastKnownWriteTimeUtc = File.GetLastWriteTimeUtc(tab.DatabaseItem.FilePath);
                }
                return FileConflictResolution.Ignore;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FileConflictDialog error: {ex.Message}");
            return FileConflictResolution.Ignore;
        }
        finally
        {
            _dialogSemaphore.Release();
        }
    }
}
