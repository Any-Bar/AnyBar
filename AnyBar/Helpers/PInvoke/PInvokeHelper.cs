using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows;
using System.Windows.Interop;
using Windows.Win32.Foundation;
using Point = System.Drawing.Point;

namespace Windows.Win32;

internal static class PInvokeHelper
{
    #region Foreground Window

    public static bool SetForegroundWindow(Window window)
    {
        return PInvoke.SetForegroundWindow(GetWindowHandle(window));
    }

    public static bool SetForegroundWindow(nint handle)
    {
        return PInvoke.SetForegroundWindow(new(handle));
    }

    public static HWND GetWindowHandle(Window window, bool ensure = false)
    {
        var windowHelper = new WindowInteropHelper(window);
        if (ensure)
        {
            windowHelper.EnsureHandle();
        }
        return new(windowHelper.Handle);
    }

    #endregion

    #region Active Window

    public static Window? GetActiveWindow()
    {
        var activeWindow = GetActiveWindowHandle();
        if (activeWindow != HWND.Null)
        {
            return HwndSource.FromHwnd(activeWindow)?.RootVisual as Window;
        }
        return null;
    }

    public static HWND GetActiveWindowHandle()
    {
        return PInvoke.GetActiveWindow();
    }

    #endregion

    #region Cursor Position

    public static Point GetCursorPos()
    {
        if (!PInvoke.GetCursorPos(out var pt))
        {
            Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
        }
        return pt;
    }

    #endregion

    #region Administrator Mode

    public static bool IsAdministrator()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    #endregion
}
