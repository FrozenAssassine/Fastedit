using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Fastedit.Controls
{
    public sealed partial class SetingsItemSeparator : UserControl
    {
        public SetingsItemSeparator()
        {
            this.InitializeComponent();
        }

        public string Header { get; set; }
    }
}
