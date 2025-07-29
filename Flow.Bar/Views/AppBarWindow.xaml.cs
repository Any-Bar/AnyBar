using CommunityToolkit.Mvvm.DependencyInjection;
using Flow.Bar.Controls.MenuFlyout;
using Flow.Bar.Helper.Plugins;
using Flow.Bar.Models;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Plugin;
using Flow.Bar.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
    private HWND _hwnd;
    private HwndSource? _hwndSource;

    private bool _isAppBarRegistered;
    private bool _isInAppBarResize;
    private bool _isMinimized;

    private readonly ExplorerWatcher _explorerWatcher = new();
    private bool _isExplorerRestarting = false;

    private readonly AppBarModel _model;
    private readonly AppBarViewModel _viewModel = Ioc.Default.GetRequiredService<AppBarViewModel>();

    private readonly AppBarMenuFlyout _contextMenu = new();

    public AppBarWindow(AppBarModel model)
    {
        _model = model;
        _viewModel.Order = model.Order;
        _viewModel.Order = _model.Order;
        _viewModel.DockMode = _model.DockMode;
        if (_model.MonitorName != null)
        {
            _viewModel.Monitor = MonitorInfo.GetDisplayMonitors().FirstOrDefault(m => m.Name == _model.MonitorName);
        }
        else
        {
            _viewModel.Monitor = null;
        }
        _viewModel.DockedWidthOrHeight = _model.DockedWidthOrHeight;
        _viewModel.IsResizable = _model.IsResizable;
        InitializeComponent();
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        Topmost = true;
        _explorerWatcher.ExplorerRestarted += async () =>
        {
            await Task.Delay(300);

            if (_isExplorerRestarting) return;
            _isExplorerRestarting = true;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_isAppBarRegistered)
                {
                    var abd = GetAppBarData();
                    PInvoke.SHAppBarMessage(PInvoke.ABM_REMOVE, ref abd);

                    var abd1 = GetAppBarData();
                    PInvoke.SHAppBarMessage(PInvoke.ABM_NEW, ref abd1);

                    // set our initial location
                    OnDockLocationChanged();
                }

                _isExplorerRestarting = false;
            });
        };
        var settingItem = new MenuItem
        {
            Header = "Appbar settings",
            Icon = new iNKORE.UI.WPF.Modern.Controls.FontIcon { Glyph = "\ue713" }
        };
        settingItem.Click += (o, e) =>
        {
            App.API.OpenSettingDialog();
        };
        _contextMenu.Items.Add(settingItem);
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
        _hwnd = new(handle);

        if (!ShowInTaskbar)
        {
            var exStyle = PInvoke.GetWindowLongPtr(_hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);

            // Add TOOLWINDOW style, remove APPWINDOW style
            var newExStyle = ((uint)exStyle | (uint)WINDOW_EX_STYLE.WS_EX_TOOLWINDOW) & ~(uint)WINDOW_EX_STYLE.WS_EX_APPWINDOW;

            PInvoke.SetWindowLongPtr(_hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, (int)newExStyle);
        }

        _hwndSource.AddHook(WndProc);

        var abd = GetAppBarData();
        PInvoke.SHAppBarMessage(PInvoke.ABM_NEW, ref abd);

        // set our initial location
        _isAppBarRegistered = true;
        InitDockHeightOrWidth();
    }

    private void InitDockHeightOrWidth()
    {
        static int DesktopDimensionToWpf(Visual visual, int dim)
        {
            var dpi = VisualTreeHelper.GetDpi(visual);

            return (int)Math.Round(dim / dpi.PixelsPerDip);
        }

        if (_viewModel.DockedWidthOrHeight != null) return;

        var monitor = MonitorInfo.GetPrimaryDisplayMonitor();
        if (monitor != null)
        {
            var taskBarHeight = monitor.Bounds.Height - monitor.WorkingArea.Height;
            if (taskBarHeight != 0) // Taskbar is docked at the top or bottom
            {
                _viewModel.DockedWidthOrHeight = DesktopDimensionToWpf(this, (int)taskBarHeight);
            }
            else
            {
                var taskBarWidth = monitor.Bounds.Width - monitor.WorkingArea.Width;
                if (taskBarWidth != 0) // Taskbar is docked at the left or right
                {
                    _viewModel.DockedWidthOrHeight = DesktopDimensionToWpf(this, (int)taskBarWidth);
                }
                else
                {
                    // No taskbar detected, set a default value
                    _viewModel.DockedWidthOrHeight = 200;
                }
            }
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Init plugin controls
        LeftOrTopStackPanel.Children.Clear();
        RightOrBottomStackPanel.Children.Clear();
        CenterStackPanel.Children.Clear();
        foreach (var pluginControlModel in _model.LeftOrTopPluginControls.OrderBy(c => c.Order))
        {
            var pluginControl = PluginManager.GetBarElement(pluginControlModel.ID, 
                _viewModel.IsHorizontal ? BarElementPosition.Left : BarElementPosition.Top);
            if (pluginControl == null) continue;
            LeftOrTopStackPanel.Children.Add(pluginControl);
            if (_viewModel.DockMode == AppBarDockMode.Left)
            {
                pluginControl.VerticalAlignment = VerticalAlignment.Top;
                pluginControl.HorizontalAlignment = HorizontalAlignment.Center;
            }
            else
            {
                pluginControl.VerticalAlignment = VerticalAlignment.Center;
                pluginControl.HorizontalAlignment = HorizontalAlignment.Left;
            }
        }
        foreach (var pluginControlModel in _model.RightOrBottomPluginControls.OrderBy(c => c.Order))
        {
            var pluginControl = PluginManager.GetBarElement(pluginControlModel.ID,
                _viewModel.IsHorizontal ? BarElementPosition.Right : BarElementPosition.Bottom);
            if (pluginControl == null) continue;
            RightOrBottomStackPanel.Children.Add(pluginControl);
            if (_viewModel.DockMode == AppBarDockMode.Left)
            {
                pluginControl.VerticalAlignment = VerticalAlignment.Top;
                pluginControl.HorizontalAlignment = HorizontalAlignment.Center;
            }
            else
            {
                pluginControl.VerticalAlignment = VerticalAlignment.Center;
                pluginControl.HorizontalAlignment = HorizontalAlignment.Left;
            }
        }
        foreach (var pluginControlModel in _model.CenterPluginControls.OrderBy(c => c.Order))
        {
            var pluginControl = PluginManager.GetBarElement(pluginControlModel.ID,
                _viewModel.IsHorizontal ? BarElementPosition.HorizontalCenter : BarElementPosition.VerticalCenter);
            if (pluginControl == null) continue;
            CenterStackPanel.Children.Add(pluginControl);
            if (_viewModel.DockMode == AppBarDockMode.Left)
            {
                pluginControl.VerticalAlignment = VerticalAlignment.Top;
                pluginControl.HorizontalAlignment = HorizontalAlignment.Center;
            }
            else
            {
                pluginControl.VerticalAlignment = VerticalAlignment.Center;
                pluginControl.HorizontalAlignment = HorizontalAlignment.Left;
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

        if (_isAppBarRegistered)
        {
            var abd = GetAppBarData();
            PInvoke.SHAppBarMessage(PInvoke.ABM_REMOVE, ref abd);
            _isAppBarRegistered = false;
        }
    }

    #endregion

    #region HWND Hook

    public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == PInvoke.WM_SIZE)
        {
            _isMinimized = ShowInTaskbar && wParam == PInvoke.SIZE_MINIMIZED;
            OnDockLocationChanged();
        }
        else if (msg == PInvoke.WM_WINDOWPOSCHANGING && !_isInAppBarResize)
        {
            var windowPos = Marshal.PtrToStructure<WINDOWPOS>(lParam);
            const SET_WINDOW_POS_FLAGS NOMOVE_NORESIZE = SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
            if ((windowPos.flags & NOMOVE_NORESIZE) != NOMOVE_NORESIZE
                && !_isMinimized
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
                switch (_viewModel.DockMode)
                {
                    case AppBarDockMode.Left:
                    case AppBarDockMode.Right:
                        // Set thumb
                        BarThumb.Width = 2;
                        BarThumb.Height = double.NaN;
                        BarThumb.Cursor = Cursors.SizeWE;
                        DockPanel.SetDock(BarThumb, _viewModel.DockMode == AppBarDockMode.Left ? Dock.Right : Dock.Left);
                        // Set grid
                        PluginControlGrid.Margin = _viewModel.DockMode == AppBarDockMode.Left ? new Thickness(0, 8, BarThumb.Width, 8) : new Thickness(BarThumb.Width, 8, 0, 8);
                        // Set stack panel
                        LeftOrTopStackPanel.Orientation = Orientation.Vertical;
                        Grid.SetRow(LeftOrTopStackPanel, 0);
                        Grid.SetColumn(LeftOrTopStackPanel, 0);
                        Grid.SetRowSpan(LeftOrTopStackPanel, 1);
                        Grid.SetColumnSpan(LeftOrTopStackPanel, 3);
                        foreach (var child in LeftOrTopStackPanel.Children)
                        {
                            if (child is FrameworkElement control)
                            {
                                control.VerticalAlignment = VerticalAlignment.Top;
                                control.HorizontalAlignment = HorizontalAlignment.Center;
                            }
                        }

                        RightOrBottomStackPanel.Orientation = Orientation.Vertical;
                        Grid.SetRow(RightOrBottomStackPanel, 2);
                        Grid.SetColumn(RightOrBottomStackPanel, 0);
                        Grid.SetRowSpan(RightOrBottomStackPanel, 1);
                        Grid.SetColumnSpan(RightOrBottomStackPanel, 3);
                        foreach (var child in RightOrBottomStackPanel.Children)
                        {
                            if (child is FrameworkElement control)
                            {
                                control.VerticalAlignment = VerticalAlignment.Top;
                                control.HorizontalAlignment = HorizontalAlignment.Center;
                            }
                        }

                        CenterStackPanel.Orientation = Orientation.Vertical;
                        foreach (var child in CenterStackPanel.Children)
                        {
                            if (child is FrameworkElement control)
                            {
                                control.VerticalAlignment = VerticalAlignment.Top;
                                control.HorizontalAlignment = HorizontalAlignment.Center;
                            }
                        }
                        break;
                    case AppBarDockMode.Top:
                    case AppBarDockMode.Bottom:
                        // Set thumb
                        BarThumb.Height = 2;
                        BarThumb.Width = double.NaN;
                        BarThumb.Cursor = Cursors.SizeNS;
                        DockPanel.SetDock(BarThumb, _viewModel.DockMode == AppBarDockMode.Top ? Dock.Bottom : Dock.Top);
                        // Set grid
                        PluginControlGrid.Margin = _viewModel.DockMode == AppBarDockMode.Top ? new Thickness(8, 0, 8, BarThumb.Height) : new Thickness(8, BarThumb.Height, 8, 0);
                        // Set stack panel
                        LeftOrTopStackPanel.Orientation = Orientation.Horizontal;
                        Grid.SetRow(LeftOrTopStackPanel, 0);
                        Grid.SetColumn(LeftOrTopStackPanel, 0);
                        Grid.SetRowSpan(LeftOrTopStackPanel, 3);
                        Grid.SetColumnSpan(LeftOrTopStackPanel, 1);
                        foreach (var child in LeftOrTopStackPanel.Children)
                        {
                            if (child is FrameworkElement control)
                            {
                                control.VerticalAlignment = VerticalAlignment.Center;
                                control.HorizontalAlignment = HorizontalAlignment.Left;
                            }
                        }

                        RightOrBottomStackPanel.Orientation = Orientation.Horizontal;
                        Grid.SetRow(RightOrBottomStackPanel, 0);
                        Grid.SetColumn(RightOrBottomStackPanel, 2);
                        Grid.SetRowSpan(RightOrBottomStackPanel, 3);
                        Grid.SetColumnSpan(RightOrBottomStackPanel, 1);
                        foreach (var child in RightOrBottomStackPanel.Children)
                        {
                            if (child is FrameworkElement control)
                            {
                                control.VerticalAlignment = VerticalAlignment.Center;
                                control.HorizontalAlignment = HorizontalAlignment.Left;
                            }
                        }

                        CenterStackPanel.Orientation = Orientation.Horizontal;
                        foreach (var child in CenterStackPanel.Children)
                        {
                            if (child is FrameworkElement control)
                            {
                                control.VerticalAlignment = VerticalAlignment.Center;
                                control.HorizontalAlignment = HorizontalAlignment.Left;
                            }
                        }
                        break;
                    default:
                        throw new NotSupportedException();
                }
                break;
            case nameof(AppBarViewModel.Monitor):
                OnDockLocationChanged();
                break;
            case nameof(AppBarViewModel.IsResizable):
                if (_viewModel.IsResizable)
                {
                    BarThumb.Cursor = _viewModel.DockMode switch
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
        var delta = _viewModel.DockMode switch
        {
            AppBarDockMode.Left => e.HorizontalChange,
            AppBarDockMode.Right => e.HorizontalChange * -1,
            AppBarDockMode.Top => e.VerticalChange,
            AppBarDockMode.Bottom => e.VerticalChange * -1,
            _ => throw new NotSupportedException(),
        };
        _viewModel.DockedWidthOrHeight += (int)(delta / VisualTreeHelper.GetDpi(this).PixelsPerDip);
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

        if (_viewModel.DockedWidthOrHeight == null) return false;

        var dockedWidthOrHeight = _viewModel.DockMode switch
        {
            AppBarDockMode.Left or AppBarDockMode.Right => BoundIntToDouble(_viewModel.DockedWidthOrHeight.Value, MinWidth, MaxWidth),
            AppBarDockMode.Top or AppBarDockMode.Bottom => BoundIntToDouble(_viewModel.DockedWidthOrHeight.Value, MinHeight, MaxHeight),
            _ => throw new NotSupportedException(),
        };

        if (_viewModel.DockedWidthOrHeight != dockedWidthOrHeight)
        {
            _viewModel.DockedWidthOrHeight = dockedWidthOrHeight;
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
        else if (!_isAppBarRegistered || _isInAppBarResize)
        {
            return;
        }
        else if (_viewModel.DockedWidthOrHeight == null)
        {
            return;
        }

        var abd = GetAppBarData();
        var bounds = _viewModel.GetSelectedMonitor().Bounds;
        abd.rc = new RECT((int)bounds.Left, (int)bounds.Top, (int)bounds.Right, (int)bounds.Bottom);

        PInvoke.SHAppBarMessage(PInvoke.ABM_QUERYPOS, ref abd);

        var dockedWidthOrHeightInDesktopPixels = _isMinimized ? 0 : WpfDimensionToDesktop(this, _viewModel.DockedWidthOrHeight.Value);
        switch (_viewModel.DockMode)
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
        if (!_isMinimized)
        {
            _isInAppBarResize = true;
            try
            {
                WindowBounds = abd.rc;
            }
            finally
            {
                _isInAppBarResize = false;
            }
        }
    }

    #endregion

    #region Grid Events

    private void MainGrid_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        OpenAppBarMenu(sender, e);
    }

    private void MainGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        OpenAppBarMenu(sender, e);
    }

    private void OpenAppBarMenu(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            var placement = _viewModel.DockMode switch
            {
                AppBarDockMode.Left => AppBarPlacementMode.Right,
                AppBarDockMode.Right => AppBarPlacementMode.Left,
                AppBarDockMode.Top => AppBarPlacementMode.Bottom,
                AppBarDockMode.Bottom => AppBarPlacementMode.Top,
                _ => throw new NotSupportedException(),
            };
            _contextMenu.ShowAt(element, new AppBarMenuFlyoutOptions()
            {
                Placement = placement,
                Position = e.GetPosition(element),
                Monitor = _viewModel.GetSelectedMonitor()
            });
            e.Handled = true;
        }
    }

    #endregion

    #region App Bar Helpers

    private unsafe APPBARDATA GetAppBarData()
    {
        return new APPBARDATA()
        {
            cbSize = (uint)sizeof(APPBARDATA),
            hWnd = _hwnd,
            uCallbackMessage = AppBarMessageId,
            uEdge = (uint)_viewModel.DockMode
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

            var frameThickness = GetFrameThickness(_hwnd);
            var actualShadow = Inflate(value, frameThickness);
            PInvoke.SetWindowPos(_hwnd, HWND.Null, actualShadow.X, actualShadow.Y, actualShadow.Width, actualShadow.Height, 0);

            var newFrameThickness = GetFrameThickness(_hwnd);
            if (frameThickness.left != newFrameThickness.left ||
                frameThickness.top != newFrameThickness.top ||
                frameThickness.right != newFrameThickness.right ||
                frameThickness.bottom != newFrameThickness.bottom)
            {
                var newActualShadow = Inflate(value, frameThickness);
                PInvoke.SetWindowPos(_hwnd, HWND.Null, newActualShadow.X, newActualShadow.Y, newActualShadow.Width, newFrameThickness.Height, 0);
            }
        }
    }


    #endregion
}
