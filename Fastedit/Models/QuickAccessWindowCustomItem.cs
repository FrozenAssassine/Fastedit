using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Fastedit.Models
{
    public class QuickAccessWindowCustomItem : IQuickAccessWindowItem
    {
        public string Command { get; set; }
        public string Shortcut { get; set; }
        public object Tag { get; set; }
        public Brush TextColor { get; set; }
        public string InfoText { get; set; } = null;
    }
}
