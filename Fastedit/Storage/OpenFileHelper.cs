using Fastedit.Dialogs;
using Fastedit.Helper;
using Fastedit.Settings;
using Fastedit.Tab;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace Fastedit.Storage
{
    public class OpenFileHelper
    {
        public static async Task<(string Text, Encoding encoding, bool Succed)> ReadTextFromFileAsync(StorageFile file, Encoding encoding = null)
        {
            try
            {
                if (file == null)
                    return ("", Encoding.Default, false);

                using (var stream = (await file.OpenReadAsync()).AsStreamForRead())
                {
                    //Detect the encoding:
                    using (var reader = new StreamReader(stream, true))
                    {
                        byte[] buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);

                        if (encoding != null)//Encoding is predefined
                            return (encoding.GetString(buffer, 0, buffer.Length), encoding, true);

                        //Encoding gets detected
                        encoding = EncodingHelper.DetectTextEncoding(buffer, out string text);
                        return (text, encoding, true);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                InfoMessages.NoAccesToReadFile();
            }
            catch (Exception ex)
            {
                InfoMessages.UnhandledException(ex.Message);
            }
            return ("", Encoding.Default, false);
        }

        private static async Task<bool> DoOpenTab(TabPageItem tab, StorageFile file, bool load = true)
        {
            if (file == null)
                return false;

            var res = await ReadTextFromFileAsync(file);
            if (!res.Succed)
                return false;

            tab.DatabaseItem.FilePath = file.Path;
            tab.DatabaseItem.FileName = file.Name;
            try
            {
                tab.DatabaseItem.FileToken = StorageApplicationPermissions.FutureAccessList.Add(file);
            }
            catch (Exception ex)
            {
                InfoMessages.UnhandledException(ex.Message);
                return false;
            }
            tab.Encoding = res.encoding;

            if (load)
                tab.textbox.LoadText(res.Text);

            TabPageHelper.SelectCodeLanguageByFile(tab, file);

            tab.textbox.GoToLine(0);
            tab.textbox.ScrollLineIntoView(0);
            tab.DataIsLoaded = load;
            tab.DatabaseItem.IsModified = false;
            tab.SetHeader(file.Name);

            if (!load)
            {
                var folder = await StorageFolder.GetFolderFromPathAsync(DefaultValues.DatabasePath);
                var newFile = await file.CopyAsync(folder);
                await newFile.RenameAsync(tab.DatabaseItem.Identifier);
            }

            return true;
        }
        public static async Task<TabPageItem> DoOpen(TabView tabView, StorageFile file, bool load = true)
        {
            var tab = TabPageHelper.AddNewTab(tabView, false);
            if (!await DoOpenTab(tab, file, load))
            {
                tabView.TabItems.Remove(tab);
                return null;
            }

            return tab;
        }
        public static async Task<bool> OpenFile(TabView tabView)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add("*");

            bool res = true;
            var files = await picker.PickMultipleFilesAsync();
            foreach (var file in files)
            {
                var tab = await DoOpen(tabView, file);
                if (tab != null)
                    tabView.SelectedItem = tab;
                else
                    res = false;
            }
            return res;
        }

        public static async Task<bool> ReopenWithEncoding(TabPageItem tab, Encoding encoding)
        {
            //File has not been saved:
            if (tab.DatabaseItem.FileToken.Length == 0)
                return false;

            var getFileRes = await FutureAccessListHelper.GetFileAsync(tab.DatabaseItem.FileToken);
            if (!getFileRes.success)
            {
                InfoMessages.FileNotFoundReopenWithEncoding();
                return false;
            }

            var res = await ReadTextFromFileAsync(getFileRes.file, encoding);
            if (res.Succed)
            {
                tab.Encoding = res.encoding;
                tab.textbox.LoadText(res.Text);
                return true;
            }
            return false;
        }

        public static async Task<bool> OpenFileForTab(TabPageItem tab)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add("*");

            var file = await picker.PickSingleFileAsync();
            if (file == null)
                return false;

            await DoOpenTab(tab, file);
            return tab != null;
        }

        public static async Task<StorageFile> PickFile(string extension)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(extension);

            return await picker.PickSingleFileAsync();
        }
    }
}
