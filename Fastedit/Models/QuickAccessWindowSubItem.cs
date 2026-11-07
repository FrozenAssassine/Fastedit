using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace Fastedit.Models
{
    public class QuickAccessWindowSubItem : IQuickAccessWindowItem
    {
        public List<IQuickAccessWindowItem> Items { get; set; } = new List<IQuickAccessWindowItem>();
        private string _Command;
        public string Command { get => _Command; set { _Command = value; CallPropertyChanged("Command"); } }
        private string _Shortcut;
        public string Shortcut { get => _Shortcut; set { _Shortcut = value; CallPropertyChanged("Shortcut"); } }
        public object Tag { get; set; }
        private Brush _TextColor;
        public Brush TextColor { get => _TextColor; set { _TextColor = value; CallPropertyChanged("TextColor"); } }
        public string InfoText { get; set; } = null;

        public bool TriggerOnSelecting { get; set; } = false;

        public delegate void SelectedChangedEvent(IQuickAccessWindowItem item);
        public event SelectedChangedEvent SelectedChanged;

        //occures, if this item gets selected.
        //use to load data like designs or syntax highlights dynamically
        public delegate void ItemSelectedEvent();
        public event ItemSelectedEvent ItemSelected;
        
        public event PropertyChangedEventHandler PropertyChanged;

        public void CallPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void CallChangedEvent(IQuickAccessWindowItem item)
        {
            SelectedChanged?.Invoke(item);
        }

        public void CallItemSelectedEvent()
        {
            this.ItemSelected?.Invoke();
        }
    }
}
