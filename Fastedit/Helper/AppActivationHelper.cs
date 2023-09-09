using Fastedit.Tab;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Navigation;

namespace Fastedit.Helper
{
    public class AppActivationHelper
    {
        public static NavigationEventArgs NavigationEvent = null;

        public static async Task<bool> HandleAppActivation(TabView tabView)
        {
            if (NavigationEvent.Parameter == null)
                return false;

            if (NavigationEvent.Parameter is IActivatedEventArgs args)
            {
                if (args.Kind == ActivationKind.Launch)
                    return true;
                else if (args.Kind == ActivationKind.File)
                {
                    var handler = args as FileActivatedEventArgs;
                    if (handler == null)
                        return false;

                    return await TabPageHelper.OpenFiles(tabView, handler.Files);
                }
            }
            return false;
        }
    }
}
