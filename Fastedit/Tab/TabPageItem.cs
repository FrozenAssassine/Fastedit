using Fastedit.Helper;
using Fastedit.Models;
using Fastedit.Settings;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Text;
using Microsoft.UI.Xaml;
using TextControlBoxNS;

namespace Fastedit.Tab
{
    public class TabPageItem : TabViewItem
    {
        public delegate void TabPageHeaderChangedEvent(string header);
        public event TabPageHeaderChangedEvent TabPageHeaderChanged;

        public TextControlBox textbox { get; private set; }
        private TabView tabView;
        public TabPageItem(TabView tabView, TabItemDatabaseItem databaseItem = null)
        {
            this.tabView = tabView;
            Initialise(tabView, databaseItem);

            this.PointerWheelChanged += TabPageItem_PointerWheelChanged;
        }

        private void TabPageItem_PointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            int scroll = e.GetCurrentPoint(sender as UIElement).Properties.MouseWheelDelta / 120;
            if(scroll > 0)
                TabPageHelper.SelectNextTab(tabView);
            else 
                TabPageHelper.SelectPreviousTab(tabView);
        }

        //Remove the textbox from the current Grid:
        public void RemoveTextbox()
        {
            if (this.Content is Grid grd)
            {
                textbox.Margin = new Thickness(0, 0, 0, 0);
                grd.Children.Remove(textbox);
            }
        }
        //add the textbox back to the current Grid:
        public void AddTextbox()
        {
            if (this.Content is Grid grd)
            {
                grd.Children.Add(textbox);
            }
        }


        private void Initialise(TabView tabView, TabItemDatabaseItem item)
        {
            textbox = TabPageHelper.CreateTextBox();
            //add a reference to the tabitem to the textbox
            textbox.Tag = this;

            var grid = new Grid();

            this.IconSource = new SymbolIconSource() { Symbol = Symbol.Document };

            this.ContextFlyout = TabFlyout.CreateFlyout(this, tabView);
            this.Content = grid;
            grid.Children.Add(textbox);

            Encoding = DefaultValues.Encoding;
            DatabaseItem = item ?? new TabItemDatabaseItem
            {
                FilePath = "",
                IsModified = false,
                ZoomFactor = 100,
            };
        }

        public bool DataIsLoaded = false;

        public SyntaxHighlightID HighlightLanguage
        {
            set
            {
                textbox.SyntaxHighlighting = TextControlBox.GetSyntaxHighlightingFromID(value);
                this.DatabaseItem.CodeLanguage = value.ToString();
            }
        }

        public int CountWords()
        {
            int wordCount = 0;

            foreach (var line in textbox.Lines)
            {
                var span = line.AsSpan();
                int index = 0;

                while (index < span.Length)
                {
                    while (index < span.Length && char.IsWhiteSpace(span[index]))
                    {
                        index++;
                    }

                    if (index < span.Length)
                    {
                        wordCount++;
                    }

                    while (index < span.Length && !char.IsWhiteSpace(span[index]))
                    {
                        index++;
                    }
                }
            }

            return wordCount;
        }
        public bool HasHeader(string header)
        {
            if (this.DatabaseItem.FileName == null)
                return false;

            return this.DatabaseItem.FileName.Equals(header, StringComparison.Ordinal);
        }
        public void SetHeader(string header)
        {
            if (DatabaseItem.IsModified)
            {

                if (this.Header == null)
                    this.Header = header ?? "";

                string currentHeader = this.Header.ToString();
                if (currentHeader.Length > 0)
                {
                    this.Header = header + (currentHeader[currentHeader.Length - 1] == '*' ? "" : "*");
                }
            }
            else
                this.Header = header;

            TabPageHeaderChanged?.Invoke(header);

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
    }
}
