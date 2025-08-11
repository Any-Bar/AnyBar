using Flow.Bar.Controls;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using Point = System.Drawing.Point;

namespace Flow.Bar.Helper.MenuFlyout;

public class AppBarMenuFlyoutHelper : IDisposable
{
    public ItemCollection Items => _contextMenu.Items;
    public Action<MenuFlyoutEx, MouseButtonEventArgs>? ShowMenu { get; set; } = null;

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

    public void MouseRightButtonDown(MouseButtonEventArgs e)
    {
        if (e.Handled) return;

        _cursorPosition = Win32Helper.GetCursorPos();
    }

    public void MouseRightButtonUp(MouseButtonEventArgs e)
    {
        if (e.Handled) return;

        // If users have moved the cursor after right button down, we should not open the context menu.
        if (_cursorPosition != null && _cursorPosition != Win32Helper.GetCursorPos()) return;
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
        ShowMenu?.Invoke(_contextMenu, e);
        _contextMenuOpened = true;
        e.Handled = true;
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
