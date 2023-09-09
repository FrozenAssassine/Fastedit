using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Fastedit.Helper
{
    public class CursorHelper
    {
        public static void SetCursor(CoreCursorType cursor)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(cursor, 0);
        }

        public static void SetArrow()
        {
            SetCursor(CoreCursorType.Arrow);
        }
    }
}
