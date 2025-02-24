using System.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace Fastedit.Models
{
    public class QuickAccessWindowCustomItem : IQuickAccessWindowItem
    {
        public string Command { get; set; }
        public string Shortcut { get; set; }
        public object Tag { get; set; }
        public Brush _TextColor;

        public event PropertyChangedEventHandler PropertyChanged;

        public Brush TextColor { get => _TextColor; set { _TextColor = value; CallPropertyChanged("TextColor"); } }

        public string InfoText { get; set; } = null;

        public void CallPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
