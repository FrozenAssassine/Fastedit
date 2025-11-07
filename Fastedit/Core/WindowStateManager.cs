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

    public WindowSizePosState GetWindowSizePosStateIndependent()
    {
        //get the window size independent of the state (maximized, minimized, restored).
        //So a minimized window still has a valid size and position and not -32000.
        //Also a maxmimized window would be the size of the screen, but now it is only as big as it was in the restore state.
        return new WindowSizePosState { position = previousWindowPosition, size = previousWindowSize, state = WindowStateHelper.GetWindowState(window) };
    }

    public WindowStateManager(Window window)
    {
        this.window = window;
        this.appWindow = window.AppWindow;

        window.AppWindow.Changed += AppWindow_Changed;
    }

    private PointInt32 previousWindowPosition; 
    private SizeInt32 previousWindowSize; 


    private void AppWindow_Changed(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowChangedEventArgs args)
    {
        if (args.DidPositionChange)
        {
            var state = WindowStateHelper.GetWindowState(window);

            if (state == OverlappedPresenterState.Restored)
            {
                previousWindowSize = appWindow.Size;
                previousWindowPosition = appWindow.Position;
            }
        }
    }
}
