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

namespace Fastedit.Storage
{
    internal class RenameFileHelper
    {
        public static async Task<bool> RenameFile(TabPageItem tab, string newName)
        {
            if (tab == null)
                return false;

            //File has been saved or opened
            if (tab.DatabaseItem.FileToken.Length > 0)
            {
                var currentFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(tab.DatabaseItem.FileToken);
                if (currentFile == null)
                    return false;

                //Nothing to rename
                if (currentFile.Name == newName)
                    return true;

                //Check if the file already exists
                if(Directory.Exists(Path.Combine(Path.GetDirectoryName(currentFile.Path), newName)))
                {
                    InfoMessages.RenameFileError();
                    return false;
                }

                try
                {
                    await currentFile.RenameAsync(newName, NameCollisionOption.FailIfExists);
                    tab.DatabaseItem.FileToken = StorageApplicationPermissions.FutureAccessList.Add(currentFile);
                }
                catch (Exception ex)
                {
                    InfoMessages.RenameFileException(ex);
                    return false;
                }
            }
            tab.SetHeader(newName);
            return true;
        }
    }
}
