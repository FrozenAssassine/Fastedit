using Fastedit.Tab;
using System;

namespace Fastedit.Models
{
    public class TabFlyoutItem
    {
        public bool Matches(string FileName) => Tab.DatabaseItem.FileName.Contains(FileName, StringComparison.OrdinalIgnoreCase);
        public TabPageItem Tab { get; set; }
    }
}
