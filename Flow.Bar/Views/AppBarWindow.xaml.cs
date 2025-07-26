using Flow.Bar.Models;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Flow.Bar.Views;

public partial class AppBarWindow : Window
{
    private HWND HWND;
    private HwndSource? _hwndSource;

    private bool IsAppBarRegistered;
    private bool IsInAppBarResize;
    private bool IsMinimized;

    private readonly ExplorerWatcher ExplorerWatcher = new();
    private bool IsExplorerRestarting = false;

    static AppBarWindow()
    {
        ShowInTaskbarProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(false));
        MinHeightProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(20d, MinMaxHeightWidth_Changed));
        MinWidthProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(20d, MinMaxHeightWidth_Changed));
        MaxHeightProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(MinMaxHeightWidth_Changed));
        MaxWidthProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(MinMaxHeightWidth_Changed));
    }

    public AppBarWindow()
    {
        InitializeComponent();
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        Topmost = true;
        ExplorerWatcher.ExplorerRestarted += async () =>
        {
            await Task.Delay(300);

            if (IsExplorerRestarting) return;
            IsExplorerRestarting = true;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (IsAppBarRegistered)
                {
                    var abd = GetAppBarData();
                    PInvoke.SHAppBarMessage(PInvoke.ABM_REMOVE, ref abd);

                    var abd1 = GetAppBarData();
                    PInvoke.SHAppBarMessage(PInvoke.ABM_NEW, ref abd1);

                    // set our initial location
                    OnDockLocationChanged();
                }

                IsExplorerRestarting = false;
            });
        };
    }

    #region Dependency Properties

    public AppBarDockMode DockMode
    {
        get => (AppBarDockMode)GetValue(DockModeProperty);
        set => SetValue(DockModeProperty, value);
    }

    public static readonly DependencyProperty DockModeProperty =
        DependencyProperty.Register("DockMode", typeof(AppBarDockMode), typeof(AppBarWindow),
            new FrameworkPropertyMetadata(AppBarDockMode.Top, DockLocation_Changed));

    public MonitorInfo Monitor
    {
        get => (MonitorInfo)GetValue(MonitorProperty);
        set => SetValue(MonitorProperty, value);
    }

    public static readonly DependencyProperty MonitorProperty =
        DependencyProperty.Register("Monitor", typeof(MonitorInfo), typeof(AppBarWindow),
            new FrameworkPropertyMetadata(null, DockLocation_Changed));

    public int DockedWidthOrHeight
    {
        get => (int)GetValue(DockedWidthOrHeightProperty);
        set => SetValue(DockedWidthOrHeightProperty, value);
    }

    public static readonly DependencyProperty DockedWidthOrHeightProperty =
        DependencyProperty.Register("DockedWidthOrHeight", typeof(int), typeof(AppBarWindow),
            new FrameworkPropertyMetadata(200, DockLocation_Changed, DockedWidthOrHeight_Coerce));

    private static object DockedWidthOrHeight_Coerce(DependencyObject d, object baseValue)
    {
        var @this = (AppBarWindow)d;
        var newValue = (int)baseValue;

        return @this.DockMode switch
        {
            AppBarDockMode.Left or AppBarDockMode.Right => BoundIntToDouble(newValue, @this.MinWidth, @this.MaxWidth),
            AppBarDockMode.Top or AppBarDockMode.Bottom => (object)BoundIntToDouble(newValue, @this.MinHeight, @this.MaxHeight),
            _ => throw new NotSupportedException(),
        };
    }

    private static int BoundIntToDouble(int value, double min, double max)
    {
        if (min > value)
        {
            return (int)Math.Ceiling(min);
        }
        else if (max < value)
        {
            return (int)Math.Floor(max);
        }

        return value;
    }

    private static void DockLocation_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var @this = (AppBarWindow)d;

        @this.OnDockLocationChanged();
    }

    private static void MinMaxHeightWidth_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.CoerceValue(DockedWidthOrHeightProperty);
    }

    #endregion

    private void Window_DpiChanged(object sender, DpiChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, () =>
        {
            // WPF seems to have a race condition during DPI changes where the window size
            // is not updated.  We have to force it away to get it to update.
            Width -= 1;
            Height -= 1;
            OnDockLocationChanged();
        });
    }

    private void Window_SourceInitialized(object sender, EventArgs e)
    {
        // add the hook, setup the appbar
        var handle = new WindowInteropHelper(this).Handle;
        _hwndSource = HwndSource.FromHwnd(handle);
        HWND = new(handle);

        if (!ShowInTaskbar)
        {
            var exStyle = PInvoke.GetWindowLongPtr(HWND, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);

            // Add TOOLWINDOW style, remove APPWINDOW style
            var newExStyle = ((uint)exStyle | (uint)WINDOW_EX_STYLE.WS_EX_TOOLWINDOW) & ~(uint)WINDOW_EX_STYLE.WS_EX_APPWINDOW;

            PInvoke.SetWindowLongPtr(HWND, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, (int)newExStyle);
        }

        _hwndSource.AddHook(WndProc);

        var abd = GetAppBarData();
        PInvoke.SHAppBarMessage(PInvoke.ABM_NEW, ref abd);

        // set our initial location
        IsAppBarRegistered = true;
        OnDockLocationChanged();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if (e.Cancel)
        {
            return;
        }

        if (_hwndSource != null)
        {
            _hwndSource.RemoveHook(WndProc);
            _hwndSource.Dispose();
        }

        if (IsAppBarRegistered)
        {
            var abd = GetAppBarData();
            PInvoke.SHAppBarMessage(PInvoke.ABM_REMOVE, ref abd);
            IsAppBarRegistered = false;
        }
    }

    private int WpfDimensionToDesktop(double dim)
    {
        var dpi = VisualTreeHelper.GetDpi(this);

        return (int)Math.Ceiling(dim * dpi.PixelsPerDip);
    }

    private void OnDockLocationChanged()
    {
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }
        else if (!IsAppBarRegistered || IsInAppBarResize)
        {
            return;
        }

        var abd = GetAppBarData();
        var bounds = GetSelectedMonitor().Bounds;
        abd.rc = new RECT((int)bounds.Left, (int)bounds.Top, (int)bounds.Right, (int)bounds.Bottom);

        PInvoke.SHAppBarMessage(PInvoke.ABM_QUERYPOS, ref abd);

        var dockedWidthOrHeightInDesktopPixels = IsMinimized ? 0 : WpfDimensionToDesktop(DockedWidthOrHeight);
        switch (DockMode)
        {
            case AppBarDockMode.Top:
                abd.rc.bottom = abd.rc.top + dockedWidthOrHeightInDesktopPixels;
                break;
            case AppBarDockMode.Bottom:
                abd.rc.top = abd.rc.bottom - dockedWidthOrHeightInDesktopPixels;
                break;
            case AppBarDockMode.Left:
                abd.rc.right = abd.rc.left + dockedWidthOrHeightInDesktopPixels;
                break;
            case AppBarDockMode.Right:
                abd.rc.left = abd.rc.right - dockedWidthOrHeightInDesktopPixels;
                break;
            default: throw new NotSupportedException();
        }

        PInvoke.SHAppBarMessage(PInvoke.ABM_SETPOS, ref abd);
        if (!IsMinimized)
        {
            IsInAppBarResize = true;
            try
            {
                WindowBounds = abd.rc;
            }
            finally
            {
                IsInAppBarResize = false;
            }
        }
    }

    private MonitorInfo GetSelectedMonitor()
    {
        var monitor = Monitor;
        var allMonitors = MonitorInfo.GetDisplayMonitors();
        if (monitor == null! || !allMonitors.Contains(monitor))
        {
            monitor = allMonitors.First(f => f.IsPrimary);
        }
        return monitor;
    }

    private unsafe APPBARDATA GetAppBarData()
    {
        return new APPBARDATA()
        {
            cbSize = (uint)sizeof(APPBARDATA),
            hWnd = HWND,
            uCallbackMessage = AppBarMessageId,
            uEdge = (uint)DockMode
        };
    }

    private static uint _AppBarMessageId;
    public static uint AppBarMessageId
    {
        get
        {
            if (_AppBarMessageId == 0)
            {
                _AppBarMessageId = PInvoke.RegisterWindowMessage("AppBarMessage_EEDFB5206FC3");
            }

            return _AppBarMessageId;
        }
    }

    public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == PInvoke.WM_SIZE)
        {
            IsMinimized = ShowInTaskbar && wParam == PInvoke.SIZE_MINIMIZED;
            OnDockLocationChanged();
        }
        else if (msg == PInvoke.WM_WINDOWPOSCHANGING && !IsInAppBarResize)
        {
            var windowPos = Marshal.PtrToStructure<WINDOWPOS>(lParam);
            const SET_WINDOW_POS_FLAGS NOMOVE_NORESIZE = SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
            if ((windowPos.flags & NOMOVE_NORESIZE) != NOMOVE_NORESIZE
                && !IsMinimized
                && !(windowPos.x == -32_000 && windowPos.y == -32_000)) // Location for minimized windows
            {
                windowPos.flags |= NOMOVE_NORESIZE;
                Marshal.StructureToPtr(windowPos, lParam, false);
            }
        }
        else if (msg == PInvoke.WM_ACTIVATE)
        {
            var abd = GetAppBarData();
            PInvoke.SHAppBarMessage(PInvoke.ABM_ACTIVATE, ref abd);
        }
        else if (msg == PInvoke.WM_WINDOWPOSCHANGED)
        {
            var abd = GetAppBarData();
            PInvoke.SHAppBarMessage(PInvoke.ABM_WINDOWPOSCHANGED, ref abd);
        }
        else if (msg == AppBarMessageId)
        {
            if (wParam == PInvoke.ABN_POSCHANGED)
            {
                OnDockLocationChanged();
                handled = true;
            }
        }

        return IntPtr.Zero;
    }

    private RECT WindowBounds
    {
        set
        {
            // SetWindowPos accepts the position _with_ the shadow, but we don't know how large the shadow
            // will be until we are in place.
            // 
            // 1. Move to the position using current shadow
            // 2. Get the actual shadow size
            // 3. Move to the position using the actual shadow size

            var frameThickness = GetFrameThickness(HWND);
            var actualShadow = Inflate(value, frameThickness);
            PInvoke.SetWindowPos(HWND, HWND.Null, actualShadow.X, actualShadow.Y, actualShadow.Width, actualShadow.Height, 0);

            var newFrameThickness = GetFrameThickness(HWND);
            if (frameThickness.left != newFrameThickness.left ||
                frameThickness.top != newFrameThickness.top ||
                frameThickness.right != newFrameThickness.right ||
                frameThickness.bottom != newFrameThickness.bottom)
            {
                var newActualShadow = Inflate(value, frameThickness);
                PInvoke.SetWindowPos(HWND, HWND.Null, newActualShadow.X, newActualShadow.Y, newActualShadow.Width, newFrameThickness.Height, 0);
            }
        }
    }

    private static RECT Inflate(RECT rect, RECT thickness)
    {
        return new RECT(
            rect.left - thickness.left,
            rect.top - thickness.top,
            rect.right + thickness.right,
            rect.bottom + thickness.bottom
        );
    }

    private static unsafe RECT GetFrameThickness(HWND hWnd)
    {
        if (!PInvoke.GetWindowRect(hWnd, out var clientBounds))
        {
            return default;
        }
        RECT frameBounds;
        if (PInvoke.DwmGetWindowAttribute(
            hWnd,
            DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
            &frameBounds,
            (uint)Marshal.SizeOf<RECT>()) != HRESULT.S_OK)
        {
            return default;
        }
        return new RECT(
            frameBounds.left - clientBounds.left,
            frameBounds.top - clientBounds.top,
            clientBounds.right - frameBounds.right,
            clientBounds.bottom - frameBounds.bottom);
    }
}

public enum AppBarDockMode
{
    Left = 0,
    Top,
    Right,
    Bottom
}
