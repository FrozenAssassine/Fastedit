using System;
using System.Runtime.InteropServices;

namespace Fastedit.Helper;

internal class InteropHelper
{
    [DllImport("User32.dll")]
    public static extern Int32 SetForegroundWindow(IntPtr hWnd);

    [DllImport("User32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("User32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("User32.dll")]
    public static extern bool AllowSetForegroundWindow(int dwProcessId);

    public const int SW_RESTORE = 9;
    public const int ASFW_ANY = -1;
}
