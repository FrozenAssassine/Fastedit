using Fastedit.Helper;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Markup;

namespace Fastedit.Settings
{
    public class AppSettings
    {
        public static void SaveSettings(string Value, object data)
        {
            if (data == null)
                return;

            //cancel if data is a type
            if (data.ToString() == data.GetType().Name)
                return;

            ApplicationData.Current.LocalSettings.Values[Value] = data.ToString();
        }
        public static string GetSettings(string value, string defaultValue = "")
        {
            return ApplicationData.Current.LocalSettings.Values[value] as string ?? defaultValue;
        }
        public static int GetSettingsAsInt(string value, int defaultValue = 0)
        {
            return ConvertHelper.ToInt(ApplicationData.Current.LocalSettings.Values[value] as string, defaultValue);
        }
        public static bool GetSettingsAsBool(string value, bool defaultValue = false)
        {
            return ConvertHelper.ToBoolean(ApplicationData.Current.LocalSettings.Values[value] as string, defaultValue);
        }
        public static Color GetSettingsAsColor(string value, Color? defaultValue)
        {
            string readColor = ApplicationData.Current.LocalSettings.Values[value] as string;

            if (readColor.Contains("#"))
            {
                return (Color)XamlBindingHelper.ConvertValue(typeof(Color), readColor);
            }
            return defaultValue ?? Color.FromArgb(0, 0, 0, 0);
        }
    }
}
