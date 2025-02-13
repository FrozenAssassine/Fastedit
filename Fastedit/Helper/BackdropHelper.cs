using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using Windows.UI;
using WinRT;
using WinUIEx;

namespace Fastedit.Helper;

internal class BackdropHelper
{
    public static SystemBackdropConfiguration currentBackdropConfig = null;

    public static void SetAcrylicBackdrop(Window window, Color color, ElementTheme theme)
    {
        if (DesktopAcrylicController.IsSupported())
        {
            var acrylicController = new DesktopAcrylicController();
            var backdropConfig = new SystemBackdropConfiguration();

            currentBackdropConfig = backdropConfig;

            backdropConfig.IsInputActive = true;
            backdropConfig.Theme = theme == ElementTheme.Light ? 
                SystemBackdropTheme.Light : 
                theme == ElementTheme.Dark ? 
                SystemBackdropTheme.Dark : 
                SystemBackdropTheme.Default;

            acrylicController.TintColor = color;
            acrylicController.TintOpacity = color.A / 255.0f;
            acrylicController.FallbackColor = color;

            acrylicController.AddSystemBackdropTarget(window.As<ICompositionSupportsSystemBackdrop>());
            acrylicController.SetSystemBackdropConfiguration(backdropConfig);
        }
    }

    public static bool TrySetMicaBackdrop(Window window, bool useMicaAlt = true)
    {
        if (MicaController.IsSupported())
        {
            Debug.WriteLine("Set Mica");
            Microsoft.UI.Xaml.Media.MicaBackdrop micaBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            micaBackdrop.Kind = useMicaAlt ? MicaKind.BaseAlt : MicaKind.Base;
            window.SystemBackdrop= micaBackdrop;
            return true;
        }
        return false;
    }
    public static void SetStaticBackdrop(Window window, Color color)
    {
        window.SystemBackdrop = new StaticBackdrop(color);
    }
}

public class StaticBackdrop : CompositionBrushBackdrop
{
    private Color color;
    public StaticBackdrop(Color color)
    {
        this.color = color;
    }
    protected override Windows.UI.Composition.CompositionBrush CreateBrush(Windows.UI.Composition.Compositor compositor)
    {
        return compositor.CreateColorBrush(color);
    }
}

