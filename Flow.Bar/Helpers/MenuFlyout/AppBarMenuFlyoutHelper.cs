using Flow.Bar.Controls;
using Flow.Bar.Enums;
using Flow.Bar.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.Win32;
using Point = System.Drawing.Point;

namespace Flow.Bar.Helper.MenuFlyout;

public class AppBarMenuFlyoutHelper : IDisposable
{
    public ItemCollection Items => _contextMenu.Items;
    public AppBarViewModel ViewModel { get; set; } = null!;
    public FrameworkElement Element { get; set; } = null!;
    public bool Handled { get; set; } = false;

    private readonly MenuFlyoutEx _contextMenu = new();
    private Point? _cursorPosition = null;
    private bool _contextMenuOpened = false;
    private bool _openContextMenuOnClosed = false;
    private MouseButtonEventArgs? _openContextMenuEventArgs = null;

    public AppBarMenuFlyoutHelper()
    {
        _contextMenu.Closed += ContextMenu_Closed;
    }

    private void ContextMenu_Closed(object? sender, object? e)
    {
        _contextMenuOpened = false;
        if (_openContextMenuOnClosed && _openContextMenuEventArgs != null)
        {
            OpenAppBarMenu(_openContextMenuEventArgs);
            _openContextMenuOnClosed = false;
            _openContextMenuEventArgs = null;
        }
    }

    public void MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.Handled) return;

        _cursorPosition = PInvokeHelper.GetCursorPos();
    }

    public void MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (e.Handled) return;

        // If users have moved the cursor after right button down, we should not open the context menu.
        if (_cursorPosition != null && _cursorPosition != PInvokeHelper.GetCursorPos()) return;
        // This is workaround for a bug in WPF that element position will change if the old appbar menu is still open
        // (Pop up menu will be considered as part of that element which can cause wrong position calculation)
        // So we need to manually hide old appbar menu and open new appbar menu after it is closed.
        if (_contextMenuOpened)
        {
            _openContextMenuOnClosed = true;
            _openContextMenuEventArgs = e;
            _contextMenu.Hide();
        }
        else
        {
            OpenAppBarMenu(e);
        }
        _cursorPosition = null;
    }

    private void OpenAppBarMenu(MouseButtonEventArgs e)
    {
        var placement = ViewModel.DockMode switch
        {
            AppBarDockMode.Left => MenuFlyoutExPlacementMode.AppBarRight,
            AppBarDockMode.Right => MenuFlyoutExPlacementMode.AppBarLeft,
            AppBarDockMode.Top => MenuFlyoutExPlacementMode.AppBarBottom,
            AppBarDockMode.Bottom => MenuFlyoutExPlacementMode.AppBarTop,
            _ => throw new NotImplementedException()
        };
        _contextMenu.ShowAt(Element, new MenuFlyoutExOptions()
        {
            Placement = placement,
            Position = e.GetPosition(Element),
            Monitor = ViewModel.ActualMonitor
        });
        _contextMenuOpened = true;
        e.Handled = Handled;
    }

    public void Dispose()
    {
        _contextMenu.Hide();
        _contextMenu.Closed -= ContextMenu_Closed;
        _contextMenu.Items.Clear();
        _cursorPosition = null;
        _openContextMenuEventArgs = null;
    }
}
