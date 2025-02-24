using Microsoft.UI.Xaml;

namespace Fastedit.Helper
{
    public class ThemeHelper
    {
        public static ElementTheme _RequestedTheme = ElementTheme.Default;

        public static ElementTheme CurrentTheme
        {
            get
            {
                if (App.m_window.Content is FrameworkElement frame)
                    return frame.RequestedTheme;
                return ElementTheme.Default;
            }
            set
            {
                if (App.m_window.Content is FrameworkElement frame)
                    frame.RequestedTheme = value;
            }
        }

    }
}
