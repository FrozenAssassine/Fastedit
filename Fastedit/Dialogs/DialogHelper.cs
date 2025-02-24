using Fastedit.Helper;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Fastedit.Dialogs
{
    public class DialogHelper
    {
        public static ElementTheme DialogDesign => ThemeHelper.CurrentTheme;
        public static Brush ContentDialogBackground()
        {
            return DesignHelper.CreateBackgroundBrush(ConvertHelper.ToColor(DesignHelper.CurrentDesign.DialogBackgroundColor), DesignHelper.CurrentDesign.DialogBackgroundType);
        }
        public static SolidColorBrush ContentDialogForeground()
        {
            return new SolidColorBrush(ConvertHelper.ToColor(DesignHelper.CurrentDesign.DialogTextColor));
        }
    }
}
