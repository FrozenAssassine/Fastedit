using Windows.UI;
using Microsoft.UI.Xaml;

namespace Fastedit.Helper;

public class ConvertHelper
{
    public static Visibility BoolToVisibility(bool visible)
    {
        return visible ? Visibility.Visible : Visibility.Collapsed;
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


    public static Color ToColor(Color? color)
    {
        return color.HasValue ? color.Value : Color.FromArgb(0, 0, 0, 0);
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
