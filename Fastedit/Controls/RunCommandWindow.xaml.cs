using Fastedit.Tab;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Fastedit.Controls
{
    public sealed partial class RunCommandWindow : UserControl
    {
        List<RunCommandWindowCustomItem> CurrentTabPages = new List<RunCommandWindowCustomItem>();
        RunCommandWindowSubItem currentPage = null;

        public RunCommandWindow()
        {
            this.InitializeComponent();
        }

        public void Toggle(TabView tabView)
        {
            if (this.Visibility == Visibility.Visible)
                Hide();
            else
                Show(tabView);
        }
        public void Show(TabView tabView)
        {
            //Add the current tabpages:
            AddCurrentTabPages(tabView);
            Items.AddRange(CurrentTabPages);

            if (itemHostListView == null)
            {
                itemHostListView = FindName("itemHostListView") as ListView;
            }

            searchbox.Text = "";
            this.Visibility = Visibility.Visible;
            showControlAnimation.Begin();
            searchbox.Focus(FocusState.Programmatic);
            searchbox_TextChanged(null, null);
        }
        public void Hide()
        {
            //Remove all tabs
            Items.RemoveAll(x => x.Tag is TabPageItem);

            currentPage = null;
            hideControlAnimation.Begin();
        }
        public List<IRunCommandWindowItem> Items { get; set; } = new List<IRunCommandWindowItem>();

        private void AddCurrentTabPages(TabView tabView)
        {
            //remove when there are too many
            if (CurrentTabPages.Count > tabView.TabItems.Count)
            {
                CurrentTabPages.RemoveRange(0, CurrentTabPages.Count - tabView.TabItems.Count);
            }

            //overwrite or add new ones:
            for (int i = 0; i < tabView.TabItems.Count; i++)
            {
                if (tabView.TabItems[i] is TabPageItem tab)
                {
                    if (i < CurrentTabPages.Count)
                    {
                        CurrentTabPages[i].Command = tab.DatabaseItem.FileName;
                        CurrentTabPages[i].Tag = tab;
                        CurrentTabPages[i].Shortcut = "Open document";
                    }
                    else
                    {
                        CurrentTabPages.Add(
                            new RunCommandWindowCustomItem
                            {
                                Command = tab.DatabaseItem.FileName,
                                Tag = tab,
                                Shortcut = "Open document"
                            });
                    }
                }
            }
        }
        private void searchbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (currentPage != null)
            {
                var source = currentPage.Items.Where(x => x.Command.ToLower().Contains(searchbox.Text.ToLower()));
                itemHostListView.ItemsSource = source.OrderBy(x => x.Command);
                return;
            }

            var newsource = Items.Where(x => x.Command.ToLower().Contains(searchbox.Text.ToLower()));

            itemHostListView.ItemsSource = newsource.OrderBy(x => x.Command);
            itemHostListView.SelectedIndex = 0;
        }
        private void itemHostListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ItemClicked(e.ClickedItem);
        }
        private void ItemClicked(object clickedItem)
        {
            if (clickedItem is RunCommandWindowItem item)
            {
                item.InvokeEvent();
                Hide();
            }
            else if (clickedItem is RunCommandWindowSubItem subItem)
            {
                //change the source -> like switching to sub page:
                currentPage = subItem;
                searchbox.Text = "";
                itemHostListView.ItemsSource = subItem.Items;
                itemHostListView.SelectedIndex = 1;
            }
            else if (clickedItem is RunCommandWindowCustomItem customItem)
            {
                //select a tabpage
                if (customItem.Tag is TabPageItem tab)
                {
                    TabPageHelper.mainPage.ChangeSelectedTab(tab);
                    Hide();
                }
            }
        }
        private void UserControl_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                if (currentPage != null)
                {
                    currentPage = null;
                    searchbox.Text = "";
                    itemHostListView.ItemsSource = Items;
                    return;
                }
                Hide();
            }
            else if (e.Key == Windows.System.VirtualKey.Down)
            {
                if (itemHostListView.SelectedIndex < itemHostListView.Items.Count - 1)
                {
                    itemHostListView.SelectedIndex++;
                    itemHostListView.ScrollIntoView(itemHostListView.Items[itemHostListView.SelectedIndex]);
                }

                itemHostListView.ScrollIntoView(itemHostListView.Items[itemHostListView.SelectedIndex]);
            }
            else if (e.Key == Windows.System.VirtualKey.Up)
            {
                if (itemHostListView.SelectedIndex > 0)
                {
                    itemHostListView.SelectedIndex--;
                    itemHostListView.ScrollIntoView(itemHostListView.Items[itemHostListView.SelectedIndex]);
                }

            }
            else if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ItemClicked(itemHostListView.SelectedItem);
            }
        }
        private void hideControlAnimation_Completed(object sender, object e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
    public class RunCommandWindowItem : IRunCommandWindowItem
    {
        public delegate void RunCommandWindowItemClickedEvent(object sender, RoutedEventArgs e);
        public event RunCommandWindowItemClickedEvent RunCommandWindowItemClicked;
        public void InvokeEvent()
        {
            RunCommandWindowItemClicked?.Invoke(this, null);
        }

        public object Tag { get; set; }
        public string Command { get; set; }
        public string Shortcut { get; set; }
    }

    public class RunCommandWindowSubItem : IRunCommandWindowItem
    {
        public List<RunCommandWindowItem> Items { get; set; } = new List<RunCommandWindowItem>();
        public string Command { get; set; }
        public string Shortcut { get; set; }
        public object Tag { get; set; }
    }

    public class RunCommandWindowCustomItem : IRunCommandWindowItem
    {
        public string Command { get; set; }
        public string Shortcut { get; set; }
        public object Tag { get; set; }
    }

    public interface IRunCommandWindowItem
    {
        string Command { get; set; }
        string Shortcut { get; set; }
        object Tag { get; set; }
    }
}
