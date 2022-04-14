using Fastedit.Core;
using Windows.UI.Xaml.Controls;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit.Helper
{
    public class BackgroundHelper
    {
        public static void SetBackgroundToPage(Page page)
        {
            AppSettings appsettings = new AppSettings();
            //Use the Mica background system
            if (appsettings.GetSettingsAsBool("UseMica", false))
            {
                muxc.BackdropMaterial.SetApplyToRootOrPageBackground(page, true);
            }
            //Use the default background system
            else
            {
                muxc.BackdropMaterial.SetApplyToRootOrPageBackground(page, false);
                //App backgroundcolor
                page.Background = appsettings.CreateBrushWithOrWithoutAcrylic(
                    appsettings.GetSettingsAsColorWithDefault("AppBackgroundColor", DefaultValues.DefaultAppBackgroundColor));
            }
        }
    }
}
