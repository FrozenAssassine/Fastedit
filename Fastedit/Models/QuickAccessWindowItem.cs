using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System.ComponentModel;

namespace Fastedit.Models
{
    public class QuickAccessWindowItem : IQuickAccessWindowItem
    {
        public delegate void RunCommandWindowItemClickedEvent(object sender, RoutedEventArgs e);
        public event RunCommandWindowItemClickedEvent RunCommandWindowItemClicked;
        public event PropertyChangedEventHandler PropertyChanged;

        public void InvokeEvent()
        {
            RunCommandWindowItemClicked?.Invoke(this, null);
        }
        public void CallPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public object Tag { get; set; }
        public string Command { get; set; }
        public string Shortcut { get; set; }
        private Brush _TextColor;
        public Brush TextColor { get => _TextColor; set { _TextColor = value; CallPropertyChanged("TextColor"); } }
        public string InfoText { get; set; } = null;
    }
}
