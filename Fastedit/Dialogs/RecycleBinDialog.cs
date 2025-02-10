using Fastedit.Helper;
using Fastedit.Settings;
using Fastedit.Storage;
using Fastedit.Tab;
using Fastedit.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

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
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch,
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
                XamlRoot = App.m_window.Content.XamlRoot
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
        private void RecyclebinWindow_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (listview.SelectedItems.Count == 0)
                return;

            args.Cancel = true;

            try
            {
                while (listview.SelectedItems.Count > 0)
                {
                    var selecteditem = listview.SelectedItems[listview.SelectedItems.Count - 1] as RecycleBinListViewItem;
                    File.Delete(selecteditem.filePath);
                    listview.Items.Remove(selecteditem);
                }
            }
            catch (Exception ex)
            {
                InfoMessages.DeleteFromRecyclebinError();
                Debug.WriteLine("Exception in RecycleBinDialog --> RecyclebinWindow_PrimaryButtonClick:" + "\n" + ex.Message);
            }
        }
        private void RecyclebinWindow_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (listview.SelectedItems.Count == 0)
                return;
            try
            {
                args.Cancel = true;
                while (listview.SelectedItems.Count > 0)
                {
                    var selecteditem = listview.SelectedItems[listview.SelectedItems.Count - 1] as RecycleBinListViewItem;
                    var res = OpenFileHelper.DoOpen(tabView, selecteditem.filePath, true);
                    if (res != null)
                    {
                        File.Delete(selecteditem.filePath);
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

        private static bool CheckAndCreateDirectory()
        {
            if (!Directory.Exists(DefaultValues.RecycleBinPath))
            {
                Directory.CreateDirectory(DefaultValues.RecycleBinPath);
                return false;
            }
            return true;
        }

        public void UpdateListViewItems(ListView listview)
        {
            //no need to render any items if the folder was just created
            if (!CheckAndCreateDirectory())
                return;

            listview.Items.Clear();

            foreach (var file in GetTrashedFiles())
            {
                listview.Items.Add(new RecycleBinListViewItem(file));
            }
        }
        private static string[] GetTrashedFiles()
        {
            return Directory.GetFiles(DefaultValues.RecycleBinPath);
        }

        public static ClearRecycleBinResult ClearRecycleBin()
        {
            try {
                if (Directory.Exists(DefaultValues.RecycleBinPath))
                    Directory.Delete(DefaultValues.RecycleBinPath, true);
            }
            catch
            {
                InfoMessages.ClearRecyclebinError();
                return ClearRecycleBinResult.Exception;
            }

            CheckAndCreateDirectory();
            return ClearRecycleBinResult.Success;
        }
        public static bool MoveFileToRecycleBin(TabPageItem tab)
        {
            CheckAndCreateDirectory();

            try
            {
                string fileName = SaveFileHelper.GenerateUniqueNameFromPath(Path.Join(DefaultValues.RecycleBinPath, tab.DatabaseItem.FileName));
                File.WriteAllLines(Path.Join(DefaultValues.RecycleBinPath, fileName), tab.textbox.Lines);
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
            CheckAndCreateDirectory();

            return SizeCalculationHelper.CalculateFolderSize(DefaultValues.RecycleBinPath);
        }
    }
}