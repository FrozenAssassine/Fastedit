using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Fastedit.Helper
{
    public class ConvertHelper
    {
        public static Visibility BoolToVisibility(bool visible)
        {
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }
        public static double ToDouble(object value, double defaultValue = 0)
        {
            if (value != null)
            {
                if (double.TryParse(value.ToString(), out double Converted) == true)
                    return Converted;
            }
            return defaultValue;
        }

        public static int ToInt(object value, int defaultValue = 0)
        {
            if (value != null)
            {
                if (int.TryParse(value.ToString(), out int converted) == true)
                    return converted;
            }
            return defaultValue;
        }

        public static bool ToBoolean(object value, bool defaultValue = false)
        {
            if (value != null)
            {
                if (bool.TryParse(value.ToString(), out bool converted) == true)
                    return converted;
            }
            return defaultValue;
        }

        public static float ToFloat(object value, float defaultValue = 0)
        {
            if (value != null)
            {
                if (float.TryParse(value.ToString(), out float converted) == true)
                    return converted;
            }
            return defaultValue;
        }

        public static Color ToColor(SolidColorBrush clr)
        {
            if (clr == null)
                return Color.FromArgb(0, 0, 0, 0);
            return clr.Color;
        }

        public static Color ToColor(Brush brush, Color? defaultValue = null)
        {
            if (brush == null)
                return defaultValue ?? Color.FromArgb(0, 0, 0, 0);

            if (brush is SolidColorBrush sbr)
                return sbr.Color;
            else if (brush is AcrylicBrush abr)
                return abr.TintColor;
            return defaultValue ?? Color.FromArgb(0, 0, 0, 0);
        }
        public static Color ToColor(Color? color)
        {
            return color ?? Color.FromArgb(0, 0, 0, 0);
        }
        public static Color GetColorFromTheme(ElementTheme theme)
        {
            if (theme == ElementTheme.Dark)
                return Color.FromArgb(255, 0, 0, 0);
            else
                return Color.FromArgb(255, 255, 255, 255);
        }
        public static string ToString(object value)
        {
            if (value == null)
                return "";
            return value.ToString();
        }
    }
}
