using System;
using System.IO;
using Fastedit.Dialogs;
using Fastedit.Core.Tab;

namespace Fastedit.Storage;

internal class RenameFileHelper
{
    public static bool RenameFile(TabPageItem tab, string newName)
    {
        if (tab == null)
            return false;

        //File has NOT been saved or opened
        if (tab.DatabaseItem.WasNeverSaved)
        {
            tab.SetHeader(newName);
            return true;
        }

        //Nothing to rename
        if (tab.DatabaseItem.FileName == newName)
            return true;

        //Check if the file already exists
        if (Directory.Exists(Path.Combine(Path.GetDirectoryName(tab.DatabaseItem.FilePath), newName)))
        {
            InfoMessages.RenameFileAlreadyExists();
            return false;
        }


        string sourceFile = tab.DatabaseItem.FilePath;
        string destFile = Path.Combine(Path.GetDirectoryName(tab.DatabaseItem.FilePath), newName);

        if (File.Exists(sourceFile))
        {
            try 
            {
                Directory.Move(sourceFile, destFile);
                tab.SetHeader(newName);
            }
            catch (Exception ex)
            {
                InfoMessages.RenameFileException(ex);
                return false;
            }
        }
        return true;
    }
}
