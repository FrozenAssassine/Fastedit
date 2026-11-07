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
        private string _Command;
        public string Command { get => _Command; set { _Command = value; CallPropertyChanged("Command"); } }
        private string _Shortcut;
        public string Shortcut { get => _Shortcut; set { _Shortcut = value; CallPropertyChanged("Shortcut"); } }
        private Brush _TextColor;
        public Brush TextColor { get => _TextColor; set { _TextColor = value; CallPropertyChanged("TextColor"); } }
        private string _InfoText = null;
        public string InfoText { get => _InfoText; set { _InfoText = value; CallPropertyChanged("InfoText"); } }
    }
}
