using Fastedit.Core.Settings;
using Fastedit.Core.Storage;
using Fastedit.Core.Tab;
using Fastedit.Dialogs;
using Fastedit.Helper;
using Fastedit.Models;
using Fastedit.Storage;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace Fastedit.Core;

public class RecycleBinManager
{
    public static bool CheckAndCreateDirectory()
    {
        if (!Directory.Exists(DefaultValues.RecycleBinPath))
        {
            Directory.CreateDirectory(DefaultValues.RecycleBinPath);
            return false;
        }
        return true;
    }

    public static string[] GetTrashedFiles()
    {
        if (!Directory.Exists(DefaultValues.RecycleBinPath))
            return [];
        return Directory.GetFiles(DefaultValues.RecycleBinPath);
    }

    public static ClearRecycleBinResult ClearRecycleBin()
    {
        try
        {
            if (Directory.Exists(DefaultValues.RecycleBinPath))
                Directory.Delete(DefaultValues.RecycleBinPath, true);
        }
        catch
        {
            InfoMessages.ClearRecycleBinError();
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
            var filePath = Path.Join(DefaultValues.RecycleBinPath, fileName);
            File.WriteAllLines(filePath, tab.textbox.Lines);
            return true;
        }
        catch (Exception ex)
        {
            InfoMessages.MoveToRecycleBinError();
            Debug.WriteLine("Exception in RecycleBinDialog -> MoveFileToRecycleBin" + ex.Message);
            return false;
        }
    }

    public static string GetSize()
    {
        CheckAndCreateDirectory();

        return SizeCalculationHelper.CalculateFolderSize(DefaultValues.RecycleBinPath);
    }

    public static void DeleteSelected(ListView itemListView, ObservableCollection<RecycleBinItem> recycleBinItems)
    {
        if (itemListView.SelectedItems.Count == 0)
            return;

        try
        {
            while (itemListView.SelectedItems.Count > 0)
            {
                var selecteditem = itemListView.SelectedItems[itemListView.SelectedItems.Count - 1] as RecycleBinItem;
                File.Delete(selecteditem.FilePath);
                recycleBinItems.Remove(selecteditem);

                itemListView.ItemsSource = recycleBinItems;
            }
        }
        catch (Exception ex)
        {
            InfoMessages.DeleteFromRecycleBinError();
            Debug.WriteLine("Exception in RecycleBinDialog --> RecyclebinWindow_PrimaryButtonClick:" + "\n" + ex.Message);
        }
    }

    public static async void ReopenSelected(ListView itemListView, TabView tabView, ObservableCollection<RecycleBinItem> recycleBinItems)
    {
        if (itemListView.SelectedItems.Count == 0)
            return;
        try
        {
            while (itemListView.SelectedItems.Count > 0)
            {
                var selecteditem = itemListView.SelectedItems[itemListView.SelectedItems.Count - 1] as RecycleBinItem;
                var res = await OpenFileHelper.DoOpenAsync(tabView, selecteditem.FilePath, true);
                if (res != null)
                {
                    File.Delete(selecteditem.FilePath);
                    recycleBinItems.Remove(selecteditem);
                }
            }
        }
        catch
        {
            InfoMessages.OpenFromRecycleBinError();
        }
    }

}

