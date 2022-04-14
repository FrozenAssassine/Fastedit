using Fastedit.Core;
using Fastedit.Core.Tab;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
    public class RecyclebinWindow
    {
        ListView listview = null;
        RecycleBinListViewItem selecteditem = null;
        TabActions tabactions = null;
        ContentDialog dialog = null;

        public async Task ShowDialog()
        {
            if (dialog != null)
            {
                await dialog.ShowAsync();
            }
        }

        public RecyclebinWindow(TabActions tabactions)
        {
            this.tabactions = tabactions;

            listview = new ListView()
            {
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch
            };
            dialog = new ContentDialog()
            {
                CornerRadius = DefaultValues.DefaultDialogCornerRadius,
                Background = DefaultValues.ContentDialogBackgroundColor(),
                Foreground = DefaultValues.ContentDialogForegroundColor(),
                RequestedTheme = DefaultValues.ContentDialogTheme(),
                Title = "Recycle bin",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Delete",
                Content = listview
            };
            listview.SelectionChanged += Listview_SelectionChanged;
            dialog.PrimaryButtonClick += RecyclebinWindow_PrimaryButtonClick;
            dialog.SecondaryButtonClick += RecyclebinWindow_SecondaryButtonClick;
            UpdateListViewItems(listview);
        }

        private async void RecyclebinWindow_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (selecteditem != null)
            {
                args.Cancel = true;
                try
                {
                    await selecteditem.file.DeleteAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception in RecyclebinWindow --> RecyclebinWindow_PrimaryButtonClick:" + "\n" + ex.Message);
                }

                UpdateListViewItems(listview);
            }
        }
        private async void RecyclebinWindow_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (selecteditem != null)
            {
                args.Cancel = true;

                if (await tabactions.OpenFileFromRecylceBin(selecteditem.file, selecteditem.Filename) == true)
                {
                    await selecteditem.file.DeleteAsync();
                }
                UpdateListViewItems(listview);
            }
        }
        private void Listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count >= 1)
            {
                if (e.AddedItems[0] is RecycleBinListViewItem listboxitem)
                {
                    selecteditem = listboxitem;
                    if (dialog != null)
                    {
                        dialog.SecondaryButtonText = "Open";
                        dialog.PrimaryButtonText = "Delete";
                    }
                }
            }
            else
            {
                dialog.SecondaryButtonText = "";
                dialog.PrimaryButtonText = "";
            }
        }

        public async void UpdateListViewItems(ListView listview)
        {
            listview.Items.Clear();
            StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(DefaultValues.RecycleBin_FolderName, CreationCollisionOption.OpenIfExists);
            var files = await folder.GetFilesAsync();
            for (int i = 0; i< files.Count; i++)
            {
                listview.Items.Add(new RecycleBinListViewItem(files[i], files[i].Name, files[i].DateCreated.UtcDateTime.ToString()));
            }
        }
        public static async Task<ClearRecycleBinResult> ClearRecycleBin()
        {
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(DefaultValues.RecycleBin_FolderName, CreationCollisionOption.OpenIfExists);
                if (folder == null) 
                    return ClearRecycleBinResult.NullError;

                var files = await folder.GetFilesAsync();
                for (int i = 0; i < files.Count; i++)
                {
                    if (files[i] == null) 
                        return ClearRecycleBinResult.NullError;
                    await files[i].DeleteAsync();
                }

                //check if all have been deleted
                var files_2 = await folder.GetFilesAsync();
                if (files_2.Count == 0)
                    return ClearRecycleBinResult.Success;
                else
                    return ClearRecycleBinResult.NotAllFilesDeleted;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in RecyclebinWindow -> ClearRecycleBin" + ex.Message);
                return ClearRecycleBinResult.Exception;
            }
        }
    }

    public enum ClearRecycleBinResult
    {
        Success, Exception, NotAllFilesDeleted, NullError
    }
    public class RecycleBinListViewItem : ListViewItem
    {
        public string Filename;
        public StorageFile file;
        public string DeletionTime;

        public RecycleBinListViewItem(StorageFile file, string Filename, string DeletionTime)
        {
            var MainStackPanel = new StackPanel();
            MainStackPanel.Children.Add(new TextBlock { Text = Filename, FontSize = 20 });
            MainStackPanel.Children.Add(new TextBlock { Text = DeletionTime, Margin = new Windows.UI.Xaml.Thickness(0, 5, 0, 0) });
            Content = new ScrollViewer { Content = MainStackPanel };
            this.Filename = Filename;
            this.file = file;
            this.DeletionTime = DeletionTime;
        }
    }
}
