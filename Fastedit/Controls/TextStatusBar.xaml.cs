using Fastedit.Core.Tab;
using Fastedit.Dialogs;
using Fastedit.Helper;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Diagnostics;

namespace Fastedit.Controls;

public sealed partial class TextStatusBar : UserControl
{
    public Dictionary<string, StatusbarItem> StatusbarSortingNames;

    public TabPageItem tabPage { get; set; }
    private bool _IsVisible = true;
    public bool IsVisible { get => _IsVisible; set { _IsVisible = value; this.Visibility = ConvertHelper.BoolToVisibility(value); } }

    public TextStatusBar()
    {
        this.InitializeComponent();

        foreach (var encoding in EncodingHelper.AllEncodings)
        {
            var item = new MenuFlyoutItem { Text = encoding.name, Tag = encoding.encoding };
            item.Click += ChangeEncoding_Click;
            EncodingFlyout.Items.Add(item);
        }

        StatusbarSortingNames = new()
        {
            { "Zoom", ItemZoom },
            { "LineColumn", ItemLineColumn },
            { "Encoding", ItemEncoding },
            { "FileName", ItemFileName },
            { "WordChar", ItemWordCharacter }
        };
    }
    public void ToggleItemVisibility(SolidColorBrush textColor)
    {
        //show them all (fallback)
        foreach (var item in StatusbarSortingNames.Values)
        {
            item.Visibility = Visibility.Visible;
            item.Foreground = textColor;
        }

        var sorting = AppSettings.StatusbarSorting.Split("|", System.StringSplitOptions.RemoveEmptyEntries);
        foreach(var item in sorting)
        {
            var splitted = item.Split(":");
            if (splitted.Length != 2)
                continue;

            if (StatusbarSortingNames.TryGetValue(splitted[0].Trim(), out StatusbarItem statusBarItem))
                statusBarItem.Visibility = ConvertHelper.BoolToVisibility(splitted[1] == "1");
        }
    }

    public void UpdateSelectionChanged()
    {
        if (!IsVisible)
            return;

        if (tabPage == null)
        {
            ItemLineColumn.ChangingText = "-";
            return;
        }

        ItemLineColumn.CustomText = $"Ln: {tabPage.textbox.CurrentLineIndex + 1}, Cl: {tabPage.textbox.CursorPosition.CharacterPosition}";
    }

    public void UpdateText()
    {
        if (!IsVisible)
            return;

        if (tabPage == null)
        {
            ItemWordCharacter.ChangingText = "-";
            return;
        }
        int charCount = tabPage.textbox.CharacterCount();
        ItemWordCharacter.CustomText = (charCount > 5_000_000 ? "" : $"W: {tabPage.CountWords()}, ") + $"C: {charCount}";
    }
    public void UpdateZoom()
    {
        if (!IsVisible)
            return;

        if (tabPage == null)
        {
            ItemZoom.ChangingText = "-";
            return;
        }

        ItemZoom.ChangingText = tabPage.textbox.ZoomFactor.ToString();
    }
    public void UpdateEncoding()
    {
        if (!IsVisible)
            return;

        if (tabPage == null)
        {
            ItemEncoding.ChangingText = "-";
            return;
        }

        ItemEncoding.ChangingText = EncodingHelper.AllEncodings[EncodingHelper.GetIndexByEncoding(tabPage.Encoding)].name;
    }
    public void UpdateFile()
    {
        if (!IsVisible)
            return;

        if (tabPage == null)
        {
            ItemFileName.ChangingText = "-";
            return;
        }

        ItemFileName.ChangingText = tabPage.DatabaseItem.FileName;
    }

    public void UpdateAll()
    {
        UpdateSelectionChanged();
        UpdateZoom();
        UpdateEncoding();
        UpdateFile();
        UpdateText();
    }
    private void ZoomSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (tabPage == null)
            return;

        tabPage.textbox.ZoomFactor = (int)zoomSlider.Value;
    }

    private void ChangeEncoding_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (tabPage == null)
            return;

        tabPage.Encoding = (System.Text.Encoding)(sender as MenuFlyoutItem).Tag;

        UpdateEncoding();
    }

    private void Statusbar_Zoom_FlyoutOpening()
    {
        if (tabPage == null)
            return;

        zoomSlider.Value = tabPage.textbox.ZoomFactor;
    }

    private void ResetZoom_DoubleClicked(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        //reset
        zoomSlider.Value = 100;
    }

    private void ItemZoom_PointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        zoomSlider.Value += e.GetCurrentPoint(sender as UIElement).Properties.MouseWheelDelta / 2;
    }

    private async void ItemFileName_StatusbarItemClick(StatusbarItem sender, Button children)
    {
        if (this.tabPage == null)
            return;

        await RenameFileDialog.Show(this.tabPage);

        UpdateFile();
    }
}
