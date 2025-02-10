using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace Fastedit.Models
{
    public class QuickAccessWindowSubItem : IQuickAccessWindowItem
    {
        public List<IQuickAccessWindowItem> Items { get; set; } = new List<IQuickAccessWindowItem>();
        public string Command { get; set; }
        public string Shortcut { get; set; }
        public object Tag { get; set; }
        private Brush _TextColor;
        public Brush TextColor { get => _TextColor; set { _TextColor = value; CallPropertyChanged("TextColor"); } }
        public string InfoText { get; set; } = null;

        public bool TriggerOnSelecting { get; set; } = false;

        public delegate void SelectedChangedEvent(IQuickAccessWindowItem item);
        public event SelectedChangedEvent SelectedChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public void CallPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void CallChangedEvent(IQuickAccessWindowItem item)
        {
            SelectedChanged?.Invoke(item);
        }
    }
}
