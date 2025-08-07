using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Flow.Bar.Helper;

public static class Win32Helper
{
    public static bool SetForegroundWindow(Window window)
    {
        return PInvoke.SetForegroundWindow(GetWindowHandle(window));
    }

    public static bool SetForegroundWindow(nint handle)
    {
        return PInvoke.SetForegroundWindow(new(handle));
    }

    internal static HWND GetWindowHandle(Window window, bool ensure = false)
    {
        var windowHelper = new WindowInteropHelper(window);
        if (ensure)
        {
            windowHelper.EnsureHandle();
        }
        return new(windowHelper.Handle);
    }

    public static Window? GetActiveWindow()
    {
        var activeWindow = GetActiveWindowHandle();
        if (activeWindow != HWND.Null)
        {
            return HwndSource.FromHwnd(activeWindow)?.RootVisual as Window;
        }
        return null;
    }

    internal static HWND GetActiveWindowHandle()
    {
        return PInvoke.GetActiveWindow();
    }

    internal static System.Drawing.Point GetCursorPos()
    {
        if (!PInvoke.GetCursorPos(out var pt))
        {
            Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
        }
        return pt;
    }
}
