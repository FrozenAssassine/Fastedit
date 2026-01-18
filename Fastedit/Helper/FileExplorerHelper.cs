using System;

namespace Fastedit.Helper;

public class FileExplorerHelper
{
    public static bool OpenExplorerAtPath(string path)
    {
        if (path == null)
            return false;

        try
        { 
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "explorer.exe";
            process.StartInfo.Arguments = $"/select,\"{path}\"";
            process.Start();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
