namespace Fastedit.Extensions
{
    public enum TabSaveMode
    {
        SaveAsTemp = 0,
        SaveAsFile = 1,
        SaveAsDragDrop = 2,
    }

    public enum AccentColors
    {
        None,
        Light2,
        Default,
        Dark2,
        Light1,
        Dark1,
    }

    public class ScrollbarPositions
    {
        public double ScrollbarPositionHorizontal { get; set; }
        public double ScrollbarPositionVertical { get; set; }
    }

    public enum Axis
    {
        X,
        Y
    }

    public enum VerticalAxis
    {
        Up,
        Down
    }

    public enum KeyModifiers
    {
        Control = 0,
        Windows = 1,
        Menu = 2,
        Shift = 3,
        None = 4
    }
    public enum WindowsVersion
    {
        Windows10 ,
        Windows11 ,
    }
}