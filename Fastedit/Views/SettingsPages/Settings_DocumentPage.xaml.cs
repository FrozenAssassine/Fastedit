using Microsoft.Graphics.Canvas.Text;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Fastedit.Core.Settings;


namespace Fastedit.Views.SettingsPages;

public sealed partial class Settings_DocumentPage : Page
{
    public string[] Fonts
    {
        get
        {
            return CanvasTextFormat.GetSystemFontFamilies().OrderBy(f => f).ToArray();
        }
    }

    public Settings_DocumentPage()
    {
        this.InitializeComponent();

        var value = AppSettings.SpacesPerTab;
        SpacesPerTabSlider.Value = value == -1 ? DefaultValues.NumberOfSpacesPerTab : value;
        SpacesOrTabsSwitch.IsOn = AppSettings.UseSpacesInsteadTabs;

        FontFamilyCombobox.SelectedItem = AppSettings.FontFamily;
        FontSizeNumberBox.Value = AppSettings.FontSize;

        ShowLinenumbersSwitch.IsOn = AppSettings.ShowLineNumbers;
        EnableSyntaxhighlightingSwitch.IsOn = AppSettings.SyntaxHighlighting;
        ShowLinehighlighterSwitch.IsOn = AppSettings.ShowLineHighlighter;

        ShowWhitespaceCharacters.IsOn = AppSettings.ShowWhitespaceCharacters;
    }

    private void SpacesOrTabsSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        AppSettings.UseSpacesInsteadTabs = SpacesOrTabsSwitch.IsOn;
    }
    private void SpacesPerTabSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        AppSettings.SpacesPerTab = (int)SpacesPerTabSlider.Value;
    }

    private void FontFamilyCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        AppSettings.FontFamily = FontFamilyCombobox.SelectedItem.ToString();
    }

    private void FontSizeNumberBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
    {
        AppSettings.FontSize = (int)FontSizeNumberBox.Value;
    }
    private void ShowLinenumbersSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        AppSettings.ShowLineNumbers = ShowLinenumbersSwitch.IsOn;
    }

    private void ShowLinehighlighterSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        AppSettings.ShowLineHighlighter = ShowLinehighlighterSwitch.IsOn;
    }

    private void EnableSyntaxhighlightingSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        AppSettings.SyntaxHighlighting = EnableSyntaxhighlightingSwitch.IsOn;
    }

    private void ShowWhitespaceCharacters_Toggled(object sender, RoutedEventArgs e)
    {
        AppSettings.ShowWhitespaceCharacters = ShowWhitespaceCharacters.IsOn;
    }
}
