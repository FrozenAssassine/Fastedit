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
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        this.SelectedLineEnding = (LineEnding)lineEndingCombobox.SelectedIndex;
    }    
}
