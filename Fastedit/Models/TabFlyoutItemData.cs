using Fastedit.Tab;
using Microsoft.UI.Xaml.Controls;

namespace Fastedit.Models
{
    public class TabFlyoutItemData
    {
        public TabFlyoutItemData(TabPageItem tab, TabView tabView, TabPageFlyoutItem item)
        {
            this.Item = item;
            this.Tab = tab;
            this.TabView = tabView;
        }
        public TabPageFlyoutItem Item { get; set; }
        public TabPageItem Tab { get; set; }
        public TabView TabView { get; set; }
    }
}
