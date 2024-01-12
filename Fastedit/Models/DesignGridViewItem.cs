using Windows.UI.Xaml.Media;

namespace Fastedit.Models
{
    public class DesignGridViewItem
    {
        public Brush AppBackground { get; set; }
        public Brush TextColor { get; set; }
        public Brush LineNumberColor { get; set; }
        public Brush TextBoxBackground { get; set; }
        public Brush LineNumberBackground { get; set; }
        public Brush TabPageBackground { get; set; }
        public string DesignName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
