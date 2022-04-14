using Fastedit.Controls.Textbox;
using Fastedit.Dialogs;
using Fastedit.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit.Helper
{
    public class SaveFileHelper
    {
        private TabPageHelper tabpagehelper = new TabPageHelper();
        /// <summary>
        /// Save the file with a filepicker
        /// </summary>
        /// <param name="TabPage">Whether the file was saved successfully</param>
        /// <returns></returns>
        public async Task<bool> SaveFileAs(muxc.TabViewItem TabPage)
        {
            try
            {
                FileExtensions fileextentions = new FileExtensions();
                FileSavePicker savePicker = new FileSavePicker();
                bool ExtensionIsRequested = false;

                //Request a fileextension:
                var Textbox = tabpagehelper.GetTextBoxFromTabPage(TabPage);
                if (Textbox!=null)
                {
                    if (Textbox.MarkdownPreview && tabpagehelper.GetTabSaveMode(TabPage) == TabSaveMode.SaveAsTemp)
                    {
                        savePicker.FileTypeChoices.Add("Markdown", new List<string>() { ".md" });
                        ExtensionIsRequested = true;
                    }
                    else if (tabpagehelper.GetFileExtension(TabPage).Length != 0)
                    {
                        savePicker.FileTypeChoices.Add("Current file extension", new List<string>() { tabpagehelper.GetFileExtension(TabPage) });
                    }
                }
                for (int i = 0; i < fileextentions.FileExtentionList.Count; i++)
                {
                    var item = fileextentions.FileExtentionList[i];
                    savePicker.FileTypeChoices.TryAdd(item.ExtensionName, item.Extension);
                }
                savePicker.SuggestedFileName = ExtensionIsRequested ? Path.GetFileNameWithoutExtension(tabpagehelper.GetTabHeader(TabPage)) : tabpagehelper.GetTabHeader(TabPage);

                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    if (Path.GetExtension(file.DisplayName) != string.Empty)
                    {
                        await file.RenameAsync(file.DisplayName);
                    }
                    bool result = await WriteTextToFile(file, tabpagehelper.GetTabEncoding(TabPage), tabpagehelper.GetTabText(TabPage), TabSaveMode.SaveAsFile);
                    if (result)
                    {
                        Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);

                        if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                        {
                            string Token = StorageApplicationPermissions.FutureAccessList.Add(file);

                            var textbox = tabpagehelper.GetTextBoxFromTabPage(TabPage);
                            textbox.ShowInfoToast(true);
                            textbox.TextBeforeLastSaved = textbox.GetText();
                            textbox.FilePath = file.Path;
                            textbox.DataBaseName = file.Name;
                            textbox.TempFile = string.Empty;
                            textbox.Storagefile = file;
                            textbox.TabSaveMode = TabSaveMode.SaveAsFile;
                            textbox.FileToken = Token;

                            tabpagehelper.SetTabHeader(TabPage, file.Name);
                            tabpagehelper.SetTabModified(TabPage, false);
                            return true;
                        }
                        else
                        {
                            ErrorDialogs.SaveFileError(tabpagehelper.GetTabHeader(TabPage));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var filepath = string.Empty;
                if (TabPage.Content is TextControlBox tb)
                {
                    filepath = tb.FilePath;
                }
                Debug.WriteLine("Exception in SaveFileHelper -> SaveFileAs\n" + e.Message);
                ErrorDialogs.OpenErrorDialog(filepath);
            }
            return false;
        }

        /// <summary>
        /// Save a file, which was already saved
        /// </summary>
        /// <param name="TabPage">The tabpage</param>
        /// <param name="SaveChangesToDataBase">Wheater the databse is updated or not</param>
        /// <returns>Wheather the file save succed</returns>
        public async Task<bool> SaveFile(muxc.TabViewItem TabPage, bool SaveChangesToDataBase = true)
        {
            try
            {
                StorageFile file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(tabpagehelper.GetTabToken(TabPage));

                if (file != null)
                {
                    var result = await WriteTextToFile(file, tabpagehelper.GetTabEncoding(TabPage), tabpagehelper.GetTabText(TabPage), tabpagehelper.GetTabSaveMode(TabPage));
                    if (result == true)
                    {
                        var textbox = tabpagehelper.GetTextBoxFromTabPage(TabPage);
                        textbox.ShowInfoToast(true);
                        textbox.TextBeforeLastSaved = textbox.GetText();
                        textbox.FilePath = file.Path;
                        textbox.DataBaseName = file.Name;
                        textbox.TempFile = string.Empty;
                        textbox.Storagefile = file;

                        tabpagehelper.SetTabModified(TabPage, false);
                        tabpagehelper.SetTabHeader(TabPage, file.Name);
                        return true;
                    }
                }
            }
            catch
            {
                return await ShowAskSaveDialogForTab(TabPage, true, false);
            }
            return false;
        }

        /// <summary>
        /// Chooses whether SaveFileAs or SaveFile, depending on the TabSaveMode
        /// </summary>
        /// <param name="TabPage">The tabpage, to save the content from</param>
        /// <returns>Wheter the file was saved or not</returns>
        public async Task<bool> Save(muxc.TabViewItem TabPage)
        {
            if (TabPage != null)
            {
                if (tabpagehelper.GetTabSaveMode(TabPage) == TabSaveMode.SaveAsTemp)
                {
                    return await SaveFileAs(TabPage);
                }
                else
                {
                    return await SaveFile(TabPage);
                }
            }
            return false;
        }

        /// <summary>
        /// Writes text to file
        /// </summary>
        /// <param name="file">The file to write to</param>
        /// <param name="encoding">The encoding to write with</param>
        /// <param name="Text">The text to write</param>
        public async Task<bool> WriteTextToFile(StorageFile file, Encoding encoding, string Text, TabSaveMode savemode)
        {
            try
            {
                //Do this only when the file was Dragged
                if (savemode == TabSaveMode.SaveAsDragDrop)
                {
                    var bytestoWrite = encoding.GetBytes(Text);
                    var buffer = encoding.GetPreamble().Concat(bytestoWrite).ToArray();
                    await PathIO.WriteBytesAsync(file.Path, buffer);
                    return true;
                }
                //Do this every time savemode != Dragdrop
                else
                {
                    if (file != null)
                    {
                        await FileIO.WriteTextAsync(file, "");
                        using (var stream = await file.OpenStreamForWriteAsync())
                        {
                            var writer = new StreamWriter(stream, encoding);
                            await writer.WriteAsync(Text);
                            writer.Close();
                            writer.Dispose();
                        }
                        return true;
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                string filepath = string.Empty;
                if (file != null)
                {
                    filepath = file.Path;
                }

                await new InfoBox(e.Message + "\nFile: " + tabpagehelper.GetPathFromStorageFile(file), "No access to write to this file").ShowAsync();
            }
            catch (Exception e)
            {
                Debug.Write("Exception in SaveFileHelper -> WriteTextToFile\n" + e.Message);

                await new InfoBox(e.Message + "\nFile: " + tabpagehelper.GetPathFromStorageFile(file), "No access to write to this file").ShowAsync();
            }
            return false;
        }

        public async Task<bool> ShowAskSaveDialogForTab(muxc.TabViewItem Tab, bool ShowFileWasDeletedDialog, bool FileWasDeletetWithSecondaryButton = true)
        {
            if (!ShowFileWasDeletedDialog)
            {
                ContentDialogResult dlgres;
                if (tabpagehelper.GetTabSaveMode(Tab) == TabSaveMode.SaveAsFile || tabpagehelper.GetTabSaveMode(Tab) == TabSaveMode.SaveAsDragDrop)
                {
                    dlgres = await SaveDialogs.AskSaveDialog(Tab);
                }
                else
                {
                    dlgres = await SaveDialogs.AskSaveDialogNeverSaved(Tab);
                }

                if (dlgres == ContentDialogResult.Primary)
                {
                    return await Save(Tab);
                }
                else if (dlgres == ContentDialogResult.Secondary)
                {
                    return true;
                }
            }
            else
            {
                var dlgres = await SaveDialogs.FileWasDeletedDialog(Tab, FileWasDeletetWithSecondaryButton);
                if (dlgres == ContentDialogResult.Primary)
                {
                    return await SaveFileAs(Tab);
                }
                else if (dlgres == ContentDialogResult.Secondary)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
