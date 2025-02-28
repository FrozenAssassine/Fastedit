using Microsoft.UI.Xaml.Controls;

namespace Fastedit.Controls;
public sealed partial class SetingsItemSeparator : UserControl
{
    public SetingsItemSeparator()
    {
        this.InitializeComponent();
    }

    public string Header { get; set; }
}
