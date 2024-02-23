using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Tab
{
    internal class SplitTabView : TabViewItem
    {
        private TabPageItem Tab1;
        private TabPageItem Tab2;
        private Grid splittedGrid;
        public TabPageItem SelectedTab { get; private set; }

        public SplitTabView(TabPageItem tab1, TabPageItem tab2)
        {
            this.Tab1 = tab1;
            this.Tab2 = tab2;
        }

        public void Close(TabView tabView)
        {
            tabView.TabItems.Remove(this);

            splittedGrid.Children.Clear();

            Tab1.AddTextbox();
            Tab2.AddTextbox();

            Tab1.textbox.GotFocus -= Textbox_GotFocus;
            Tab2.textbox.GotFocus -= Textbox_GotFocus;

            tabView.TabItems.Add(Tab1);
            tabView.TabItems.Add(Tab2);
        }

        public void Open(TabView tabView)
        {
            Tab1.RemoveTextbox();
            Tab2.RemoveTextbox();

            Tab1.textbox.GotFocus += Textbox_GotFocus;
            Tab2.textbox.GotFocus += Textbox_GotFocus;

            splittedGrid = new Grid();
            splittedGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            splittedGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            splittedGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            var gridSplitter = new GridSplitter
            {
                Width = 16,
                ResizeBehavior = GridSplitter.GridResizeBehavior.BasedOnAlignment,
                ResizeDirection = GridSplitter.GridResizeDirection.Columns,
            };

            Grid.SetColumn(Tab1.textbox, 0);
            Grid.SetColumn(gridSplitter, 1);
            Grid.SetColumn(Tab2.textbox, 2);

            splittedGrid.Children.Add(Tab1.textbox);
            splittedGrid.Children.Add(Tab2.textbox);
            splittedGrid.Children.Add(gridSplitter);

            splittedGrid.Margin = new Thickness(0, 40, 0, 0);

            this.Content = splittedGrid;
            this.Header = Tab1.Header + " - " + Tab2.Header;

            tabView.TabItems.Remove(Tab1);
            tabView.TabItems.Remove(Tab2);

            tabView.TabItems.Add(this);
        }

        private void Textbox_GotFocus(TextControlBox.TextControlBox sender)
        {
            SelectedTab = sender == Tab1.textbox ? Tab1 : Tab2;
        }
    }
}
