using Fastedit.Helper;
using Fastedit.Models;
using Fastedit.Settings;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Tab
{
    public class TabPageItem : TabViewItem
    {
        public delegate void TabPageHeaderChangedEvent(string header);
        public event TabPageHeaderChangedEvent TabPageHeaderChanged;

        public TextControlBox.TextControlBox textbox { get; private set; }

        public TabPageItem(TabView tabView)
        {
            Initialise(tabView);
        }

        //Remove the textbox from the current Grid:
        public void RemoveTextbox()
        {
            if (this.Content is Grid grd)
            {
                textbox.Margin = new Thickness(0, 0, 0, 0);
                grd.Children.Clear();
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


        private void Initialise(TabView tabView)
        {
            textbox = TabPageHelper.CreateTextBox();
            //add a reference to the tabitem to the textbox
            textbox.Tag = this;

            var grid = new Grid();

            this.IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Page2 };

            this.ContextFlyout = TabFlyout.CreateFlyout(this, tabView);
            this.Content = grid;
            grid.Children.Add(textbox);

            Encoding = DefaultValues.Encoding;
            DatabaseItem = new TabItemDatabaseItem
            {
                FilePath = "",
                FileToken = "",
                IsModified = false,
                ZoomFactor = 100,
            };
        }

        public bool DataIsLoaded = false;

        public TextControlBox.Renderer.CodeLanguage CodeLanguage
        {
            get => textbox.CodeLanguage;
            set
            {
                textbox.CodeLanguage = value;
                DatabaseItem.CodeLanguage = value == null ? null : value.Name;
            }
        }

        public int CountWords()
        {
            int words = 0;
            foreach (string line in textbox.Lines)
            {
                words += line.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            }
            return words;
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
            textbox.CodeLanguage = TextControlBox.TextControlBox.GetCodeLanguageFromId(_DataBaseItem.CodeLanguage ?? "");
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
