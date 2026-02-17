using System;
using System.Runtime.InteropServices;

namespace Fastedit.Helper;

internal class InteropHelper
{
    [DllImport("User32.dll")]
    public static extern Int32 SetForegroundWindow(IntPtr hWnd);
}
