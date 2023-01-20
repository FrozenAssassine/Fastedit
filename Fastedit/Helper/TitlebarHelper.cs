using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Helper
{
    public class TitlebarHelper
    {
        Grid CustomDragRegion, ShellTitlebarInset = null;
        FlowDirection flowDirection;

        public TitlebarHelper(Grid CustomDragRegion, Grid ShellTitlebarInset, FlowDirection flowDirection)
        {
            this.CustomDragRegion = CustomDragRegion;
            this.ShellTitlebarInset = ShellTitlebarInset;
            this.flowDirection = flowDirection;
        }

        public void SetTitlebar()
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            Window.Current.SetTitleBar(CustomDragRegion);

            ApplicationView appView = ApplicationView.GetForCurrentView();
            appView.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            appView.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (flowDirection == FlowDirection.LeftToRight)
            {
                CustomDragRegion.MinWidth = sender.SystemOverlayRightInset;
                if (ShellTitlebarInset != null)
                    ShellTitlebarInset.MinWidth = sender.SystemOverlayLeftInset;
            }
            else
            {
                CustomDragRegion.MinWidth = sender.SystemOverlayLeftInset;
                if (ShellTitlebarInset != null)
                    ShellTitlebarInset.MinWidth = sender.SystemOverlayRightInset;
            }
            CustomDragRegion.Height = sender.Height;

            if (ShellTitlebarInset != null)
                ShellTitlebarInset.Height = CustomDragRegion.Height;

        }

    }
}
