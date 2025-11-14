using Fastedit.Helper;
using Fastedit.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Text;
using Microsoft.UI.Xaml;
using TextControlBoxNS;
using Fastedit.Core.Settings;
using System.Diagnostics;

namespace Fastedit.Core.Tab;

public class TabPageItem : TabViewItem
{
    public TextControlBox textbox { get; private set; }
    private TabView tabView;
    private MainPage mainPage;
    
    public TabPageItem(TabView tabView, TabItemDatabaseItem databaseItem = null)
    {
        this.tabView = tabView;
        Initialise(tabView, databaseItem);

        PointerWheelChanged += TabPageItem_PointerWheelChanged;
    }

    private void TabPageItem_PointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (e.KeyModifiers != Windows.System.VirtualKeyModifiers.Shift)
            return;

        int scroll = e.GetCurrentPoint(sender as UIElement).Properties.MouseWheelDelta / 120;
        if (scroll > 0)
            TabPageHelper.SelectNextTab(tabView);
        else
            TabPageHelper.SelectPreviousTab(tabView);
    }

    //Remove the textbox from the current Grid:
    public void RemoveTextbox()
    {
        if (Content is Grid grd)
        {
            textbox.Margin = new Thickness(0, 0, 0, 0);
            grd.Children.Remove(textbox);
        }
    }
    //add the textbox back to the current Grid:
    public void AddTextbox()
    {
        if (Content is Grid grd)
        {
            grd.Children.Add(textbox);
        }
    }

    public void UpdateTabIcon()
    {
        this.IconSource = 
            new FontIconSource { 
                Glyph = this.DatabaseItem.IsModified ? "\uE915" : "\uE7C3" 
            };
    }

    private void Initialise(TabView tabView, TabItemDatabaseItem item)
    {
        textbox = TabPageHelper.CreateTextBox();
        //add a reference to the tabitem to the textbox
        textbox.Tag = this;

        var grid = new Grid();

        ContextFlyout = TabFlyout.CreateFlyout(this, tabView);
        Content = grid;
        grid.Children.Add(textbox);

        Encoding = DefaultValues.Encoding;
        DatabaseItem = item ?? new TabItemDatabaseItem
        {
            FilePath = "",
            IsModified = false,
            ZoomFactor = 100,
            LineEnding = LineEnding.CRLF,
        };

        UpdateTabIcon();
    }

    public bool DataIsLoaded = false;

    public SyntaxHighlightID HighlightLanguage
    {
        set
        {
            textbox.SyntaxHighlighting = TextControlBox.GetSyntaxHighlightingFromID(value);
            DatabaseItem.CodeLanguage = value.ToString();
        }
    }

    public bool HasHeader(string header)
    {
        if (DatabaseItem.FileName == null)
            return false;

        return DatabaseItem.FileName.Equals(header, StringComparison.Ordinal);
    }
    public void SetHeader(string header)
    {
        if (DatabaseItem.IsModified)
        {
            if (Header == null)
                Header = header ?? "";

            string currentHeader = Header.ToString();
            if (currentHeader.Length > 0)
            {
                Header = header + (currentHeader[currentHeader.Length - 1] == '*' ? "" : "*");
            }
        }
        else
            Header = header;

        UpdateTabIcon();

        DatabaseItem.FileName = header;
    }
    //Ensure the save status of the file to be displayed in the header
    public void UpdateHeader()
    {
        SetHeader(DatabaseItem.FileName);
    }

    private void ApplyDatabaseItemToTextbox()
    {
        if (DatabaseItem == null)
            return;

        LineEnding = _DataBaseItem.LineEnding;
        textbox.ZoomFactor = _DataBaseItem.ZoomFactor;
        SetHeader(_DataBaseItem.FileName);

        SyntaxHighlightID highlightID = SyntaxHighlightID.None;
        if (Enum.TryParse(_DataBaseItem.CodeLanguage, true, out SyntaxHighlightID language))
            highlightID = language;

        textbox.SyntaxHighlighting = TextControlBox.GetSyntaxHighlightingFromID(highlightID);
    }

    private TabItemDatabaseItem _DataBaseItem;
    public TabItemDatabaseItem DatabaseItem { get => _DataBaseItem; set { _DataBaseItem = value; ApplyDatabaseItemToTextbox(); } }
    private Encoding _Encoding;
    public Encoding Encoding
    {
        get => _Encoding;
        set
        {
            _Encoding = value;

            if (_DataBaseItem == null)
                return;
            _DataBaseItem.Encoding = EncodingHelper.GetIndexByEncoding(value);
        }
    }

    public LineEnding LineEnding
    {
        get => _DataBaseItem.LineEnding;
        set
        {
            textbox.LineEnding = _DataBaseItem.LineEnding = value;
        }
    }

    public void SetTabsSpaces(int spaces = -1)
    {
        //-1 = use tabs positive values => spaces
        this.textbox.UseSpacesInsteadTabs = spaces != -1;
        if(spaces > 0)
            this.textbox.NumberOfSpacesForTab = spaces;
    }
}
