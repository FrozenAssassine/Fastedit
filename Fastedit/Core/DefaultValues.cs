using System;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace Fastedit.Core
{
    public class DefaultValues
    {
        //MainPage
        public static string LocalFolderPath = ApplicationData.Current.LocalFolder.Path.ToString();
        public static int AutoSaveTempFileMinutes = 4;
        public static int AutoBackupDataBaseMinutes = 4;
        public static CornerRadius DefaultDialogCornerRadius = new CornerRadius(5);
        public static bool LoadRecentTabsOnStart = true;
        public static SolidColorBrush WrongInput_Color = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        public static SolidColorBrush CorrectInput_Color = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
        public static string DefaultThemeName = "Acrylic-Dark.fasteditdesign";
        public static string DefaultWindows11ThemeName = "Mica-Accentcolor.fasteditdesign";
        public static int TextBoxInfoToast_Time = 3; //How long the infotoast is displayed on the textbox until it gets closed

        //Tab Actions
        public static CornerRadius DefaultTabCornerRadius = new CornerRadius(5, 5, 0, 0);
        public static double DefaultZoomFactor = 0.2;
        public static int MaxZoom = 300;
        public static int MinZoom = 20;
        public static int MinFontWithZoom = 2;
        public static int MaxFontWithZoom = 120;
        public static string NewDocName = "Untitled.txt";
        public static bool HandWritingEnabled = false;
        public static string DefaultTabSize = "\t";
        public static string DefaultTabIconId = "\uE8A5";

        //SetttingsPage
        public static bool SpellCheckingEnabled = false;
        public static int DefaultFontsize = 16;
        public static int defaultTabSizeMode = 0; //0-2
        public static string DefaultFontFamily = "Consolas";

        public static string CustomDesigns_FolderName = "Designs";
        public static string Database_FolderName = "DataBase";
        public static string RecycleBin_FolderName = "RecycleBin";
        public static string Backup_FolderName = "Backup";
        public static string Extension_FasteditDesign = ".fasteditdesign";

        //Colors
        public static Color SystemAccentColor
        {
            get
            {
                return (Color)XamlBindingHelper.ConvertValue(typeof(Color), Application.Current.Resources["SystemAccentColor"].ToString());
            }
        }
        public static Color SystemAccentColorLight1
        {
            get
            {
                return (Color)XamlBindingHelper.ConvertValue(typeof(Color), Application.Current.Resources["SystemAccentColorLight1"].ToString());
            }
        }
        public static Color SystemAccentColorDark2
        {
            get
            {
                return (Color)XamlBindingHelper.ConvertValue(typeof(Color), Application.Current.Resources["SystemAccentColorDark2"].ToString());
            }
        }
        public static Color SystemAccentColorDark1
        {
            get
            {
                return (Color)XamlBindingHelper.ConvertValue(typeof(Color), Application.Current.Resources["SystemAccentColorDark1"].ToString());
            }
        }
        public static Color SystemAccentColorLight2
        {
            get
            {
                try
                {
                    var color = Application.Current.Resources["SystemAccentColorLight2"];
                    if (color != null)
                    {
                        return (Color)XamlBindingHelper.ConvertValue(typeof(Color), Application.Current.Resources["SystemAccentColorLight2"].ToString());
                    }
                    return Color.FromArgb(0, 0, 0, 0);
                }
                catch
                {
                    return Color.FromArgb(0, 0, 0, 0);
                }
            }
        }

        public static Color SetColorTransparency(Color clr, int Transparency)
        {
            return Color.FromArgb((byte)Transparency, clr.R, clr.G, clr.B);
        }

        public static Color DefaultTextColor = SystemAccentColorLight2;
        public static Color DefaultTextSelectionColor = SystemAccentColor;
        public static Color DefaultTextBackgroundColor = Color.FromArgb(0, 0, 0, 0);
        public static Color DefaultCommandBarBackgroundColor = SystemAccentColorLight2;
        public static Color DefaultTabColorNotFocused = Color.FromArgb(0, 0, 0, 0);
        public static Color DefaultTabColorFocused
        {
            get
            {
                return Color.FromArgb(120, SystemAccentColorDark2.R, SystemAccentColorDark2.G, SystemAccentColorDark2.B); ;
            }
        }

        public static Color DefaultAppBackgroundColor = SystemAccentColor;
        public static Color DefaultTitleBarBackgroundColor = SystemAccentColor;
        public static Color DefaultLineNumberForegroundColor = Colors.LightGray;
        public static Color DefaultLineNumberBackgroundColor = Color.FromArgb(10, 0, 0, 0);
        public static Color DefaultDialogBackground = SystemAccentColor;
        public static Color DefaultStatusbarForegroundColor = Color.FromArgb(255, 255, 255, 255);
        public static Color DefaultStatusbarBackgroundColor = Color.FromArgb(100, 0, 0, 0);

        public static Brush ContentDialogBackgroundColor()
        {
            AppSettings appsettings = new AppSettings();
            return appsettings.CreateBrushWithOrWithoutAcrylic(
                appsettings.GetSettingsAsColorWithDefault("DialogBackgroundColor", DefaultDialogBackground));
        }
        public static SolidColorBrush ContentDialogForegroundColor()
        {
            AppSettings appsettings = new AppSettings();
            return new SolidColorBrush(Extensions.Convert.WhiteOrBlackFromColorBrightness(
                appsettings.GetSettingsAsColorWithDefault("DialogBackgroundColor", DefaultDialogBackground)
                ));
        }
        public static ElementTheme ContentDialogTheme()
        {
            AppSettings appsettings = new AppSettings();
            return (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsInt("ThemeIndex", 0).ToString());
        }
    }
}
