using Fastedit.Dialogs;
using Fastedit.Helper;
using Fastedit.Models;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Fastedit.Core.Tab;
using Microsoft.UI.Xaml.Media;

namespace Fastedit.Controls
{
    public sealed partial class QuickAccessWindow : UserControl
    {
        public SolidColorBrush textColor { get; private set; } = null;
        List<QuickAccessWindowCustomItem> CurrentTabPages = new List<QuickAccessWindowCustomItem>();
        QuickAccessWindowSubItem currentPage = null;

        QuickAccessWindowInfoItem WordCountDisplay = new QuickAccessWindowInfoItem { Command = Multilingual.L("T_NumberOfWords") };
        QuickAccessWindowInfoItem CharacterCountDisplay = new QuickAccessWindowInfoItem { Command = Multilingual.L("T_NumberOfCharacter") };
        QuickAccessWindowInfoItem LineCountDisplay = new QuickAccessWindowInfoItem { Command = Multilingual.L("T_NumberOfLines") };
        QuickAccessWindowInfoItem EncodingDisplay = new QuickAccessWindowInfoItem { Command = Multilingual.L("T_CurrentEncoding") };
        QuickAccessWindowInfoItem FilePathDisplay = new QuickAccessWindowInfoItem { Command = Multilingual.L("T_FilePath") };
        QuickAccessWindowInfoItem FileNameDisplay = new QuickAccessWindowInfoItem { Command = Multilingual.L("T_FileName") };
        
        private bool mappingsInitialized = false;
        private readonly List<(IQuickAccessWindowItem item, string resourceKey)> itemMappings = new();

        private static readonly Dictionary<string, string> initialCommandMap = new()
        {
            { "Close Tab", "MainPage_MenuFlyoutItem_22.Text" },
            { "Syntax Highlighting", "MainPage_MenuFlyoutSubItem_2.Text" },
            { "New tab", "MainPage_MenuFlyoutItem_1.Text" },
            { "Encoding", "MainPage_MenuFlyoutItem_21.Text" },
            { "Copy", "MainPage_MenuFlyoutItem_11.Text" },
            { "Paste", "MainPage_MenuFlyoutItem_12.Text" },
            { "Cut", "MainPage_MenuFlyoutItem_10.Text" },
            { "Duplicate Line", "MainPage_MenuFlyoutItem_13.Text" },
            { "File Info", "MainPage_MenuFlyoutItem_18.Text" },
            { "Open File", "MainPage_MenuFlyoutItem_2.Text" },
            { "Recycle Bin", "MainPage_MenuFlyoutItem_6.Text" },
            { "Redo", "MainPage_MenuFlyoutItem_9.Text" },
            { "Undo", "MainPage_MenuFlyoutItem_8.Text" },
            { "Zoom In", "MainPage_MenuFlyoutItem_35.Text" },
            { "Zoom Out", "MainPage_MenuFlyoutItem_36.Text" },
            { "Save As", "MainPage_MenuFlyoutItem_4.Text" },
            { "Save", "MainPage_MenuFlyoutItem_3.Text" },
            { "Select All", "MainPage_MenuFlyoutItem_17.Text" },
            { "Rename", "MainPage_MenuFlyoutItem_19.Text" },
            { "Tab Key Behaviour (Tabs/Spaces)", "MainPage_MenuFlyoutSubItem_3.Text" },
            { "Reformat Tabs/Spaces", "MainPage_MenuFlyoutSubItem_4.Text" },
            { "Line Ending", "MainPage_MenuFlyoutSubItem_1.Text" },
            { "Settings", "MainPage_MenuFlyoutItem_7.Text" },
            { "Fullscreen", "MainPage_MenuFlyoutItem_33.Text" },
            { "Compact Overlay", "MainPage_MenuFlyoutItem_32.Text" },
            { "Always On Top", "MainPage_MenuFlyoutItem_34.Text" },
            { "Search", "MainPage_MenuFlyoutItem_14.Text" },
            { "Replace", "MainPage_MenuFlyoutItem_15.Text" },
            { "Undock Tab", "MainPage_MenuFlyoutItem_24.Text" },
            { "Save All", "MainPage_MenuFlyoutItem_5.Text" },
            { "Close All", "MainPage_MenuFlyoutItem_23.Text" },
            { "Surround With", "MainPage_MenuFlyoutItem_30.Text" },
            { "Toggle Readonly", "MainPage_ToggleMenuFlyoutItem_2.Text" },
            { "Word wrap", "MainPage_ToggleMenuFlyoutItem_1.Text" },
            { "Design", "SettingsPage_NavigationViewItem_4.Content" },
        };

        private void EnsureItemMappings()
        {
            if (mappingsInitialized)
                return;

            mappingsInitialized = true;
            RegisterMappingsRecursive(Items);
        }

        private void RegisterMappingsRecursive(IEnumerable<IQuickAccessWindowItem> items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                if (item == null)
                    continue;

                if (!string.IsNullOrEmpty(item.Command) && initialCommandMap.TryGetValue(item.Command, out var key))
                {
                    itemMappings.Add((item, key));
                }
                else if (item.Command == "Tabs")
                {
                    itemMappings.Add((item, "MainPage_ToggleMenuFlyoutItem_4.Text"));
                }

                if (item is QuickAccessWindowSubItem sub)
                {
                    RegisterMappingsRecursive(sub.Items);
                }
            }
        }

        public void ReloadLanguage()
        {
            EnsureItemMappings();

            foreach (var (item, key) in itemMappings)
            {
                var localized = Multilingual.L(key);
                if (!string.IsNullOrEmpty(localized) && localized != key)
                {
                    item.Command = localized;
                }
            }

            WordCountDisplay.Command = Multilingual.L("T_NumberOfWords");
            CharacterCountDisplay.Command = Multilingual.L("T_NumberOfCharacter");
            LineCountDisplay.Command = Multilingual.L("T_NumberOfLines");
            EncodingDisplay.Command = Multilingual.L("T_CurrentEncoding");
            FilePathDisplay.Command = Multilingual.L("T_FilePath");
            FileNameDisplay.Command = Multilingual.L("T_FileName");

            foreach (var page in CurrentTabPages)
            {
                page.Shortcut = Multilingual.L("T_OpenDocument");
            }
        }

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
            textColor = DialogHelper.ContentDialogForeground();
            grid.Background = DialogHelper.ContentDialogBackground();
            foreach (var item in items)
            {
                if (item is QuickAccessWindowSubItem sub_item)
                {
                    UpdateColors(sub_item.Items);
                }
                item.TextColor = textColor;
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
            ReloadLanguage();
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
                        CurrentTabPages[i].Shortcut = Multilingual.L("T_OpenDocument");
                    }
                    else
                    {
                        CurrentTabPages.Add(
                            new QuickAccessWindowCustomItem
                            {
                                Command = tab.DatabaseItem.FileName,
                                Tag = tab,
                                Shortcut = Multilingual.L("T_OpenDocument")
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
                subItem.CallItemSelectedEvent();
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
