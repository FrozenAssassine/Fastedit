using System.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace Fastedit.Models
{
    public interface IQuickAccessWindowItem: INotifyPropertyChanged
    {
        string Command { get; set; }
        string Shortcut { get; set; }
        string InfoText { get; set; }
        object Tag { get; set; }
        Brush TextColor { get; set; }
    }
}
