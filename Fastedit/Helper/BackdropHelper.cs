using Microsoft.UI.Xaml;

namespace Fastedit.Helper;

internal class BackdropHelper
{
    public static bool TrySetMicaBackdrop(Window window, bool useMicaAlt = false)
    {
        if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
        {
            Microsoft.UI.Xaml.Media.MicaBackdrop micaBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            micaBackdrop.Kind = useMicaAlt ? Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt : Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base;
            window.SystemBackdrop= micaBackdrop;
            return true;
        }
        return false;
    }
    public static bool TrySetAcrylicBackdrop(Window window)
    {
        if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
        {
            Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop DesktopAcrylicBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
            window.SystemBackdrop = DesktopAcrylicBackdrop;
            return true;
        }

        return false;
    }
}
