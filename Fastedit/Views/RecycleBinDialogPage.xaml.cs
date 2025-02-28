using Fastedit.Core;
using Fastedit.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace Fastedit.Views;

public sealed partial class RecycleBinDialogPage : Page
{
    public ListView itemList => itemListView;
    private TabView tabView;
    private ObservableCollection<RecycleBinItem> recycleBinItems;

    public RecycleBinDialogPage(TabView tabView)
    {
        this.InitializeComponent();
        this.tabView = tabView;

        recycleBinItems = new ObservableCollection<RecycleBinItem>(RecycleBinManager.GetTrashedFiles().Select(x => new RecycleBinItem(x)));
    }

    private void OpenSelected_Click(object sender, RoutedEventArgs e)
    {
        RecycleBinManager.ReopenSelected(itemListView, tabView, recycleBinItems);
    }

    private void DeleteSelected_Click(object sender, RoutedEventArgs e)
    {
        RecycleBinManager.DeleteSelected(itemListView, recycleBinItems);
    }

    private void ClearRecycleBin_Click(object sender, RoutedEventArgs e)
    {
        RecycleBinManager.ClearRecycleBin();
        recycleBinItems.Clear();
    }
}
