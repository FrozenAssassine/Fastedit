using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Fastedit.Models
{
    public interface IQuickAccessWindowItem
    {
        string Command { get; set; }
        string Shortcut { get; set; }
        string InfoText { get; set; }
        object Tag { get; set; }
        Brush TextColor { get; set; }
    }
}
