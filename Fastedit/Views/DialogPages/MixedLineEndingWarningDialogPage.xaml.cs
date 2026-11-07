using Microsoft.UI.Xaml.Controls;
using System;
using TextControlBoxNS;

namespace Fastedit.Views.DialogPages;

public sealed partial class MixedLineEndingWarningDialogPage : Page
{
    private string[] LineEndingStrings
    {
        get
        {
            return Enum.GetNames(typeof(LineEnding));
        }
    }

    public LineEnding SelectedLineEnding;


    public MixedLineEndingWarningDialogPage()
    {
        InitializeComponent();
        warningTextBlock1.Text = Multilingual.L("MixedLineEndingWarningDialogPage_TextBlock_1.Text");
        warningTextBlock2.Text = Multilingual.L("MixedLineEndingWarningDialogPage_TextBlock_2.Text");
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        this.SelectedLineEnding = (LineEnding)lineEndingCombobox.SelectedIndex;
    }    
}
