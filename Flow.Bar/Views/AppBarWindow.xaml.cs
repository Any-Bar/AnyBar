using CommunityToolkit.Mvvm.DependencyInjection;
using Flow.Bar.Controls;
using Flow.Bar.Helper.MenuFlyout;
using Flow.Bar.Helper.Plugins;
using Flow.Bar.Helper.Windows;
using Flow.Bar.Models;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Plugin;
using Flow.Bar.Services;
using Flow.Bar.ViewModels;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using FontIcon = iNKORE.UI.WPF.Modern.Controls.FontIcon;

namespace Flow.Bar.Views;

public partial class AppBarWindow : Window
{
    public AppBarViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<AppBarViewModel>();
    public AppBarModel Model { get; }

    private static readonly string ClassName = nameof(AppBarWindow);

    private readonly AppBarManagementService _appBarManagementService = Ioc.Default.GetRequiredService<AppBarManagementService>();
    private readonly NavigationViewService _navigationViewService = Ioc.Default.GetRequiredService<NavigationViewService>();

    private HWND _hwnd;
    private HwndSource? _hwndSource;

    private bool _isAppBarRegistered;
    private bool _isInAppBarResize;
    private bool _isMinimized;

    private readonly ExplorerWatcher _explorerWatcher = new();
    private bool _isExplorerRestarting = false;

    private readonly AppBarMenuFlyoutHelper _menuFlyoutHelper = new();

    #region Constructor

    public AppBarWindow(AppBarModel model)
    {
        Model = model;
        ViewModel.Initialize(model);
        DataContext = ViewModel;
        InitializeComponent();
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        Topmost = true;
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        InitializaExplorerWatcher();
        InitializeMenuFlyout();
    }

    #endregion

    #region Initialization

    private void InitializaExplorerWatcher()
    {
        _explorerWatcher.ExplorerRestarted += async () =>
        {
            await Task.Delay(300);

            if (_isExplorerRestarting) return;
            _isExplorerRestarting = true;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_isAppBarRegistered)
                {
                    {
                        var abd = GetAppBarData();
                        PInvoke.SHAppBarMessage(PInvoke.ABM_REMOVE, ref abd);
                    }

                    {
                        var abd = GetAppBarData();
                        PInvoke.SHAppBarMessage(PInvoke.ABM_NEW, ref abd);
                    }

                    // set our initial location
                    OnDockLocationChanged();
                }

                _isExplorerRestarting = false;
            });
        };
    }

    private void InitializeMenuFlyout()
    {
        var settingItem = new MenuItem
        {
            Icon = new FontIcon { Glyph = "\ue713" }
        };
        settingItem.SetResourceReference(HeaderedItemsControl.HeaderProperty, nameof(Localize.SettingAppBarWindow_AppBarSettings));
        settingItem.Click += (o, e) =>
        {
            // Setting window is already opened
            if (WindowTracker.GetActiveWindow<SettingWindow>().Count > 0)
            {
                App.API.ShowSettingWindow();
                _navigationViewService.NavigateTo(SettingPageTag.AppBarSetting, Model);
            }
            else
            {
                _navigationViewService.SetNextNavigation(SettingPageTag.AppBarSetting, Model);
                App.API.ShowSettingWindow();
            }
        };
        _menuFlyoutHelper.Items.Add(settingItem);
        _menuFlyoutHelper.ViewModel = ViewModel;
        _menuFlyoutHelper.Element = MainGrid;
        _menuFlyoutHelper.Handled = true;
    }

    #endregion

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
        @this.OnDockLocationChanged();
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
        OnDockLocationChanged();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // No need to call OnDockLocationChanged - It will be called in property changed handler
        UpdateDockedWidthOrHeight();
        OnIsResizableChanged();
        ViewModel.InitializeBarElements();
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

        _menuFlyoutHelper.Dispose();
        ViewModel.Dispose();
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

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
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
        switch (e.PropertyName)
        {
            case nameof(AppBarViewModel.DockMode):
                OnDockLocationChanged();
                break;
            case nameof(AppBarViewModel.ActualMonitor):
                OnDockLocationChanged();
                break;
            case nameof(AppBarViewModel.IsResizable):
                OnIsResizableChanged();
                break;
            case nameof(AppBarViewModel.FollowSystemTaskbarWidthOrHeight):
                OnSystemTaskbarWidthOrHeightChanged();
                break;
            case nameof(AppBarViewModel.DockedWidthOrHeight):
                OnDockedWidthOrHeightChanged();
                break;
            case nameof(AppBarViewModel.ActualDockedWidthOrHeight):
                OnDockLocationChanged();
                break;
        }
    }

    #endregion

    #region Thumb Events

    private void OnIsResizableChanged()
    {
        if (ViewModel.IsResizable)
        {
            BarThumb.DragCompleted += BarThumb_DragCompleted;
        }
        else
        {
            BarThumb.DragCompleted -= BarThumb_DragCompleted;
        }
    }

    private void BarThumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        var delta = ViewModel.DockMode switch
        {
            AppBarDockMode.Left => e.HorizontalChange,
            AppBarDockMode.Right => e.HorizontalChange * -1,
            AppBarDockMode.Top => e.VerticalChange,
            AppBarDockMode.Bottom => e.VerticalChange * -1,
            _ => throw new NotImplementedException()
        };
        ViewModel.DockedWidthOrHeight += (int)(delta / VisualTreeHelper.GetDpi(this).PixelsPerDip);
        // Users have set a specific width or height, so we should not follow the system taskbar width or height.
        if (ViewModel.FollowSystemTaskbarWidthOrHeight)
        {
            ViewModel.FollowSystemTaskbarWidthOrHeight = false;
        }
    }

    #endregion

    #region Dock Events

    private void OnSystemTaskbarWidthOrHeightChanged()
    {
        UpdateDockedWidthOrHeight();

        _appBarManagementService.SetFollowSystemTaskbarWidthOrHeight(ViewModel.Model.Order, ViewModel.FollowSystemTaskbarWidthOrHeight);
    }

    private void OnDockedWidthOrHeightChanged()
    {
        UpdateDockedWidthOrHeight();

        _appBarManagementService.SetDockedWidthOrHeight(ViewModel.Model.Order, ViewModel.DockedWidthOrHeight);
    }

    private void UpdateDockedWidthOrHeight()
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

        var dockedWidthOrHeight = ViewModel.GetDockedWidthOrHeight();
        ViewModel.ActualDockedWidthOrHeight = ViewModel.DockMode switch
        {
            AppBarDockMode.Left or AppBarDockMode.Right => BoundIntToDouble(dockedWidthOrHeight, MinWidth, MaxWidth),
            AppBarDockMode.Top or AppBarDockMode.Bottom => BoundIntToDouble(dockedWidthOrHeight, MinHeight, MaxHeight),
            _ => throw new NotImplementedException()
        };
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
        else if (ViewModel.ActualMonitor == null)
        {
            return;
        }

        var abd = GetAppBarData();
        var bounds = ViewModel.ActualMonitor.Bounds;
        abd.rc = new RECT((int)bounds.Left, (int)bounds.Top, (int)bounds.Right, (int)bounds.Bottom);

        PInvoke.SHAppBarMessage(PInvoke.ABM_QUERYPOS, ref abd);

        var dockedWidthOrHeightInDesktopPixels = _isMinimized ? 0 : WpfDimensionToDesktop(this, ViewModel.ActualDockedWidthOrHeight);
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
            default:
                throw new NotImplementedException();
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

    private void MainGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        App.API.LogVerbose(ClassName, $"Prepare {nameof(MainGrid)} right click context menu");
        _menuFlyoutHelper.MouseRightButtonDown(sender, e);
    }

    private void MainGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        App.API.LogVerbose(ClassName, $"Show {nameof(MainGrid)} right click context menu");
        _menuFlyoutHelper.MouseRightButtonUp(sender, e);
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
            uEdge = (uint)ViewModel.DockMode
        };
    }

    private static uint _AppBarMessageId;
    private static uint AppBarMessageId
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

    #region StackView Events

    private void StackView_ItemMouseLeftButtonDown(object sender, StackViewItemMouseButtonEventArgs e)
    {
        if (e.ClickedItem is not StackViewItem item) return;
        if (item.DataContext is not BarElementModel model) return;

        App.API.LogVerbose(ClassName, $"Prepare {nameof(StackViewItem)} left click context menu");
        // TODO
        e.OriginalEventArgs.Handled = true;
    }

    private void StackView_ItemMouseLeftButtonUp(object sender, StackViewItemMouseButtonEventArgs e)
    {
        if (e.ClickedItem is not StackViewItem item) return;
        if (item.DataContext is not BarElementModel model) return;

        App.API.LogVerbose(ClassName, $"Show {nameof(StackViewItem)} left click context menu");
        // TODO
        e.OriginalEventArgs.Handled = true;
    }

    private void StackView_ItemPreviewMouseRightButtonDown(object sender, StackViewItemMouseButtonEventArgs e)
    {
        if (e.ClickedItem is not StackViewItem item) return;
        if (item.DataContext is not BarElementModel model) return;

        App.API.LogVerbose(ClassName, $"Prepare {nameof(StackViewItem)} right click context menu");
        if (PluginManager.GetRightClickMenu(model.ID) is IRightClickMenu rightClickMenu)
        {
            if (BarElementMenuFlyoutHelper.GetMenuFlyoutHelper(item) is not AppBarMenuFlyoutHelper helper)
            {
                helper = new AppBarMenuFlyoutHelper();
                foreach (var menuItem in rightClickMenu.GetRightClickMenuItems())
                {
                    helper.Items.Add(menuItem);
                }
                helper.ViewModel = ViewModel;
                BarElementMenuFlyoutHelper.SetMenuFlyoutHelper(item, helper);
            }

            helper.MouseRightButtonDown(sender, e.OriginalEventArgs);
        }
        e.OriginalEventArgs.Handled = true;
    }

    private void StackView_ItemPreviewMouseRightButtonUp(object sender, StackViewItemMouseButtonEventArgs e)
    {
        if (e.ClickedItem is not StackViewItem item) return;
        if (item.DataContext is not BarElementModel model) return;

        App.API.LogVerbose(ClassName, $"Prepare {nameof(StackViewItem)} right click context menu");
        if (PluginManager.GetRightClickMenu(model.ID) is IRightClickMenu rightClickMenu)
        {
            if (BarElementMenuFlyoutHelper.GetMenuFlyoutHelper(item) is not AppBarMenuFlyoutHelper helper)
            {
                helper = new AppBarMenuFlyoutHelper();
                foreach (var menuItem in rightClickMenu.GetRightClickMenuItems())
                {
                    helper.Items.Add(menuItem);
                }
                helper.ViewModel = ViewModel;
                BarElementMenuFlyoutHelper.SetMenuFlyoutHelper(item, helper);
            }

            helper.MouseRightButtonUp(sender, e.OriginalEventArgs);
        }
        e.OriginalEventArgs.Handled = true;
    }

    #endregion
}
