using Fastedit.Dialogs;
using Fastedit.Helper;
using Fastedit.Models;
using Fastedit.Tab;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

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
            //Add the current tabpages:
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
            //Remove all tabs
            Items.RemoveAll(x => x.Tag is TabPageItem);

            currentPage = null;
            hideControlAnimation.Begin();
        }
        public List<IQuickAccessWindowItem> Items { get; set; } = new List<IQuickAccessWindowItem>();

        private void UpdateLiveCommands(TabView tabView)
        {
            if (tabView.SelectedItem == null)
                return;

            if (tabView.SelectedItem is TabPageItem selectedTab)
            {
                WordCountDisplay.InfoText = selectedTab.CountWords().ToString();
                CharacterCountDisplay.InfoText = selectedTab.textbox.CharacterCount.ToString();
                LineCountDisplay.InfoText = selectedTab.textbox.NumberOfLines.ToString();
                EncodingDisplay.InfoText = EncodingHelper.GetEncodingName(selectedTab.Encoding);
                FilePathDisplay.InfoText = selectedTab.DatabaseItem.FilePath;
                FileNameDisplay.InfoText = selectedTab.DatabaseItem.FileName;
            }
        }
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
                item.InvokeEvent();
                Hide();
            }
            else if (clickedItem is QuickAccessWindowSubItem subItem)
            {
                //change the source -> like switching to sub page:
                currentPage = subItem;
                searchbox.Text = "";
                itemHostListView.ItemsSource = subItem.Items;
                itemHostListView.SelectedIndex = 1;
            }
            else if (clickedItem is QuickAccessWindowCustomItem customItem)
            {
                //select a tabpage
                if (customItem.Tag is TabPageItem tab)
                {
                    TabPageHelper.mainPage.ChangeSelectedTab(tab);
                    Hide();
                }
            }
            else if (clickedItem is QuickAccessWindowInfoItem infoitem)
            {
                //Copy to clipbard
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(infoitem.InfoText);
                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                Clipboard.SetContent(dataPackage);

                Hide();
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
                ItemClicked(itemHostListView.SelectedItem ?? itemHostListView.Items[0]);
            }
        }
        private void hideControlAnimation_Completed(object sender, object e)
        {
            this.Visibility = Visibility.Collapsed;
        }
   
    }
}
