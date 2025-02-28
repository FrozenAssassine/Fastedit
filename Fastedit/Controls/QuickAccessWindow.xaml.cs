using Fastedit.Dialogs;
using Fastedit.Helper;
using Fastedit.Models;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Fastedit.Core.Tab;

namespace Fastedit.Controls
{
    public sealed partial class QuickAccessWindow : UserControl
    {
        List<QuickAccessWindowCustomItem> CurrentTabPages = new List<QuickAccessWindowCustomItem>();
        QuickAccessWindowSubItem currentPage = null;

        QuickAccessWindowInfoItem WordCountDisplay = new QuickAccessWindowInfoItem { Command = "Number of Words" };
        QuickAccessWindowInfoItem CharacterCountDisplay = new QuickAccessWindowInfoItem { Command = "Number of Character" };
        QuickAccessWindowInfoItem LineCountDisplay = new QuickAccessWindowInfoItem { Command = "Number of Lines" };
        QuickAccessWindowInfoItem EncodingDisplay = new QuickAccessWindowInfoItem { Command = "Current Encoding" };
        QuickAccessWindowInfoItem FilePathDisplay = new QuickAccessWindowInfoItem { Command = "File Path" };
        QuickAccessWindowInfoItem FileNameDisplay = new QuickAccessWindowInfoItem { Command = "File Name" };

        public QuickAccessWindow()
        {
            this.InitializeComponent();

            Items.Add(WordCountDisplay);
            Items.Add(CharacterCountDisplay);
            Items.Add(LineCountDisplay);
            Items.Add(EncodingDisplay);
            Items.Add(FilePathDisplay);
            Items.Add(FileNameDisplay);
        }

        public delegate void ClosedEvent();
        public event ClosedEvent Closed;
        
        public void UpdateColors()
        {
            UpdateColors(Items);
        }
        public void UpdateColors(List<IQuickAccessWindowItem> items)
        {
            var textcolor = DialogHelper.ContentDialogForeground();
            grid.Background = DialogHelper.ContentDialogBackground();
            foreach (var item in items)
            {
                if (item is QuickAccessWindowSubItem sub_item)
                {
                    UpdateColors(sub_item.Items);
                }
                item.TextColor = textcolor;
            }
            grid.RequestedTheme = DialogHelper.DialogDesign;
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
            AddCurrentTabPages(tabView);
            UpdateLiveCommands(tabView);
            Items.AddRange(CurrentTabPages);

            if (itemHostListView == null)
            {
                itemHostListView = FindName("itemHostListView") as ListView;
            }
            UpdateColors();

            searchbox.Text = "";
            this.Visibility = Visibility.Visible;
            showControlAnimation.Begin();
            searchbox.Focus(FocusState.Programmatic);
            searchbox_TextChanged(null, null);
        }
        public void Hide()
        {
            Items.RemoveAll(x => x.Tag is TabPageItem);

            currentPage = null;
            hideControlAnimation.Begin();
            Closed?.Invoke();
        }
        public List<IQuickAccessWindowItem> Items { get; set; } = new List<IQuickAccessWindowItem>();

        private void UpdateLiveCommands(TabView tabView)
        {
            if (tabView.SelectedItem == null)
                return;

            if (tabView.SelectedItem is TabPageItem selectedTab)
            {
                WordCountDisplay.InfoText = selectedTab.textbox.WordCount().ToString();
                CharacterCountDisplay.InfoText = selectedTab.textbox.CharacterCount().ToString();
                LineCountDisplay.InfoText = selectedTab.textbox.NumberOfLines.ToString();
                EncodingDisplay.InfoText = EncodingHelper.GetEncodingName(selectedTab.Encoding);
                FilePathDisplay.InfoText = selectedTab.DatabaseItem.FilePath;
                FileNameDisplay.InfoText = selectedTab.DatabaseItem.FileName;
            }
        }
        private void AddCurrentTabPages(TabView tabView)
        {
            //remove if there are too many tabs
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
                            new QuickAccessWindowCustomItem
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
            if (clickedItem is QuickAccessWindowItem item)
            {
                Hide();
                item.InvokeEvent();
            }
            else if (clickedItem is QuickAccessWindowSubItem subItem)
            {
                //change the source -> like switching to sub page:
                currentPage = subItem;
                searchbox.Text = "";
                itemHostListView.ItemsSource = subItem.Items;
                itemHostListView.LayoutUpdated += (sender, e) =>
                {
                    if(itemHostListView.SelectedItem == null)
                        itemHostListView.SelectedIndex = 0;
                };
            }
            else if (clickedItem is QuickAccessWindowCustomItem customItem)
            {
                //select a tabpage
                if (customItem.Tag is TabPageItem tab)
                {
                    Hide();
                    TabPageHelper.mainPage.ChangeSelectedTab(tab);
                }
            }
            else if (clickedItem is QuickAccessWindowInfoItem infoitem)
            {
                Hide();
                ClipboardHelper.Copy(infoitem.InfoText);
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

                    //for live changes like desing page
                    if (currentPage != null && currentPage.TriggerOnSelecting && itemHostListView.SelectedIndex != -1)
                        currentPage.CallChangedEvent(itemHostListView.SelectedItem as IQuickAccessWindowItem);
                }

                if (itemHostListView.SelectedItem == null)
                    return;

                itemHostListView.ScrollIntoView(itemHostListView.Items[itemHostListView.SelectedIndex]);
            }
            else if (e.Key == Windows.System.VirtualKey.Up)
            {
                if (itemHostListView.SelectedIndex > 0)
                {
                    itemHostListView.SelectedIndex--;
                    itemHostListView.ScrollIntoView(itemHostListView.Items[itemHostListView.SelectedIndex]);

                    //for live changes like desing page
                    if (currentPage != null && currentPage.TriggerOnSelecting && itemHostListView.SelectedIndex != -1)
                        currentPage.CallChangedEvent(itemHostListView.SelectedItem as IQuickAccessWindowItem);
                }
            }
            else if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (itemHostListView.SelectedItem == null)
                    return;

                ItemClicked(itemHostListView.SelectedItem);
            }
        }
        private void hideControlAnimation_Completed(object sender, object e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
