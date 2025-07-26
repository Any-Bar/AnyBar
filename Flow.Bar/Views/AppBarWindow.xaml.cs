using Flow.Bar.Models;
using Flow.Bar.ViewModels;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

    private readonly AppBarViewModel ViewModel;

    public AppBarWindow(AppBarViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
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

    static AppBarWindow()
    {
        ShowInTaskbarProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(false));
        MinHeightProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(20d, MinMaxHeightWidth_Changed));
        MinWidthProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(20d, MinMaxHeightWidth_Changed));
        MaxHeightProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(MinMaxHeightWidth_Changed));
        MaxWidthProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(MinMaxHeightWidth_Changed));
    }

    private static void MinMaxHeightWidth_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var @this = (AppBarWindow)d;

        if (!@this.OnDockWidthOrHeightChanged())
        {
            @this.OnDockLocationChanged();
        }
    }

    #endregion

    #region Window Events

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
        InitDockHeightOrWidth();
    }

    private void InitDockHeightOrWidth()
    {
        static int DesktopDimensionToWpf(Visual visual, int dim)
        {
            var dpi = VisualTreeHelper.GetDpi(visual);

            return (int)Math.Round(dim / dpi.PixelsPerDip);
        }

        var monitor = MonitorInfo.GetPrimaryDisplayMonitor();
        if (monitor != null)
        {
            var taskBarHeight = monitor.Bounds.Height - monitor.WorkingArea.Height;
            if (taskBarHeight != 0) // Taskbar is docked at the top or bottom
            {
                ViewModel.DockedWidthOrHeight = DesktopDimensionToWpf(this, (int)taskBarHeight);
            }
            else
            {
                var taskBarWidth = monitor.Bounds.Width - monitor.WorkingArea.Width;
                if (taskBarWidth != 0) // Taskbar is docked at the left or right
                {
                    ViewModel.DockedWidthOrHeight = DesktopDimensionToWpf(this, (int)taskBarWidth);
                }
                else
                {
                    // No taskbar detected, set a default value and raise the location change event manually
                    OnDockLocationChanged();
                }
            }
        }
    }

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

    #endregion

    #region HWND Hook

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

    #endregion

    #region View Model Events

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Dispatcher.BeginInvoke?
        switch (e.PropertyName)
        {
            case nameof(AppBarViewModel.DockMode):
                OnDockLocationChanged();
                switch (ViewModel.DockMode)
                {
                    case AppBarDockMode.Left:
                    case AppBarDockMode.Right:
                        BarThumb.Width = 5;
                        BarThumb.Height = double.NaN;
                        BarThumb.Cursor = Cursors.SizeWE;
                        DockPanel.SetDock(BarThumb, ViewModel.DockMode == AppBarDockMode.Left ? Dock.Right : Dock.Left);
                        break;
                    case AppBarDockMode.Top:
                    case AppBarDockMode.Bottom:
                        BarThumb.Height = 5;
                        BarThumb.Width = double.NaN;
                        BarThumb.Cursor = Cursors.SizeNS;
                        DockPanel.SetDock(BarThumb, ViewModel.DockMode == AppBarDockMode.Top ? Dock.Bottom : Dock.Top);
                        break;
                    default:
                        throw new NotSupportedException();
                }
                break;
            case nameof(AppBarViewModel.Monitor):
                OnDockLocationChanged();
                break;
            case nameof(AppBarViewModel.IsResizable):
                if (ViewModel.IsResizable)
                {
                    BarThumb.Cursor = ViewModel.DockMode switch
                    {
                        AppBarDockMode.Left or AppBarDockMode.Right => Cursors.SizeWE,
                        AppBarDockMode.Top or AppBarDockMode.Bottom => Cursors.SizeNS,
                        _ => throw new NotSupportedException(),
                    };
                    BarThumb.DragCompleted += BarThumb_DragCompleted;
                }
                else
                {
                    BarThumb.Cursor = Cursors.Arrow;
                    BarThumb.DragCompleted -= BarThumb_DragCompleted;
                }
                break;
            case nameof(AppBarViewModel.DockedWidthOrHeight):
                if (!OnDockWidthOrHeightChanged())
                {
                    OnDockLocationChanged();
                }
                break;
        }
    }

    #endregion

    #region Thumb Events

    private void BarThumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
        var delta = ViewModel.DockMode switch
        {
            AppBarDockMode.Left => e.HorizontalChange,
            AppBarDockMode.Right => e.HorizontalChange * -1,
            AppBarDockMode.Top => e.VerticalChange,
            AppBarDockMode.Bottom => e.VerticalChange * -1,
            _ => throw new NotSupportedException(),
        };
        ViewModel.DockedWidthOrHeight += (int)(delta / VisualTreeHelper.GetDpi(this).PixelsPerDip);
    }

    #endregion

    #region Dock Events

    private bool OnDockWidthOrHeightChanged()
    {
        static int BoundIntToDouble(int value, double min, double max)
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

        var dockedWidthOrHeight = ViewModel.DockMode switch
        {
            AppBarDockMode.Left or AppBarDockMode.Right => BoundIntToDouble(ViewModel.DockedWidthOrHeight, MinWidth, MaxWidth),
            AppBarDockMode.Top or AppBarDockMode.Bottom => BoundIntToDouble(ViewModel.DockedWidthOrHeight, MinHeight, MaxHeight),
            _ => throw new NotSupportedException(),
        };

        if (ViewModel.DockedWidthOrHeight != dockedWidthOrHeight)
        {
            ViewModel.DockedWidthOrHeight = dockedWidthOrHeight;
            return true;
        }

        return false;
    }

    private void OnDockLocationChanged()
    {
        static int WpfDimensionToDesktop(Visual visual, double dim)
        {
            var dpi = VisualTreeHelper.GetDpi(visual);

            return (int)Math.Ceiling(dim * dpi.PixelsPerDip);
        }

        if (DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }
        else if (!IsAppBarRegistered || IsInAppBarResize)
        {
            return;
        }

        var abd = GetAppBarData();
        var bounds = ViewModel.GetSelectedMonitor().Bounds;
        abd.rc = new RECT((int)bounds.Left, (int)bounds.Top, (int)bounds.Right, (int)bounds.Bottom);

        PInvoke.SHAppBarMessage(PInvoke.ABM_QUERYPOS, ref abd);

        var dockedWidthOrHeightInDesktopPixels = IsMinimized ? 0 : WpfDimensionToDesktop(this, ViewModel.DockedWidthOrHeight);
        switch (ViewModel.DockMode)
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

    #endregion

    #region App Bar Helpers

    private unsafe APPBARDATA GetAppBarData()
    {
        return new APPBARDATA()
        {
            cbSize = (uint)sizeof(APPBARDATA),
            hWnd = HWND,
            uCallbackMessage = AppBarMessageId,
            uEdge = (uint)ViewModel.DockMode
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

    private RECT WindowBounds
    {
        set
        {
            static RECT Inflate(RECT rect, RECT thickness)
            {
                return new RECT(
                    rect.left - thickness.left,
                    rect.top - thickness.top,
                    rect.right + thickness.right,
                    rect.bottom + thickness.bottom
                );
            }

            static unsafe RECT GetFrameThickness(HWND hWnd)
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


    #endregion
}
