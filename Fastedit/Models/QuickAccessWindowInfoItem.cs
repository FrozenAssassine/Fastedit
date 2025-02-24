using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;

namespace Fastedit.Models
{
    public class QuickAccessWindowInfoItem : IQuickAccessWindowItem
    {
        public string Command { get; set; }
        public string Shortcut { get; set; }
        public string InfoText { get; set; }
        public object Tag { get; set; }
        public Brush _TextColor;
        public Brush TextColor { get => _TextColor; set { _TextColor = value; CallPropertyChanged("TextColor"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void CallPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
