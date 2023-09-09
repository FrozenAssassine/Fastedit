using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using Windows.Storage;
using Fastedit.Tab;
using Fastedit.Dialogs;
using Fastedit.Helper;

namespace Fastedit.Storage
{
    internal class RenameFileHelper
    {
        public static async Task<bool> RenameFile(TabPageItem tab, string newName)
        {
            if (tab == null)
                return false;

            //File has NOT been saved or opened
            if (tab.DatabaseItem.FileToken.Length <= 0)
            {
                tab.SetHeader(newName);
                return true;
            }

            var getFileRes = await FutureAccessListHelper.GetFileAsync(tab.DatabaseItem.FileToken);
            if (!getFileRes.success)
                return false;

            //Nothing to rename
            if (getFileRes.file.Name == newName)
                return true;

            //Check if the file already exists
            if (Directory.Exists(Path.Combine(Path.GetDirectoryName(getFileRes.file.Path), newName)))
            {
                InfoMessages.RenameFileError();
                return false;
            }

            try
            {
                await getFileRes.file.RenameAsync(newName, NameCollisionOption.FailIfExists);
                tab.DatabaseItem.FileToken = StorageApplicationPermissions.FutureAccessList.Add(getFileRes.file);
            }
            catch (Exception ex)
            {
                InfoMessages.RenameFileException(ex);
                return false;
            }

            tab.SetHeader(newName);
            return true;
        }
    }
}
