using Microsoft.UI.Xaml.Controls;
using System.IO;

namespace Fastedit.Models
{
    public class RecycleBinListViewItem : ListViewItem
    {
        public string filePath { get; set; }
        public RecycleBinListViewItem(string file)
        {
            this.filePath = file;

            var dateCreated = new FileInfo(filePath).CreationTime;

            this.Content = new TextBlock
            {
                Text = Path.GetFileName(file) + "\n" + dateCreated.ToString("HH:mm dd.MM.yyyy")
            };
        }
    }
}
