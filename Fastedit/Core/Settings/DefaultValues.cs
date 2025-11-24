using System.IO;
using System.Text;
using Windows.Storage;
using Windows.UI;

namespace Fastedit.Core.Settings
{
    public class DefaultValues
    {
        public static string NewTabTitle = "Untitled";
        public static string NewTabExtension = ".txt";
        public static Encoding Encoding = new UTF8Encoding(false);
        public static bool FastLoadTabs = true;
        public static string DatabasePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Database");
        public static string DesignPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Designs");
        public static string RecycleBinPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Trash");
        public static string TemporaryFilesPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Temp");
        public static int ZoomSteps = 5;
        public static string FontFamily = "Consolas";
        public static int FontSize = 18;
        public static bool ShowLineHighlighter = true;
        public static bool ShowLinenumbers = true;
        public static bool SyntaxHighlighting = true;
        public static bool ShowMenubar = true;
        public static bool ShowStatusbar = true;
        public static int DefaultTabsSpaces = 4; //-1 for tabs, > 0 for spaces
        public static string DefaultDesignName = "Simple_Dark.json";
        public static Color wrongInputColor = Color.FromArgb(255, 255, 0, 0);
        public static Color correctInputColor = Color.FromArgb(255, 0, 255, 0);
        public static string StatusbarSorting = "Zoom:1|LineColumn:1|Encoding:1|FileName:1|WordChar:1|LineEndings:1|TabsSpaces:1";
        public static int MenubarAlignment = 0;
        public static bool HideTitlebar = true;
        public static bool ShowWhitespaceCharacters = true;
        public static bool EnableClickableLinks = true;

        public const int windowWidth = 1100;
        public const int windowHeight = 700;
        public const int windowLeft = -1;
        public const int windowTop = -1;

    }
}
