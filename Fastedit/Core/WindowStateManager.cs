using Fastedit.Helper;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using Windows.Graphics;

namespace Fastedit.Core;

public class WindowSizePosState
{
    public OverlappedPresenterState state;
    public PointInt32 position;
    public SizeInt32 size;
}

public class WindowStateManager
{
    public bool IsMinimized { get; private set; }
    private Window window;
    private AppWindow appWindow;

    private PointInt32 previousWindowPosition;
    private SizeInt32 previousWindowSize;

    public WindowStateManager(Window window)
    {
        this.window = window;
        this.appWindow = window.AppWindow;

        // 1. Initial mit den aktuellen Werten belegen
        previousWindowPosition = appWindow.Position;
        previousWindowSize = appWindow.Size;

        window.AppWindow.Changed += AppWindow_Changed;
    }

    private void AppWindow_Changed(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowChangedEventArgs args)
    {
        // 2. Auf Größen- UND Positionsänderungen reagieren
        if (args.DidPositionChange || args.DidSizeChange)
        {
            var state = WindowStateHelper.GetWindowState(window);

            if (state == OverlappedPresenterState.Restored)
            {
                previousWindowSize = appWindow.Size;
                previousWindowPosition = appWindow.Position;
            }
        }
    }

    public WindowSizePosState GetWindowSizePosStateIndependent()
    {
        return new WindowSizePosState
        {
            position = previousWindowPosition,
            size = previousWindowSize,
            state = WindowStateHelper.GetWindowState(window)
        };
    }
}