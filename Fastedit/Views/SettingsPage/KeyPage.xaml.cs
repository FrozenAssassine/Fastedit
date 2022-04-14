using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Fastedit.Views.SettingsPage
{
    public sealed partial class KeyPage : Page
    {
        private AppSettings appsettings = new AppSettings();
        private Thickness ItemThickness = new Thickness(20, 5, 0, 0);

        public KeyPage()
        {
            this.InitializeComponent();
            RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsString("ThemeIndex", "0"));
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }
        private void Page_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //Navigate between Pages with mouse button 4/5
            Windows.UI.Input.PointerPoint currentPoint = e.GetCurrentPoint(this);
            if (currentPoint.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                Windows.UI.Input.PointerPointProperties pointerProperties = currentPoint.Properties;

                if (pointerProperties.IsXButton1Pressed && Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
                else if (pointerProperties.IsXButton2Pressed && Frame.CanGoForward)
                {
                    Frame.GoForward();
                }
            }
        }
    }
}
