using Windows.UI.Xaml;

namespace Fastedit.Helper
{
    public class ThemeHelper
    {
        public static ElementTheme CurrentTheme
        {
            set
            {
                if (Window.Current.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = value;
                }
            }
            get
            {
                if (Window.Current.Content is FrameworkElement rootElement)
                    return rootElement.RequestedTheme;
                return ElementTheme.Default;
            }
        }
    }
}
