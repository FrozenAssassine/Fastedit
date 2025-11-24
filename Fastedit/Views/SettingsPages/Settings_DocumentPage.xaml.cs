using Microsoft.Graphics.Canvas.Text;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Fastedit.Core.Settings;
using Fastedit.Helper;


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

    public string[] LineEndings
    {
        get => LineEndingHelper.GetLineEndings().Select(item => item.ToString()).ToArray();
    }

    public Settings_DocumentPage()
    {
        this.InitializeComponent();


        int tabsSpaces = AppSettings.TabsSpacesMode;
        TabsSpacesSelectorCombobox.SelectedItem = 
            TabsSpacesSelectorCombobox.Items.First(
                item => ConvertHelper.ToInt((item as ComboBoxItem).Tag.ToString(), DefaultValues.DefaultTabsSpaces) == tabsSpaces
                );

        FontFamilyCombobox.SelectedItem = AppSettings.FontFamily;
        FontSizeNumberBox.Value = AppSettings.FontSize;

        ShowLinenumbersSwitch.IsOn = AppSettings.ShowLineNumbers;
        EnableSyntaxhighlightingSwitch.IsOn = AppSettings.SyntaxHighlighting;
        ShowLinehighlighterSwitch.IsOn = AppSettings.ShowLineHighlighter;

        ShowWhitespaceCharacters.IsOn = AppSettings.ShowWhitespaceCharacters;
        EnableClickableLinks.IsOn = AppSettings.EnableClickableLinks;

        LineEndingSelectorCombobox.SelectedIndex = AppSettings.DefaultLineEnding.GetHashCode();

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

    private void EnableClickableLinks_Toggled(object sender, RoutedEventArgs e)
    {
        AppSettings.EnableClickableLinks = EnableClickableLinks.IsOn;
    }

    private void TabsSpacesSelectorCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        AppSettings.TabsSpacesMode = ConvertHelper.ToInt((TabsSpacesSelectorCombobox.SelectedItem as ComboBoxItem).Tag, DefaultValues.DefaultTabsSpaces);
    }

    private void LineEndingSelectorCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        AppSettings.DefaultLineEnding = (TextControlBoxNS.LineEnding)LineEndingSelectorCombobox.SelectedIndex;
    }
}
