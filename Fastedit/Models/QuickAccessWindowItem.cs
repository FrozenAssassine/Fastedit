using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;

namespace Fastedit.Models
{
    public class QuickAccessWindowItem : IQuickAccessWindowItem
    {
        public delegate void RunCommandWindowItemClickedEvent(object sender, RoutedEventArgs e);
        public event RunCommandWindowItemClickedEvent RunCommandWindowItemClicked;
        public void InvokeEvent()
        {
            RunCommandWindowItemClicked?.Invoke(this, null);
        }

        public object Tag { get; set; }
        public string Command { get; set; }
        public string Shortcut { get; set; }
        public Brush TextColor { get; set; }
        public string InfoText { get; set; } = null;
    }
}
