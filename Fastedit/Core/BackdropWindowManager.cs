using Fastedit.Helper;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Runtime.InteropServices;
using Windows.UI;
using WinRT;

namespace Fastedit.Core
{
    class WindowsSystemDispatcherQueueHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        struct DispatcherQueueOptions
        {
            internal int dwSize;
            internal int threadType;
            internal int apartmentType;
        }

        [DllImport("CoreMessaging.dll")]
        private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

        object m_dispatcherQueueController = null;
        public void EnsureWindowsSystemDispatcherQueueController()
        {
            if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
            {
                return;
            }

            if (m_dispatcherQueueController == null)
            {
                DispatcherQueueOptions options;
                options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
                options.threadType = 2;    // DQTYPE_THREAD_CURRENT
                options.apartmentType = 2; // DQTAT_COM_STA

                CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
            }
        }
    }

    public class BackdropWindowManager
    {
        private Window window;
        public BackdropWindowManager(Window window)
        {
            this.window = window;

            m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
            m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
        }

        WindowsSystemDispatcherQueueHelper m_wsdqHelper;
        MicaController m_micaController;
        DesktopAcrylicController m_acrylicController;
        SystemBackdropConfiguration m_configurationSource;

        public void SetBackdrop(BackgroundType type, FasteditDesign design)
        {
            if(type != BackgroundType.Solid)
                SetStaticBackground(design, true);

            if (m_micaController != null)
            {
                m_micaController.Dispose();
                m_micaController = null;
            }
            if (m_acrylicController != null)
            {
                m_acrylicController.Dispose();
                m_acrylicController = null;
            }

            window.Activated -= Window_Activated;
            window.Closed -= Window_Closed;
            
            ((FrameworkElement)window.Content).ActualThemeChanged -= Window_ThemeChanged;
            m_configurationSource = null;

            if (type == BackgroundType.Mica)
                TrySetMicaBackdrop();
            if (type == BackgroundType.Acrylic)
                TrySetAcrylicBackdrop(design);
            if (type == BackgroundType.Solid)
                SetStaticBackground(design);
        }

        private void SetupController()
        {
            //hooking up the policy object
            m_configurationSource = new SystemBackdropConfiguration();
            window.Activated += Window_Activated;
            window.Closed += Window_Closed;
            ((FrameworkElement)window.Content).ActualThemeChanged += Window_ThemeChanged;

            m_configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();
        }

        bool TrySetMicaBackdrop()
        {
            if (!MicaController.IsSupported())
                return false;

            SetupController();

            m_micaController = new MicaController();
            m_micaController.AddSystemBackdropTarget(window.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
            m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
            return true;
        }

        bool TrySetAcrylicBackdrop(FasteditDesign design)
        {
            if (!DesktopAcrylicController.IsSupported())
                return false;

            SetupController();

            var color = ConvertHelper.ToColor(design.BackgroundColor);
            m_acrylicController = new DesktopAcrylicController();
            m_acrylicController.TintColor = Color.FromArgb(255, color.R, color.G, color.B);
            m_acrylicController.TintOpacity = color.A / 255.0f;
            m_acrylicController.FallbackColor = color;
            m_acrylicController.AddSystemBackdropTarget(window.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
            m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
            return true;
        }

        private void SetStaticBackground(FasteditDesign design, bool clear = false)
        {
            Grid grid;
            if(window.Content is Page page)
                grid = page.Content as Grid;
            else 
                grid = window.Content as Grid;

            grid.Background = clear ? null : new SolidColorBrush(ConvertHelper.ToColor(design.BackgroundColor));
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            if (m_micaController != null)
            {
                m_micaController.Dispose();
                m_micaController = null;
            }
            if (m_acrylicController != null)
            {
                m_acrylicController.Dispose();
                m_acrylicController = null;
            }
            window.Activated -= Window_Activated;
            m_configurationSource = null;
        }

        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            if (m_configurationSource != null)
            {
                SetConfigurationSourceTheme();
            }
        }

        private void SetConfigurationSourceTheme()
        {
            switch (((FrameworkElement)window.Content).ActualTheme)
            {
                case ElementTheme.Dark: m_configurationSource.Theme = SystemBackdropTheme.Dark; break;
                case ElementTheme.Light: m_configurationSource.Theme = SystemBackdropTheme.Light; break;
                case ElementTheme.Default: m_configurationSource.Theme = SystemBackdropTheme.Default; break;
            }
        }
    }
}
