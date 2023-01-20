using Fastedit.Helper;
using Fastedit.Settings;
using Fastedit.Storage;
using Fastedit.Tab;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
    public class RecycleBinDialog
    {
        ListView listview = null;
        ContentDialog dialog = null;
        TabView tabView = null;

        public async Task ShowDialog()
        {
            if (dialog != null)
            {
                await dialog.ShowAsync();
            }
        }

        public RecycleBinDialog(TabView tabView)
        {
            this.tabView = tabView;

            listview = new ListView()
            {
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch,
                SelectionMode = ListViewSelectionMode.Multiple
            };
            dialog = new ContentDialog()
            {
                Background = DialogHelper.ContentDialogBackground(),
                Foreground = DialogHelper.ContentDialogForeground(),
                RequestedTheme = DialogHelper.DialogDesign,
                Title = "Recycle bin",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Delete",
                Content = listview,
            };
            dialog.Closing += Dialog_Closing;
            listview.SelectionChanged += Listview_SelectionChanged;
            dialog.PrimaryButtonClick += RecyclebinWindow_PrimaryButtonClick;
            dialog.SecondaryButtonClick += RecyclebinWindow_SecondaryButtonClick;
            UpdateListViewItems(listview);
        }

        private void Dialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            listview.Items.Clear();
        }

        //delete selected items:
        private async void RecyclebinWindow_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (listview.SelectedItems.Count == 0)
                return;

            args.Cancel = true;

            try
            {
                while (listview.SelectedItems.Count > 0)
                {
                    var selecteditem = listview.SelectedItems[listview.SelectedItems.Count - 1] as RecycleBinListViewItem;
                    await selecteditem.file.DeleteAsync();
                    listview.Items.Remove(selecteditem);
                }
            }
            catch (Exception ex)
            {
                InfoMessages.DeleteFromRecyclebinError();
                Debug.WriteLine("Exception in RecycleBinDialog --> RecyclebinWindow_PrimaryButtonClick:" + "\n" + ex.Message);
            }
        }
        private async void RecyclebinWindow_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (listview.SelectedItems.Count == 0)
                return;
            try
            {
                args.Cancel = true;
                while (listview.SelectedItems.Count > 0)
                {
                    var selecteditem = listview.SelectedItems[listview.SelectedItems.Count - 1] as RecycleBinListViewItem;
                    var res = await OpenFileHelper.DoOpen(tabView, selecteditem.file, true);
                    if (res != null)
                    {
                        await selecteditem.file.DeleteAsync();
                        listview.Items.Remove(selecteditem);
                    }
                }
            }
            catch
            {
                InfoMessages.OpenFromRecyclebinError();
            }
        }
        private void Listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count >= 1)
            {
                dialog.SecondaryButtonText = "Open";
                dialog.PrimaryButtonText = "Delete";
            }
            else
                dialog.PrimaryButtonText = dialog.SecondaryButtonText = "";
        }

        public async void UpdateListViewItems(ListView listview)
        {
            if (!Directory.Exists(DefaultValues.RecycleBinPath))
                Directory.CreateDirectory(DefaultValues.RecycleBinPath);

            listview.Items.Clear();
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(DefaultValues.RecycleBinPath);
            var files = await folder.GetFilesAsync();
            for (int i = 0; i < files.Count; i++)
            {
                listview.Items.Add(new RecycleBinListViewItem(files[i]));
            }
        }
        public static async Task<ClearRecycleBinResult> ClearRecycleBin()
        {
            try
            {
                if (!Directory.Exists(DefaultValues.RecycleBinPath))
                    Directory.CreateDirectory(DefaultValues.RecycleBinPath);

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(DefaultValues.RecycleBinPath);
                var files = await folder.GetFilesAsync();
                for (int i = 0; i < files.Count; i++)
                {
                    if (files[i] == null)
                        return ClearRecycleBinResult.NullError;
                    await files[i].DeleteAsync();
                }

                //check if all have been deleted
                var files_2 = await folder.GetFilesAsync();
                return files_2.Count == 0 ? ClearRecycleBinResult.Success : ClearRecycleBinResult.NotAllFilesDeleted;
            }
            catch (Exception ex)
            {
                InfoMessages.ClearRecyclebinError();
                Debug.WriteLine("Exception in RecycleBinDialog -> ClearRecycleBin " + ex.Message);
                return ClearRecycleBinResult.Exception;
            }
        }
        public static async Task<bool> MoveFileToRecycleBin(TabPageItem tab)
        {
            try
            {
                if (!Directory.Exists(DefaultValues.RecycleBinPath))
                    Directory.CreateDirectory(DefaultValues.RecycleBinPath);

                var folder = await StorageFolder.GetFolderFromPathAsync(DefaultValues.RecycleBinPath);
                if (folder == null)
                    return false;

                var file = await folder.CreateFileAsync(tab.DatabaseItem.FileName, CreationCollisionOption.GenerateUniqueName);
                if (file == null)
                    return false;

                await FileIO.WriteTextAsync(file, tab.textbox.GetText());

                return true;
            }
            catch (Exception ex)
            {
                InfoMessages.MoveToRecyclebinError();
                Debug.WriteLine("Exception in RecycleBinDialog -> MoveFileToRecycleBin" + ex.Message);
                return false;
            }
        }

        public static string GetSize()
        {
            if (!Directory.Exists(DefaultValues.RecycleBinPath))
                Directory.CreateDirectory(DefaultValues.RecycleBinPath);

            return SizeCalculationHelper.CalculateFolderSize(DefaultValues.RecycleBinPath);
        }
    }

    public enum ClearRecycleBinResult
    {
        Success, Exception, NotAllFilesDeleted, NullError
    }
    public class RecycleBinListViewItem : ListViewItem
    {
        public StorageFile file { get; set; }

        public RecycleBinListViewItem(StorageFile file)
        {
            this.file = file;
            this.Content = new TextBlock
            {
                Text = file.Name + "\n" + file.DateCreated.UtcDateTime.ToString()
            };
        }
    }
}