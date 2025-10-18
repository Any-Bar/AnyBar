using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows;
using System.Windows.Interop;
using AnyBar.Models.Monitor;
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

    #region Window Fullscreen

    private const string WINDOW_CLASS_CONSOLE = "ConsoleWindowClass";
    private const string WINDOW_CLASS_WINTAB = "Flip3D";
    private const string WINDOW_CLASS_PROGMAN = "Progman";
    private const string WINDOW_CLASS_WORKERW = "WorkerW";

    private static HWND _hwnd_shell;
    private static HWND HWND_SHELL =>
        _hwnd_shell != HWND.Null ? _hwnd_shell : _hwnd_shell = PInvoke.GetShellWindow();

    private static HWND _hwnd_desktop;
    private static HWND HWND_DESKTOP =>
        _hwnd_desktop != HWND.Null ? _hwnd_desktop : _hwnd_desktop = PInvoke.GetDesktopWindow();

    public static unsafe bool IsForegroundWindowFullscreen()
    {
        // Get current active window
        var hWnd = PInvoke.GetForegroundWindow();
        if (hWnd.Equals(HWND.Null))
        {
            return false;
        }

        // If current active window is desktop or shell, exit early
        if (hWnd.Equals(HWND_DESKTOP) || hWnd.Equals(HWND_SHELL))
        {
            return false;
        }

        string windowClass;
        const int capacity = 256;
        Span<char> buffer = stackalloc char[capacity];
        int validLength;
        fixed (char* pBuffer = buffer)
        {
            validLength = PInvoke.GetClassName(hWnd, pBuffer, capacity);
        }

        windowClass = buffer[..validLength].ToString();

        // For Win+Tab (Flip3D)
        if (windowClass == WINDOW_CLASS_WINTAB)
        {
            return false;
        }

        PInvoke.GetWindowRect(hWnd, out var appBounds);

        // For console (ConsoleWindowClass), we have to check for negative dimensions
        if (windowClass == WINDOW_CLASS_CONSOLE)
        {
            return appBounds.top < 0 && appBounds.bottom < 0;
        }

        // For desktop (Progman or WorkerW, depends on the system), we have to check
        if (windowClass is WINDOW_CLASS_PROGMAN or WINDOW_CLASS_WORKERW)
        {
            var hWndDesktop = PInvoke.FindWindowEx(hWnd, HWND.Null, "SHELLDLL_DefView", null);
            hWndDesktop = PInvoke.FindWindowEx(hWndDesktop, HWND.Null, "SysListView32", "FolderView");
            if (hWndDesktop != HWND.Null)
            {
                return false;
            }
        }

        var monitorInfo = MonitorInfo.GetNearestDisplayMonitor(hWnd);
        if (monitorInfo is null)
        {
            return false;
        }

        return (appBounds.bottom - appBounds.top) == monitorInfo.Bounds.Height &&
               (appBounds.right - appBounds.left) == monitorInfo.Bounds.Width;
    }

    #endregion
}
