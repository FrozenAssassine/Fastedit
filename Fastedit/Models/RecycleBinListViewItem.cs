using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Models
{
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
