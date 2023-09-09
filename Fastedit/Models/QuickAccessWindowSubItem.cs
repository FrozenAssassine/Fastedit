using System.Collections.Generic;
using Windows.UI.Xaml.Media;

namespace Fastedit.Models
{
    public class QuickAccessWindowSubItem : IQuickAccessWindowItem
    {
        public List<IQuickAccessWindowItem> Items { get; set; } = new List<IQuickAccessWindowItem>();
        public string Command { get; set; }
        public string Shortcut { get; set; }
        public object Tag { get; set; }
        public Brush TextColor { get; set; }
        public string InfoText { get; set; } = null;
    }
}
