using System;
using System.IO;

namespace Fastedit.Models;

public class RecycleBinItem
{
    public string FilePath { get; set; }
    public string FileName;
    public string CreationDate;
    
    public RecycleBinItem(string path)
    {
        this.FilePath = path;
        this.FileName = Path.GetFileName(FilePath);
        try
        {
            this.CreationDate = new FileInfo(FilePath).CreationTime.ToString("HH:mm dd.MM.yyyy");
        }
        catch(Exception)
        {
            this.CreationDate = "-";
        }
    }
}
