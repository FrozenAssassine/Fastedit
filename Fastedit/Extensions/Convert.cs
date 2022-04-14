using Fastedit.Core;
using System;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace Fastedit.Extensions
{
    public static class Convert
    {
        public static Color GetColorFromTheme(ElementTheme theme)
        {
            if (theme == ElementTheme.Dark)
                return Color.FromArgb(255, 0, 0, 0);
            else
                return Color.FromArgb(255, 255, 255, 255);
        }
        public static Color GetColorFromThemeReversed(ElementTheme theme)
        {
            if (theme == ElementTheme.Light)
                return Color.FromArgb(255, 0, 0, 0);
            else
                return Color.FromArgb(255, 255, 255, 255);
        }
        public static Visibility BoolToVisibility(bool visible)
        {
            if (visible)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public static Color WhiteOrBlackFromColorBrightness(Color input)
        {
            if (input.R + input.G + input.B >= (127 * 3)) //>=381:
            {
                return Color.FromArgb(255, 0, 0, 0);
            }
            else
            {
                return Color.FromArgb(255, 255, 255, 255);
            }
        }

        public static ElementTheme ThemeFromColorBrightness(Color input, AppSettings appsettings = null)
        {
            if (appsettings == null)
                appsettings = new AppSettings();

            if (appsettings.GetSettingsAsBool("UseMica", false) == true)
                return ThemeHelper.RootTheme;
            else
            {
                int midval = input.R + input.G + input.B;
                if (midval >= (127 * 3)) //>=381:
                    return ElementTheme.Light;
                else
                    return ElementTheme.Dark;
            }
        }

        public static Color ToColor(object clr, Color Default)
        {
            try
            {
                if (clr == null) return Default;

                if (clr is SolidColorBrush) { return (clr as SolidColorBrush).Color; }
                else if (clr is AcrylicBrush) { return (clr as AcrylicBrush).TintColor; }
                else if (clr is Color color) { return color; }
                else if (Convert.ToInt(clr, -1) <= 5 && Convert.ToInt(clr, -1) >= 0)
                {
                    AccentColors accentColor = (AccentColors)Enum.Parse(typeof(AccentColors), clr.ToString());
                    if (accentColor == AccentColors.Dark2)
                        return DefaultValues.SystemAccentColorDark2;
                    else if (accentColor == AccentColors.Default)
                        return DefaultValues.SystemAccentColor;
                    else if (accentColor == AccentColors.Light2)
                        return DefaultValues.SystemAccentColorLight2;
                    else if (accentColor == AccentColors.Light1)
                        return DefaultValues.SystemAccentColorLight1;
                    else if (accentColor == AccentColors.Dark1)
                        return DefaultValues.SystemAccentColorDark1;
                    else
                        return Default;
                }
                else { return (Color)XamlBindingHelper.ConvertValue(typeof(Color), clr); }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in Convert --> ToColor\n" + ex.Message);
                return Default;
            }
        }

        public static double ToDouble(object Value, double Default = 0.0)
        {
            if (Value != null)
            {
                double Converted;
                if (double.TryParse(Value.ToString(), out Converted) == true)
                    return Converted;
            }
            return Default;
        }

        public static int ToInt(object Value, int Default = 0)
        {
            if (Value != null)
            {
                int Converted;
                if (int.TryParse(Value.ToString(), out Converted) == true)
                    return Converted;
            }
            return Default;
        }

        public static bool ToBoolean(object Value, bool Default = false)
        {
            bool Converted;
            if (Value != null)
            {
                if (bool.TryParse(Value.ToString(), out Converted) == true)
                    return Converted;

            }
            return Default;
        }

        public static float ToFloat(object Value, float Default)
        {
            float Converted;
            if (Value != null)
            {
                if (float.TryParse(Value.ToString(), out Converted) == true)
                    return Converted;
            }
            return Default;
        }

        public static Color ToColor(SolidColorBrush clr)
        {
            return clr.Color;
        }
    }
}
