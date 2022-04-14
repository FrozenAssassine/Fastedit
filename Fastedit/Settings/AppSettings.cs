using Fastedit.Core;
using Fastedit.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Convert = Fastedit.Extensions.Convert;

namespace Fastedit
{
    public class AppSettings
    {
        //!! REPLACE "." WITH "/" !!//
        public string GetResourceString(string keyStr)
        {
            var stringout = ResourceLoader.GetForCurrentView().GetString(keyStr);
            return stringout.Length != 0 ? stringout : "NULL";
        }
        public static string GetResourceStringStatic(string keyStr)
        {
            var stringout = ResourceLoader.GetForCurrentView().GetString(keyStr);
            return stringout.Length != 0 ? stringout : "NULL";
        }

        public ElementTheme CurrentApplicationTheme
        {
            get
            {
                return (ElementTheme)Enum.Parse(typeof(ElementTheme), GetSettingsAsString("ThemeIndex", "0"));
            }
        }

        public Brush CreateBrushWithOrWithoutAcrylic(Color color)
        {
            if (GetSettingsAsBool("AcrylicEnabled", true) == false || color.A>=255)
            {
                return new SolidColorBrush(color);
            }
            else
            {
                return new AcrylicBrush
                {
                    TintColor = color,
                    TintOpacity = (double)color.A / 2.55,
                    FallbackColor = color,
                    BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                };
            }
        }

        public void SaveSettings(string Value, object data)
        {
            if (data == null)
                return;

            //cancel if data is a type
            if(data.ToString() == data.GetType().Name)
                return;

            ApplicationData.Current.LocalSettings.Values[Value] = data.ToString();
        }
        public void SaveSettingsAsColor(string Value, Color Color, AccentColors accentcolor = AccentColors.None)
        {
            ApplicationData.Current.LocalSettings.Values[Value] =
                accentcolor != AccentColors.None ? ((int)accentcolor).ToString() : Color.ToString();
        }
        public float GetSettingsAsFloat(string Value, float defaultvalue = 0)
        {
            return Convert.ToFloat(ApplicationData.Current.LocalSettings.Values[Value] as string, defaultvalue);
        }
        public string GetSettings(string Value)
        {
            return ApplicationData.Current.LocalSettings.Values[Value] as string;
        }
        public int GetSettingsAsInt(string Value, int defaultvalue = 0)
        {
            return ApplicationData.Current.LocalSettings.Values[Value] is string value
                ? Convert.ToInt(value, defaultvalue) : defaultvalue;
        }
        public double GetSettingsAsDouble(string Value, double defaultvalue = 0)
        {
            return Convert.ToDouble(ApplicationData.Current.LocalSettings.Values[Value] as string, defaultvalue);
        }
        public bool GetSettingsAsBool(string Value, bool defaultvalue = false)
        {
            return Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values[Value] as string, defaultvalue);
        }
        public string GetSettingsAsString(string Value, string Default = "")
        {
            return ApplicationData.Current.LocalSettings.Values[Value] is string val && val.Length != 0 ? val : Default;
        }
        public Color GetSettingsAsColorWithDefault(string Value, Color Default)
        {
            if (ApplicationData.Current.LocalSettings.Values[Value] is string readColor)
            {
                if (readColor.Length != 0)
                {
                    try
                    {
                        if (readColor.Contains("#"))
                            return (Color)XamlBindingHelper.ConvertValue(typeof(Color), readColor);
                        else if (readColor.Length < 3)
                        {
                            AccentColors clr = (AccentColors)Enum.Parse(typeof(AccentColors), readColor.Trim());
                            if (clr == AccentColors.Dark2)
                                return DefaultValues.SystemAccentColorDark2;
                            else if (clr == AccentColors.Default)
                                return DefaultValues.SystemAccentColor;
                            else if (clr == AccentColors.Light2)
                                return DefaultValues.SystemAccentColorLight2;
                            else if (clr == AccentColors.Light1)
                                return DefaultValues.SystemAccentColorLight1;
                            else if (clr == AccentColors.Dark1)
                                return DefaultValues.SystemAccentColorDark1;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception in AppSettings --> GetSettingsAsColorWithDefault\n" + ex.Message);
                        return Default;
                    }
                }
                return Default;
            }
            else
            {
                return Default;
            }
        }
        public Color GetSettingsAsColor(string Value)
        {
            try
            {
                if (ApplicationData.Current.LocalSettings.Values[Value] is string readColor && readColor.Length != 0)
                {
                    if (readColor.Contains("#"))
                    {
                        return (Color)XamlBindingHelper.ConvertValue(typeof(Color), readColor);
                    }
                    else if (readColor == "0" || readColor == "1" || readColor == "2" || readColor == "3")
                    {
                        AccentColors clr = (AccentColors)Enum.Parse(typeof(AccentColors), readColor.Trim());
                        if (clr == AccentColors.Dark2)
                        {
                            return DefaultValues.SystemAccentColorDark2;
                        }
                        else if (clr == AccentColors.Default)
                        {
                            return DefaultValues.SystemAccentColor;
                        }
                        else if (clr == AccentColors.Light2)
                        {
                            return DefaultValues.SystemAccentColorLight2;
                        }
                        else if (clr == AccentColors.Light1)
                        {
                            return DefaultValues.SystemAccentColorLight1;
                        }
                        else if (clr == AccentColors.Dark1)
                        {
                            return DefaultValues.SystemAccentColorDark1;
                        }
                        else
                        {
                            return Color.FromArgb(255, 0, 0, 0);
                        }
                    }
                    else
                    {
                        return Color.FromArgb(255, 0, 0, 0);
                    }
                }
                else
                {
                    return Color.FromArgb(255, 0, 0, 0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in AppSettings --> GetSettingsAsColor\n" + ex.Message);
                return Color.FromArgb(255, 0, 0, 0);
            }
        }
        public void ClearSettings()
        {
            ApplicationData.Current.LocalSettings.Values.Clear();
        }
    }
}