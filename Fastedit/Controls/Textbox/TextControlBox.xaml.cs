using ColorCode;
using Fastedit.Core;
using Fastedit.Core.Tab;
using Fastedit.Dialogs;
using Fastedit.Extensions;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Convert = Fastedit.Extensions.Convert;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit.Controls.Textbox
{
    public sealed partial class TextControlBox : UserControl
    {
        private readonly TabKey tabkey = null;
        private readonly Linenumbers linenumbers = null;
        private readonly AppSettings appsettings = new AppSettings();
        private readonly TextControlBoxFlyoutMenu flyoutmenu = null;
        public ScrollViewer MainContentScrollViewer = null;
        public Grid LineNumberGrid = null;
        public Canvas LineNumberCanvas = null;
        public Canvas LineHighlighterCanvas = null;
        public Grid LineHighlighterControl = null;
        private int OldLineNumber = 0;
        private int OldWordCount = 0;
        private bool MarkdownRoatation = false;
        public string TextBuffer = string.Empty;
        public new bool IsLoaded = false;

        public TextControlBox()
        {
            this.InitializeComponent();
            tabkey = new TabKey(this);
            linenumbers = new Linenumbers(this, textbox);
            flyoutmenu = new TextControlBoxFlyoutMenu(this);

            //Create the flyouts
            textbox.ContextFlyout = flyoutmenu.CreateFlyout(false);
            //add events:
            textbox.AddHandler(PointerPressedEvent, new PointerEventHandler(Textbox_PointerPressed), true);
            KeyDown += TextControlBox_KeyDown;
            textbox.CopyingToClipboard += Textbox_CopyingToClipboard;
            textbox.TextChanged += Textbox_TextChanged;
            textbox.PointerWheelChanged += Textbox_PointerWheelChanged;
            textbox.SelectionChanged += Textbox_SelectionChanged;
            textbox.Paste += Textbox_Paste;
            //ScrollViewer
            ScrollViewer.SetHorizontalScrollMode(textbox, ScrollMode.Enabled);
            ScrollViewer.SetVerticalScrollMode(textbox, ScrollMode.Enabled);
            base.Focus(FocusState.Programmatic);
            linenumbers.UpdateLinenumberRendering();
        }

        //Events
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateMarkdownSize();
            UpdateLineHighlighter();
        }
        private void Textbox_CopyingToClipboard(RichEditBox sender, TextControlCopyingToClipboardEventArgs args)
        {
            Copy();
            args.Handled = true;
        }
        private void Textbox_Paste(object sender, TextControlPasteEventArgs e)
        {
            //Prevent the textbox from pasting because it has a custom paste function
            e.Handled = true;
        }
        private void OnLineNumberGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (LineNumberGrid != null)
            {
                linenumbers.ResetLinenumberCanvas();
            }
        }
        private void Textbox_Loaded(object sender, RoutedEventArgs e)
        {
            //Find and assign the children to it's variable
            LineHighlighterCanvas = textbox.FindDependencyObject("LineHighlighterCanvas") as Canvas;
            LineHighlighterControl = textbox.FindDependencyObject("LineHighlighterControl") as Grid;
            LineHighlighterControl.Background = new SolidColorBrush(LineHighlighterBackground);
            LineHighlighterControl.BorderBrush = new SolidColorBrush(LineHighlighterForeground);
            LineNumberCanvas = textbox.FindDependencyObject("LineNumberCanvas") as Canvas;
            LineNumberGrid = textbox.FindDependencyObject("LineNumberGrid") as Grid;
            MainContentScrollViewer = textbox.FindDependencyObject("ContentElement") as ScrollViewer;

            linenumbers.UpdateLinenumberRendering();
            textbox.Focus(FocusState.Programmatic);
        }
        private void Textbox_TextChanged(object sender, RoutedEventArgs e)
        {
            string text = GetText();
            linenumbers.UpdateLinenumberRendering();

            if (TextBeforeLastSaved != text)
            {
                TextChangedevent?.Invoke(this);
                SaveStatusChangedEvent?.Invoke(this, IsModified);
                IsModified = true;
                if (MarkdownPreview && text != null)
                {
                    markdowntextblock.Text = text;
                }
            }
            int words = CountWords(text);
            if (words != OldWordCount)
            {
                OldWordCount = words;
                WordCountChangedEvent?.Invoke(this, words);
            }
        }
        private void Textbox_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var Control = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
            if (Control.HasFlag(CoreVirtualKeyStates.Down))
            {
                ScrollViewer.SetVerticalScrollMode(textbox, ScrollMode.Disabled);

                int delta = e.GetCurrentPoint((UIElement)sender).Properties.MouseWheelDelta;

                if (delta < 0)
                {
                    ZoomOut(0.1);
                }

                if (delta > 0)
                {
                    ZoomIn(0.1);
                }

                ScrollViewer.SetVerticalScrollMode(textbox, ScrollMode.Enabled);
            }
        }
        private void Textbox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateLineHighlighter();
            UpdateLineNumber();

            tabdatafromdatabase.TabSelLenght = SelectionLenght;
            tabdatafromdatabase.TabSelStart = SelectionStart;
        }
        private void Textbox_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Pointer_PressedEvent?.Invoke(this, e);         
        }
        private void TextControlBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            KeyPressedEvent?.Invoke(this, e);
            var Shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);
            var Control = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);

            if (Control.HasFlag(CoreVirtualKeyStates.Down) && Shift.HasFlag(CoreVirtualKeyStates.None))
            {
                //Cut // Copy // Paste
                KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.C, Copy);
                KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.V, Paste);
                KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.X, Cut);
                KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.D, DuplicateLine);

                //Undo // Redo
                KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.Z, Undo);
                KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.Y, Redo);

                //Select all
                KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.A, SelectAll);

                //Scroll up // Scroll down
                KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.Up, ScrollWithArrowKeys, VerticalAxis.Up);
                KeyboardCommands.KeyboardCommand(e.Key, VirtualKey.Down, ScrollWithArrowKeys, VerticalAxis.Down);

                //Zoom in / Zoom out
                if (e.Key.ToString() == "187")
                {
                    ZoomIn(DefaultValues.DefaultZoomFactor);
                }
                if (e.Key.ToString() == "189")
                {
                    ZoomOut(DefaultValues.DefaultZoomFactor);
                }
            }
            else
            {
                //TabKey
                if (Shift.HasFlag(CoreVirtualKeyStates.Down))
                {
                    if (e.Key == VirtualKey.Tab && SelectionLenght == 0)
                    {
                        e.Handled = true;
                        tabkey.MoveTextWithTab_Back_WithoutSelection();
                    }
                    else if (e.Key == VirtualKey.Tab && SelectionLenght != 0)
                    {
                        e.Handled = true;
                        tabkey.MoveTextWithTab_Back_WithSelection();
                    }
                }
                else if (Shift.HasFlag(CoreVirtualKeyStates.None))
                {
                    if (e.Key == VirtualKey.Tab && SelectionLenght == 0)
                    {
                        e.Handled = true;
                        tabkey.MoveTextWithTab_Forward_WithoutSelection();
                    }
                    else if (e.Key == VirtualKey.Tab && SelectionLenght != 0)
                    {
                        e.Handled = true;
                        tabkey.MoveTextWithTab_Forward_WithSelection();
                    }
                }
            }
        }
        private void TextControlBox_ScrollChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (LineHighlighterCanvas != null && LineHighlighter == true)
            {
                MainContentScrollViewer.StartExpressionAnimation(LineHighlighterCanvas, Axis.Y);
            }

            MainContentScrollViewer.StartExpressionAnimation(LineNumberCanvas, Axis.Y);
            linenumbers.UpdateLinenumberRendering();
            UpdateLineHighlighterScroll();
        }
        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            linenumbers.UpdateLinenumberRendering();
        }
        private void Markdowntextblock_MarkdownRendered(object sender, MarkdownRenderedEventArgs e)
        {
            if (progressring != null)
            {
                progressring.IsActive = false;
                progressring.Visibility = Visibility.Collapsed;
            }
        }

        //Store data
        public TabDataForDatabase tabdatafromdatabase = new TabDataForDatabase
        {
            ZoomFactor = 100,
            TabName = "",
            TabModified = false,
            TabHeader = "",
            TabToken = "",
            TabPath = "",
            DataBaseName = "",
            TabTemp = "",
            TabReadOnly = false,
            TabSelLenght = 0,
            TabSelStart = 0,
            CurrentSelectedTabIndex = 0,
            TabSaveMode = TabSaveMode.SaveAsFile,
            TabEncoding = 1,
            Markdown = false,
            MarkdownMode2 = false,
            MarkdownIsColumn = false,
            WordWrap = TextWrapping.NoWrap,
        };
        public string IdentifierName
        {
            get { return this.Name; }
            set { this.Name = value; tabdatafromdatabase.TabName = value; }
        }
        public string FilePath { get => tabdatafromdatabase.TabPath; set { tabdatafromdatabase.TabPath = value; } }
        public string DataBaseName { get => tabdatafromdatabase.DataBaseName; set { tabdatafromdatabase.DataBaseName = value; } }
        public string Header
        {
            get => tabdatafromdatabase.TabHeader;
            set
            {
                DocumentTitleChangedEvent?.Invoke(this, value);
                tabdatafromdatabase.TabHeader = value;
            }
        }
        public bool IsModified
        {
            get => tabdatafromdatabase.TabModified;
            set
            {
                tabdatafromdatabase.TabModified = value;
                SaveStatusChangedEvent?.Invoke(this, value);
            }
        }
        public TabSaveMode TabSaveMode { get => tabdatafromdatabase.TabSaveMode; set {tabdatafromdatabase.TabSaveMode = value; } }
        public string FileToken { get => tabdatafromdatabase.TabToken; set { tabdatafromdatabase.TabToken = value; } }
        public string TempFile { get => tabdatafromdatabase.TabTemp; set { tabdatafromdatabase.TabTemp = value; } }
        private Encoding _Encoding = Encoding.Default;
        public Encoding Encoding
        {
            get => _Encoding;
            set
            {
                if (_Encoding != value)
                {
                    _Encoding = value;
                    tabdatafromdatabase.TabEncoding = Encodings.EncodingToInt(value);
                    IsModified = true;
                    EncodingChangedEvent?.Invoke(this, value);
                }
            }
        }
        public StorageFile Storagefile { get; set; } = null;
        public string TextBeforeLastSaved { get; set; } = "";
        
        //Just for setting the Encoding, without affecting the Modified state
        public void SetEncoding(Encoding encoding)
        {
            if (_Encoding != encoding)
            {
                _Encoding = encoding;
                tabdatafromdatabase.TabEncoding = Encodings.EncodingToInt(encoding);
                EncodingChangedEvent?.Invoke(this, encoding);
            }
        }

        //TextModes:
        public TextWrapping WordWrap
        {
            get => textbox.TextWrapping;
            set { textbox.TextWrapping = value; linenumbers.UpdateLinenumberRendering(); tabdatafromdatabase.WordWrap = value; }
        }
        public bool IsReadOnly
        {
            get => textbox.IsReadOnly;
            set { textbox.IsReadOnly = value; tabdatafromdatabase.TabReadOnly = value; }
        }
        public bool SpellChecking
        {
            get => textbox.IsSpellCheckEnabled;
            set { textbox.IsSpellCheckEnabled = value; }
        }
        public bool IsHandWritingEnabled
        {
            get => textbox.IsHandwritingViewEnabled;
            set { textbox.IsHandwritingViewEnabled = value; }
        }

        //Font & Fontsize
        private async void SetFontSize(double val)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                _textfontsize = val;
                if (val > DefaultValues.MaxFontWithZoom)
                {
                    val = DefaultValues.MaxFontWithZoom;
                }
                else if (val < DefaultValues.MinFontWithZoom)
                {
                    val = DefaultValues.MinFontWithZoom;
                }

                if (textbox != null)
                {
                    textbox.FontSize = val;
                }

                var newZoomFactor = Math.Round(val * 100 / appsettings.GetSettingsAsInt("FontSize", DefaultValues.DefaultFontsize));
                if (Math.Abs(newZoomFactor - _zoomFactor) >= 1)
                {
                    _zoomFactor = newZoomFactor;
                }
                ZoomChangedEvent?.Invoke(this, newZoomFactor);
            });
        }
        private double _textfontsize = DefaultValues.DefaultFontsize;
        public new double FontSize
        {
            get { return _textfontsize; }
            set
            {
                SetFontSize(value);
            }
        }
        public double FontSizeWithoutZoom
        {
            set
            {
                if (value > DefaultValues.MaxFontWithZoom)
                {
                    value = DefaultValues.MaxFontWithZoom;
                }
                else if (value < DefaultValues.MinFontWithZoom)
                {
                    value = DefaultValues.MinFontWithZoom;
                }
                textbox.FontSize = value;
            }
        }
        public new FontFamily FontFamily
        {
            get { return textbox.FontFamily; }
            set
            {
                textbox.FontFamily = value;

                linenumbers.UpdateLinenumberRendering();
            }
        }

        public Rect GetSelectionRect()
        {
            textbox.Document.Selection.GetRect(Windows.UI.Text.PointOptions.ClientCoordinates,
                out Rect rect, out var _);
            return rect;
        }

        //Appearance:
        public Color TextColor
        {
            get
            {
                if (textbox.Foreground is SolidColorBrush colorBrush)
                {
                    return colorBrush.Color;
                }
                else
                {
                    return Color.FromArgb(255, 0, 0, 0);
                }
            }
            set
            {
                textbox.Foreground = new SolidColorBrush(value);
            }
        }
        public Color LineNumberBackground
        {
            get
            {
                if (LineNumberGrid != null)
                {
                    return Convert.ToColor(LineNumberGrid.Background, Colors.Gray);
                }
                else
                {
                    return Color.FromArgb(0, 0, 0, 0);
                }
            }
            set
            {
                if (LineNumberGrid != null)
                {
                    LineNumberGrid.Background = new SolidColorBrush(value);
                }

                linenumbers.UpdateLinenumberRendering();
            }
        }
        private Color _LineNumberForeground;
        public Color LineNumberForeground
        {
            get { return _LineNumberForeground; }
            set
            {
                _LineNumberForeground = value;
                linenumbers.UpdateLinenumberRendering();
            }
        }
        public new Color Background
        {
            get => Convert.ToColor(textbox.Background, Colors.Gray);
            set
            {
                textbox.Background = new SolidColorBrush(value);
                if(markdowntextblock != null)
                    markdowntextblock.Background = new SolidColorBrush(value);
            }
        }
        public Color TextSelectionColor
        {
            get => textbox.SelectionHighlightColor.Color;
            set
            {
                textbox.SelectionHighlightColor = new SolidColorBrush(value);
                textbox.SelectionHighlightColorWhenNotFocused = new SolidColorBrush(value);
            }
        }
        public double BackgroundOpacity
        {
            get { return textbox.Background.Opacity; }
            set { textbox.Background.Opacity = value; }
        }
        private bool _ShowLineNumbers { get; set; }
        public bool ShowLineNumbers
        {
            get { return _ShowLineNumbers; }
            set
            {
                _ShowLineNumbers = value;
                if (value == true)
                    linenumbers.ShowLinenumbers();
                else
                    linenumbers.HideLinenumbers();
            }
        }
        private Color _LineHighlighterForeground;
        public Color LineHighlighterForeground
        {
            get
            {
                return _LineHighlighterForeground;
            }
            set 
            {
                _LineHighlighterForeground = value;
                if (LineHighlighterControl != null)
                    LineHighlighterControl.BorderBrush = new SolidColorBrush(value);
            }
        }
        private Color _LineHighlighterBackground;
        public Color LineHighlighterBackground
        {
            get
            {
                return _LineHighlighterBackground;
            }
            set
            {
                _LineHighlighterBackground = value;
                if (LineHighlighterControl != null)
                    LineHighlighterControl.Background = new SolidColorBrush(value);
            }
        }

        //Rightclickmenu
        public bool ShowSelectionFlyout
        {
            set 
            {
                textbox.SelectionFlyout = value ? flyoutmenu.CreateSelectionFlyout() : null;
            }
            get => textbox.SelectionFlyout == null ? false : true;
        }
        public void UpdateContextFlyout()
        {
            textbox.ContextFlyout = flyoutmenu.CreateFlyout(false);
        }

        //Line highlighter
        public void UpdateLineHighlighterScroll()
        {
            if (MainContentScrollViewer == null || LineHighlighterControl == null)
                return;

            var selsize = GetSelectionRect();
            var MarginTop = selsize.Y - MainContentScrollViewer.VerticalOffset + LineHighlighterControl.Height*2;
            bool condition = MarginTop < 40 || MarginTop > textbox.ActualHeight + 10;

            LineHighlighterControl.Visibility =  Convert.BoolToVisibility(!condition);
        }
        public void UpdateLineHighlighter()
        {
            if (!LineHighlighter || LineHighlighterCanvas == null || LineHighlighterControl == null)
            {
                return;
            }

            if (SelectionIsMultiline() && LineHighlighterControl.Visibility == Visibility.Visible)
            {
                LineHighlighterControl.Visibility = Visibility.Collapsed;
            }
            else if (LineHighlighterControl.Visibility == Visibility.Collapsed && SelectionLenght == 0)
            {
                LineHighlighterControl.Visibility = Visibility.Visible;
            }

            if (LineHighlighterControl.Visibility == Visibility.Visible)
            {
                LineHighlighterControl.BorderThickness = new Thickness((double)_zoomFactor / 50);
                LineHighlighterControl.Height = GetSingleLineHeight() + 4;
                LineHighlighterControl.Margin =
                    new Thickness(LineNumberGrid.ActualWidth + 5, (GetSelectionRect().Y + textbox.Padding.Top - 2), 0, 0);
                int Width = (int)(textbox.ActualWidth - LineNumberGrid.ActualWidth - 5);
                LineHighlighterControl.Width = Width < 0 ? 0 : Width - (MainContentScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible ? 7 : 0);
            }
        }
        private bool _LineHighlighter = false;
        public bool LineHighlighter
        {
            set
            {
                if (value)
                {
                    UpdateLineHighlighter();
                    if(LineHighlighterControl != null && LineHighlighterCanvas != null)
                        LineHighlighterControl.Visibility = LineHighlighterCanvas.Visibility = Visibility.Visible;
                }
                else
                {
                    if(LineHighlighterControl != null)
                    {
                        LineHighlighterControl.Visibility = LineHighlighterCanvas.Visibility = Visibility.Collapsed;
                    }
                }
                _LineHighlighter = value;
            }
            get => _LineHighlighter;
        }
       
        /*
    //Syntax Highlighting
    private RtfFormatter _rtfFormatter;
    private ILanguage _language;
    private DispatcherTimer UpdateSyntaxHighlightingTimer = new DispatcherTimer();
    private bool _SyntaxHighlighting { get; set; }
    public bool SyntaxHighlighting
    {
        get => _SyntaxHighlighting;
        set
        {
            if (value)
            {
                _rtfFormatter = new RtfFormatter(this, ActualTheme == ElementTheme.Light ? SyntaxHighlightingColorCodes.Light : SyntaxHighlightingColorCodes.Dark);
                _language = new CS_Language();
                textbox.TextChanging += Textbox_TextChanging;
                textbox.ActualThemeChanged += Textbox_ActualThemeChanged;

                UpdateSyntaxHighlightingTimer.Interval = new TimeSpan(0, 0, 0, 2);
                UpdateSyntaxHighlightingTimer.Tick += UpdateSyntaxHighlightingTimer_Tick;
            }
            else
            {
                _rtfFormatter = null;
                _language = null;
                textbox.TextChanging -= Textbox_TextChanging;
                textbox.ActualThemeChanged -= Textbox_ActualThemeChanged;
            }
            _SyntaxHighlighting = value;
        }
    }
    private void UpdateSyntaxHighlightingTimer_Tick(object sender, object e)
    {
        UpdateSyntaxHighlightingForText();
        UpdateSyntaxHighlightingTimer.Stop();
    }
    public int GetFontSizeFromRTF()
    {
        string text = "";
        textbox.Document.GetText(TextGetOptions.FormatRtf, out text);
        int index = text.IndexOf(@"\fs");
        int endindex = text.IndexOf(@"\lang1033");
        if (index == -1|| endindex == -1)
            return 16;
        text = text.Substring(index + 3, endindex - index - 3);
        return Convert.ToInt(text, 16);
    }
    public void UpdateSyntaxHighlightingForText()
    {
        // Attempt to get Scrollviewer offsets, to preserve location.
        var vertOffset = MainContentScrollViewer.VerticalOffset;
        var horOffset = MainContentScrollViewer.HorizontalOffset;

        var selstart = SelectionStart;
        var sellenght = SelectionLenght;

        textbox.Document.GetText(TextGetOptions.UseCrlf, out string raw);
        textbox.Document.Undo();
        textbox.Document.BeginUndoGroup();

        var rtf = _rtfFormatter.GetRtfString(raw, _language);

        bool isreadonly = IsReadOnly;
        if (isreadonly)
            textbox.IsReadOnly = false;

        textbox.Document.SetText(TextSetOptions.FormatRtf, rtf);

        if (isreadonly == true)
            textbox.IsReadOnly = isreadonly;
        SetSelection(selstart, sellenght);

        textbox.Document.ApplyDisplayUpdates();
        textbox.Document.EndUndoGroup();
        //Debug.WriteLine(selstart + "::" + sellenght + "::::" + SelectionStart + "::" + SelectionLenght);

        MainContentScrollViewer.ChangeView(horOffset, vertOffset, null, true);
    }
    private void Textbox_TextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
    {
        //if (args.IsContentChanging && _SyntaxHighlighting)
        //{
        //    if (UpdateSyntaxHighlightingTimer.IsEnabled)
        //        return;
        //    else
        //        UpdateSyntaxHighlightingTimer.Start();
        //}
    }
    private void Textbox_ActualThemeChanged(FrameworkElement sender, object args)
    {
        _rtfFormatter = new RtfFormatter(this, ActualTheme == ElementTheme.Light ? SyntaxHighlightingColorCodes.Light : SyntaxHighlightingColorCodes.Dark);
        UpdateSyntaxHighlightingForText();
    }
    private void UserControl_GotFocus(object sender, RoutedEventArgs e)
    {
        if (SyntaxHighlighting)
            UpdateSyntaxHighlightingForText();
    }
*/

        //Markdown
        public bool MarkdownPreview
        {
            get => markdowntextblock != null;
            set
            {
                tabdatafromdatabase.Markdown = value;
                if (value == true)
                {
                    if (markdowntextblock == null || markdowntextblock_sv == null || Splittedview_Splitter == null)
                    {
                        markdowntextblock_sv = FindName("markdowntextblock_sv") as ScrollViewer;
                        markdowntextblock = FindName("markdowntextblock") as MarkdownTextBlock;
                        Splittedview_Splitter = FindName("Splittedview_Splitter") as GridSplitter;

                        if (MarkdownPreview)
                        {
                            markdowntextblock.Text = GetText();
                            progressring = FindName("progressring") as muxc.ProgressRing;

                            markdowntextblock.Background = textbox.Background;
                            MarkdownRoatation = tabdatafromdatabase.MarkdownIsColumn;
                            Markdown_UpdateRotation();

                            if (tabdatafromdatabase.MarkdownMode2) //left-right / top-bottom
                            {
                                if (tabdatafromdatabase.MarkdownIsColumn)
                                {
                                    Grid.SetColumn(markdowntextblock_sv, 0);
                                    Grid.SetColumn(Splittedview_Splitter, 1);
                                    Grid.SetColumn(textbox, 2);
                                }
                                else
                                {
                                    Grid.SetRow(textbox, 2);
                                    Grid.SetRow(Splittedview_Splitter, 1);
                                    Grid.SetRow(markdowntextblock_sv, 0);
                                }
                            }
                            else
                            {
                                if (tabdatafromdatabase.MarkdownIsColumn)
                                {
                                    Grid.SetColumn(markdowntextblock_sv, 2);
                                    Grid.SetColumn(Splittedview_Splitter, 1);
                                    Grid.SetColumn(textbox, 0);
                                }
                                else
                                {
                                    Grid.SetRow(textbox, 0);
                                    Grid.SetRow(Splittedview_Splitter, 1);
                                    Grid.SetRow(markdowntextblock_sv, 2);
                                }
                            }
                            UpdateMarkdownSize();
                        }

                    }
                }
                else
                {
                    splittedgrid.ColumnDefinitions.Clear();
                    splittedgrid.RowDefinitions.Clear();
                    if(markdowntextblock_sv != null)
                        UnloadObject(markdowntextblock_sv);
                    if(markdowntextblock != null)
                        UnloadObject(markdowntextblock);
                    if(Splittedview_Splitter != null)
                        UnloadObject(Splittedview_Splitter);
                }
            }
        }    
        private void SetGridColumns()
        {
            if (splittedgrid.ColumnDefinitions.Count > 0)
                return;
            splittedgrid.RowDefinitions.Clear();
            splittedgrid.ColumnDefinitions.Add(new ColumnDefinition());
            splittedgrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16)});
            splittedgrid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        private void SetGridRows()
        {
            if (splittedgrid.RowDefinitions.Count > 0)
                return;
            splittedgrid.ColumnDefinitions.Clear();
            splittedgrid.RowDefinitions.Add(new RowDefinition());
            splittedgrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(16) });
            splittedgrid.RowDefinitions.Add(new RowDefinition());
        }
        private bool Markdown_Column()
        {
            SetGridColumns();
            bool res = false;
            if (Grid.GetColumn(markdowntextblock_sv) == 2)
            {
                Grid.SetColumn(markdowntextblock_sv, 0);
                Grid.SetColumn(textbox, 2);
                res = true;
            }
            else if (Grid.GetColumn(markdowntextblock_sv) == 0)
            {
                Grid.SetColumn(markdowntextblock_sv, 2);
                Grid.SetColumn(textbox, 0);
            }
            Grid.SetColumn(Splittedview_Splitter, 1);
            Markdown_Button_Rotation.Angle = 0;
            return res;
        }
        private bool Markdown_Row()
        {
            SetGridRows();
            bool res = false;
            if (Grid.GetRow(markdowntextblock_sv) == 2)
            {
                Grid.SetRow(markdowntextblock_sv, 0);
                Grid.SetRow(textbox, 2);
                res = true;
            }
            else if (Grid.GetRow(markdowntextblock_sv) == 0)
            {
                Grid.SetRow(markdowntextblock_sv, 2);
                Grid.SetRow(textbox, 0);
            }
            Grid.SetRow(Splittedview_Splitter, 1);
            Markdown_Button_Rotation.Angle = 90;
            return res;
        }
        private void Markdown_ToggleLeftRight()
        {
            if (!MarkdownPreview)
                return;
            bool res = false;
            //Markdowntextblock is column
            if (tabdatafromdatabase.MarkdownIsColumn)
            {
                res = Markdown_Column();
            }
            //Markdowntextblock is row
            else
            {
                res = Markdown_Row();
            }
            tabdatafromdatabase.MarkdownMode2 = res; //left right / top bottom
            tabdatafromdatabase.MarkdownIsColumn = tabdatafromdatabase.MarkdownIsColumn; //Is column
        }
        private void Markdown_UpdateRotation()
        {
            if (!MarkdownRoatation)
            {
                MarkdownRoatation = true;
                tabdatafromdatabase.MarkdownIsColumn = false;
                GridSplitter_GripperBar.Height = 5;
                GridSplitter_GripperBar.Width = 25;
                Markdown_Button_Rotation.Angle = 90;
                Markdown_Button_Rotation.CenterY = 0;
                SetGridRows();
                GridsplitterSettingsFlyout.Margin = new Thickness(100, 0, 0, 0);
            }
            else
            {
                MarkdownRoatation = false;
                tabdatafromdatabase.MarkdownIsColumn = true;
                GridSplitter_GripperBar.Height = 25;
                GridSplitter_GripperBar.Width = 5;
                Markdown_Button_Rotation.Angle = 0;
                Markdown_Button_Rotation.CenterY = 14;
                SetGridColumns();
                GridsplitterSettingsFlyout.Margin = new Thickness(0, -135, 0, 0);
            }
        }
        private void UpdateMarkdownSize()
        {
            if (MarkdownPreview)
            {
                if (tabdatafromdatabase.MarkdownIsColumn)
                {
                    Splittedview_Splitter.Width = 16;
                    Splittedview_Splitter.Height = this.ActualHeight;
                }
                else
                {
                    Splittedview_Splitter.Height = 16;
                    Splittedview_Splitter.Width = this.ActualWidth;
                }
                UpdateLineHighlighter();
            }
        }
        private void ToggleLeftRight_Markdown_Click(object sender, RoutedEventArgs e)
        {
            Markdown_ToggleLeftRight();
            UpdateMarkdownSize();
        }    
        private void Rotate90Degree_Click(object sender, RoutedEventArgs e)
        {
            Markdown_UpdateRotation();
            Markdown_ToggleLeftRight();
            UpdateMarkdownSize();
        }
        private void CloseMarkdown_Click(object sender, RoutedEventArgs e)
        {
            MarkdownPreview = false;
        }
        
        //Selection:
        public string SelectedText
        {
            get { return textbox.Document.Selection.Text; }
            set { textbox.Document.Selection.Text = value; }
        }
        public int SelectionStart
        {
            get { return textbox.Document.Selection.StartPosition; }
            set { textbox.Document.Selection.StartPosition = value; }
        }
        public int SelectionLenght
        {
            get { return textbox.Document.Selection.Length; }
        }
        public void SetSelection(int StartIndex, int Lenght)
        {
            textbox.Document.Selection.SetRange(StartIndex, StartIndex + Lenght);
        }
        public void SelectAll()
        {
            SetSelection(0, GetText().Length);
        }
        public bool SelectionIsMultiline()
        {
            return textbox.Document.Selection.Text.Contains("\r") || textbox.Document.Selection.Text.Contains("\n");
        }
        
        //Zoom actions:
        private double __zoomFactor = 100;
        public double _zoomFactor { get => __zoomFactor; set { __zoomFactor = value; tabdatafromdatabase.ZoomFactor = value; } }
        public void SetFontZoomFactor(double ZoomFactor)
        {
            var fontZoomFactor = Math.Round(ZoomFactor);
            if (fontZoomFactor >= DefaultValues.MinZoom && fontZoomFactor <= DefaultValues.MaxZoom)
            {
                FontSize = fontZoomFactor / 100 * appsettings.GetSettingsAsInt("FontSize", DefaultValues.DefaultFontsize);
                _zoomFactor = ZoomFactor;
            }
        }
        public void ZoomIn(double Factor)
        {
            if (_zoomFactor < DefaultValues.MaxZoom)
            {
                if (_zoomFactor % 10 > 0)
                {
                    SetFontZoomFactor(Math.Ceiling(_zoomFactor / 10) * 10);
                }
                else
                {
                    FontSize += Factor * appsettings.GetSettingsAsInt("FontSize", DefaultValues.DefaultFontsize);
                }
            }
        }
        public void ZoomOut(double Factor)
        {
            if (_zoomFactor > DefaultValues.MinZoom)
            {
                if (_zoomFactor % 10 > 0)
                {
                    SetFontZoomFactor(Math.Floor(_zoomFactor / 10) * 10);
                }
                else
                {
                    FontSize -= Factor * appsettings.GetSettingsAsInt("FontSize", DefaultValues.DefaultFontsize);
                }
            }
        }

        //Cursorposition & Linenumbering
        public int GetCurrentLineNumber
        {
            get
            {
                return textbox.Document.Selection.GetIndex(TextRangeUnit.Paragraph);
            }
        }
        public int GetLinesCount
        {
            get
            {
                return GetText().Split('\r', '\n').Length;
            }
        }
        public string[] GetLineNumberContent
        {
            get
            {
                return GetText().Split('\r', '\n');
            }
        }
        public double GetSingleLineHeight()
        {
            textbox.Document.GetRange(0, 0).GetRect(PointOptions.ClientCoordinates, out var rect, out _);
            return rect.Height <= 0 ? 1.35 * textbox.FontSize : rect.Height;
        }
        public int CountWords(string text = null)
        {
            if (text == null)
            {
                text = GetText();
            }

            return text.Split(new char[] { '\n', ' ', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        //Scrollbar-Positions
        public ScrollbarPositions GetScrollbarPositions()
        {
            if (MainContentScrollViewer != null)
            {
                return new ScrollbarPositions
                {
                    ScrollbarPositionHorizontal = MainContentScrollViewer.HorizontalOffset,
                    ScrollbarPositionVertical = MainContentScrollViewer.VerticalOffset
                };
            }
            else
            {
                return new ScrollbarPositions
                {
                    ScrollbarPositionHorizontal = 0,
                    ScrollbarPositionVertical = 0
                };
            }
        }
        public void SetScrollbarPositions(ScrollbarPositions pos, bool DisableAnimations = false)
        {
            if (MainContentScrollViewer != null)
            {
                MainContentScrollViewer.ChangeView(pos.ScrollbarPositionHorizontal, pos.ScrollbarPositionVertical, null, DisableAnimations);
            }
        }
        public void ScrollIntoView()
        {
            if (MainContentScrollViewer != null)
            {
                double vOffset = MainContentScrollViewer.VerticalOffset + this.ActualHeight / 2;
                MainContentScrollViewer.ChangeView(null, vOffset, null, true);
            }
        }

        //Undo-Redo
        public void Undo()
        {
            textbox.Document.Undo();
        }
        public void Redo()
        {
            textbox.Document.Redo();
        }

        //Basic Textediting:
        public void Copy()
        {
            void DoCopy(string text = null)
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(text == null ? textbox.Document.Selection.Text : text);
                Clipboard.SetContentWithOptions(
                    dataPackage,
                    new ClipboardContentOptions()
                    {
                        IsAllowedInHistory = true,
                        IsRoamable = true
                    });
                try
                {
                    Clipboard.Flush();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception in TextControlBox --> Copy:" + "\n" + e.Message);
                }
            }
            if (textbox.Document.Selection.Length == 0)
            {
                DoCopy(GetLineNumberContent[GetCurrentLineNumber]);
            }
            else
            {
                DoCopy();
            }
        }
        public void DuplicateLine()
        {
            if (textbox.Document.Selection.Length == 0)
            {
                textbox.Document.Selection.SetIndex(TextRangeUnit.Paragraph, GetCurrentLineNumber, false);
                string selectedtext = GetLineNumberContent[GetCurrentLineNumber-1];
                textbox.Document.Selection.Text = selectedtext + "\n";
                SetSelection(SelectionStart + (selectedtext.Length*2)+1, 0);
            }
            else
            {
                SetSelection(SelectionStart, 0);
                DuplicateLine();
            }
        }
        public async void Paste()
        {
            if (!IsReadOnly)
            {
                DataPackageView dataPackageView = Clipboard.GetContent();
                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    string text = await dataPackageView.GetTextAsync();
                    if (SelectionLenght > 0)
                    {
                        textbox.Document.Selection.Text = "";
                    }

                    textbox.Document.Selection.Text = text;
                    textbox.Document.Selection.SetRange(SelectionStart + text.Length, SelectionStart + text.Length);
                }
            }
        }
        public void Cut()
        {
            if (!IsReadOnly)
            {
                if (textbox.Document.Selection.Length == 0)
                {
                    var CurrentSelStart = SelectionStart;

                    SelectLine(GetCurrentLineNumber);
                    textbox.Document.Selection.Cut();
                    SetSelection(CurrentSelStart, 0);

                }
                else
                {
                    textbox.Document.Selection.Cut();
                }
            }
        }
        public void SurroundSelectionBy(string surroundtext)
        {
            textbox.Document.Selection.Text = surroundtext + SelectedText + surroundtext;
        }
        public void SurroundSelectionBy(string surroundtext1, string surroundtext2)
        {
            textbox.Document.Selection.Text = surroundtext1 + SelectedText + surroundtext2;
        }
        
        //Info-Toast
        public void ShowInfoToast(bool ShowSavedMessage, string text = "")
        {
            ContentSavedInfo.IsOpen = true;
            ContentSavedInfo.Background = DefaultValues.ContentDialogBackgroundColor();
            ContentSavedInfo.Foreground = DefaultValues.ContentDialogForegroundColor();
            if (text.Length == 0 || ShowSavedMessage)
            {
                ContentSavedInfo.Title = appsettings.GetResourceString("Toast_SaveSucced/Text");
            }
            else
            {
                ContentSavedInfo.Title = text;
            }

            DispatcherTimer hidetoastTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, DefaultValues.TextBoxInfoToast_Time)
            };
            hidetoastTimer.Stop();
            hidetoastTimer.Tick += delegate
            {
                hidetoastTimer.Stop();
                ContentSavedInfo.IsOpen = false;
            };
            hidetoastTimer.Start();
        }

        //Set & Get text
        /// <summary>
        /// Get the text of the textbox
        /// </summary>
        /// <returns>The text</returns>
        public string GetText()
        {
            if (!IsLoaded)
            {
                return TextBuffer;
            }
            textbox.Document.GetText(TextGetOptions.None, out string out1);
            if (out1.Length != 0)
            {
                out1 = out1.Remove(out1.Length - 1, 1);
            }
            if (out1 == null)
                return "";
            return out1;
        }
        /// <summary>
        /// Set text to textbox
        /// </summary>
        /// <param name="text">The text to set to</param>
        public async Task SetText(string text, bool ismodified = false)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                bool isreadonly = IsReadOnly;
                if (isreadonly)
                    textbox.IsReadOnly = false;

                textbox.Document.SetText(TextSetOptions.None, text);
                TextBeforeLastSaved = GetText();
                IsModified = ismodified;
                textbox.TextDocument.ClearUndoRedoHistory();

                if (isreadonly == true)
                    textbox.IsReadOnly = isreadonly;
            });
        }
        /// <summary>
        /// Overrides the whole text, but the modified state stays
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task ChangeText(string text, TextSetOptions textSetOptions = TextSetOptions.None)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                bool isreadonly = IsReadOnly;
                if (isreadonly)
                    textbox.IsReadOnly = false;

                textbox.Document.SetText(textSetOptions, text);

                if (isreadonly == true)
                    textbox.IsReadOnly = isreadonly;
            });
        }

        //Replace text
        public bool ReplaceInText(string Word, string ReplaceWord, bool Up, bool MatchCase, bool WholeWord)
        {
            if (Word.Length == 0)
            {
                return false;
            }

            bool res = FindInText(Word, Up, MatchCase, WholeWord);
            if (res)
            {
                SelectedText = ReplaceWord;
            }

            return res;
        }
        public bool ReplaceAll(string Word, string ReplaceWord, bool Up, bool MatchCase, bool WholeWord)
        {

            if (Word.Length == 0)
            {
                return false;
            }

            int selstart = SelectionStart, sellenght = SelectionLenght;

            if (!WholeWord)
            {
                SelectAll();
                if (MatchCase)
                {
                    SelectedText = GetText().Replace(Word, ReplaceWord);
                }
                else
                {
                    SelectedText = GetText().Replace(Word.ToLower(), ReplaceWord.ToLower());
                }

                return true;
            }

            SetSelection(0, 0);
            bool res = true;
            while (res)
            {
                res = ReplaceInText(Word, ReplaceWord, Up, MatchCase, WholeWord);
            }

            SetSelection(selstart, sellenght);
            return true;
        }

        /// <summary>
        /// Selects a word in the textbox text
        /// </summary>
        /// <param name="Word">The Word to search for</param>
        /// <param name="Up">The searchdirection: Up / Down</param>
        /// <param name="MatchCase"></param>
        /// <param name="WholeWord"></param>
        /// <param name="Text">The Text to search in. If null it uses the textbox text</param>
        /// <returns></returns>
        public bool FindInText(string Word, bool Up, bool MatchCase, bool WholeWord)
        {
            string Text = GetText();
            bool NotFound()
            {
                if (Word.Length > 20)
                {
                    Word = Word.Substring(0, 20) + "... ";
                }
                SetSelection(0, 0);
                ShowInfoToast(false, Word + " " + InfoBarMessages.SearchNotFound);
                return false;
            }
            //Search down:
            if (!Up)
            {
                if (!MatchCase)
                {
                    Text = Text.ToLower();
                    Word = Word.ToLower();
                }
                if (SelectionStart == -1)
                {
                    SelectionStart = 0;
                }

                int startpos = SelectionStart;
                if (SelectionLenght > 0)
                {
                    startpos = SelectionStart + SelectionLenght;
                }
                if (Word.Length + startpos > Text.Length)
                {
                    return NotFound();
                }
                
                    int index = WholeWord ? Extensions.StringBuilder.IndexOfWholeWord(Text, Word, startpos) : Text.IndexOf(Word, startpos);
                    if (index == -1)
                    {
                        return NotFound();
                    }
                    SetSelection(index, Word.Length);
                textbox.Document.Selection.ScrollIntoView(PointOptions.Start);
                return true;
            }
            else
            {
                try
                {
                    if (!MatchCase)
                    {
                        Text = Text.ToLower();
                        Word = Word.ToLower();
                    }
                    if (SelectionStart == -1)
                    {
                        SelectionStart = 0;
                    }

                    string shortedText = Text.Substring(0, SelectionStart);
                    int index = WholeWord ? Extensions.StringBuilder.LastIndexOfWholeWord(shortedText, Word) : shortedText.LastIndexOf(Word);
                    if (index == -1)
                    {
                        SetSelection(Text.Length, 0);
                        return NotFound();
                    }

                    SetSelection(index, Word.Length);
                    textbox.Document.Selection.ScrollIntoView(PointOptions.Start);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception in TextControlBox --> FindInText:" + "\n" + ex.Message);
                    return false;
                }
            }
        }

        //Go to line
        public bool GoToLine(int line)
        {
            if (appsettings.GetSettingsAsBool("SelectLineAfterGoingToIt", true))
            {
                SelectLine(line);
            }
            else
            {
                textbox.Document.Selection.SetIndex(TextRangeUnit.Paragraph, line, false);
                textbox.Focus(FocusState.Programmatic);
            }
            textbox.Document.Selection.ScrollIntoView(PointOptions.None);
            return true;
        }
        public void SelectLine(int Line)
        {
            textbox.Document.Selection.SetIndex(TextRangeUnit.Paragraph, Line, true);
        }

        //Other
        public new void Focus(FocusState focusstate)
        {
            textbox.Focus(focusstate);
        }
        private void UpdateLineNumber()
        {
            if (GetCurrentLineNumber != OldLineNumber)
            {
                if (SelectionLenght > 0)
                {
                    int linenumber = GetCurrentLineNumber + textbox.Document.Selection.Text.Split(new char[] { '\n', '\r' }).Length;
                    LineNumberchangedEvent?.Invoke(this, linenumber);
                }
                else
                    LineNumberchangedEvent?.Invoke(this, GetCurrentLineNumber);

                OldLineNumber = GetCurrentLineNumber;
            }
        }
        public void ScrollWithArrowKeys(VerticalAxis axis)
        {
            if (MainContentScrollViewer == null)
                return;

            if (axis == VerticalAxis.Up)
            {
                MainContentScrollViewer.ChangeView(null, MainContentScrollViewer.VerticalOffset - 10, null, true);
            }
            if (axis == VerticalAxis.Down)
            {
                MainContentScrollViewer.ChangeView(null, MainContentScrollViewer.VerticalOffset + 10, null, true);
            }
        }

        //Events
        public delegate void TextChanged(TextControlBox sender);
        public delegate void KeyPressed(TextControlBox sender, KeyRoutedEventArgs e);
        public delegate void ZoomChanged(TextControlBox sender, double ZoomFactor);
        public delegate void LineNumberChanged(TextControlBox sender, int CurrentLine);
        public delegate void DocumentTitleChanged(TextControlBox sender, string Header);
        public delegate void Pointer_Pressed(TextControlBox sender, PointerRoutedEventArgs e);
        public delegate void Encoding_Changed(TextControlBox sender, Encoding e);
        public delegate void SaveStatusChanged(TextControlBox sender, bool IsModified);
        public delegate void WordCountChanged(TextControlBox sender, int Words);
        public event KeyPressed KeyPressedEvent;
        public event ZoomChanged ZoomChangedEvent;
        public event LineNumberChanged LineNumberchangedEvent;
        public event DocumentTitleChanged DocumentTitleChangedEvent;
        public event Pointer_Pressed Pointer_PressedEvent;
        public event Encoding_Changed EncodingChangedEvent;
        public event TextChanged TextChangedevent;
        public event WordCountChanged WordCountChangedEvent;
        public event SaveStatusChanged SaveStatusChangedEvent;
    }
    public class MyRichEditBox : RichEditBox
    {
        public DependencyObject FindDependencyObject(string name)
        {
            return this.GetTemplateChild(name);
        }
        //Override the OnKeyDown class to make custom shortcuts and disable defaults
        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            var ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
            var shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);
            if (ctrl.HasFlag(CoreVirtualKeyStates.Down))
            {
                if (e.Key == VirtualKey.Back ||
                    e.Key == VirtualKey.Delete ||
                    e.Key == VirtualKey.Enter ||
                    e.Key == VirtualKey.Right ||
                    e.Key == VirtualKey.Left ||
                    e.Key == VirtualKey.Up ||
                    e.Key == VirtualKey.Down)
                {
                    base.OnKeyDown(e);
                }
                else
                {
                    return;
                }
            }
            else if (e.Key == VirtualKey.Enter && shift.HasFlag(CoreVirtualKeyStates.Down))
            {
                return;
            }
            else
            {
                base.OnKeyDown(e);
            }
        }
    }
    public class SelectionPoint
    {
        public int SelectionStart { get; set; }
        public int SelectionLenght { get; set; }
    }
    public class TextBoxDocument
    {
        public string FilePath { get; set; }
        public string DataBaseName { get; set; }
        public string Header { get; set; }
        public bool IsModified { get; set; }
        public bool IsHandWritingEnabled { get; set; }
        public TabSaveMode TabSaveMode { get; set; }
        public string FileToken { get; set; }
        public StorageFile Storagefile { get; set; }
        public TextWrapping WordWrap { get; set; }
        public string TempFile { get; set; }
        public Encoding Encoding { get; set; }
        public double ZoomFactor { get; set; }
        public bool IsReadonly { get; set; }
        public string InternalTabName { get; set; }
    }
}
