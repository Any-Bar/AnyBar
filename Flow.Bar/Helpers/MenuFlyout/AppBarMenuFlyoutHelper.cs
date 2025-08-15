using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Flow.Bar.Controls;
using Flow.Bar.Enums;
using Flow.Bar.Plugin;
using Flow.Bar.Views;
using Windows.Win32;
using Point = System.Drawing.Point;

namespace Flow.Bar.Helpers.MenuFlyout;

public class AppBarMenuFlyoutHelper : IDisposable
{
    public ItemCollection Items => _contextMenu.Items;
    public AppBarWindow Window { get; set; } = null!;

    private readonly bool _handled;
    private readonly ContextMenuPopupMode _popupMode;
    private readonly MenuFlyoutEx _contextMenu = new();
    private Point? _cursorPosition = null;
    private bool _contextMenuOpened = false;
    private bool _openContextMenuOnClosed = false;
    private MouseButtonEventArgs? _openContextMenuEventArgs = null;

    public AppBarMenuFlyoutHelper(bool handled = false,
        ContextMenuPopupMode popupMode = ContextMenuPopupMode.AlwaysPopup,
        Style? contextMenuStyle = null, Action<ContextMenu>? onApplyTemplate = null)
    {
        _handled = handled;
        _popupMode = popupMode;
        _contextMenu.PopupMode = popupMode;
        if (contextMenuStyle != null)
        {
            _contextMenu.MenuFlyoutPresenterStyle = contextMenuStyle;
        }
        if (onApplyTemplate != null)
        {
            _contextMenu.OnApplyTemplateAction = onApplyTemplate;
        }
        _contextMenu.Closed += ContextMenu_Closed;
    }

    private void ContextMenu_Closed(object? sender, object? e)
    {
        // Set context menu opened flag
        _contextMenuOpened = false;

        // If we are in AlwaysPopup mode and the context menu is closed, we should open it again
        if (_popupMode == ContextMenuPopupMode.AlwaysPopup &&
            _openContextMenuOnClosed && _openContextMenuEventArgs != null)
        {
            OpenAppBarMenu(_openContextMenuEventArgs);
            _openContextMenuOnClosed = false;
            _openContextMenuEventArgs = null;
        }
    }

    public void MouseButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.Handled) return;

        _cursorPosition = PInvokeHelper.GetCursorPos();
    }

    public void MouseButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (e.Handled) return;

        // If users have moved the cursor after right button down, we should not open the context menu.
        if (_cursorPosition != null && _cursorPosition != PInvokeHelper.GetCursorPos()) return;

        if (_popupMode == ContextMenuPopupMode.AlwaysPopup)
        {
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
        }
        else
        {
            if (_contextMenuOpened)
            {
                _contextMenu.Hide();
            }
            else
            {
                OpenAppBarMenu(e);
            }
        }

        _cursorPosition = null;
    }

    private void OpenAppBarMenu(MouseButtonEventArgs e)
    {
        var placement = Window.ViewModel.DockMode switch
        {
            AppBarDockMode.Left => MenuFlyoutExPlacementMode.AppBarRight,
            AppBarDockMode.Right => MenuFlyoutExPlacementMode.AppBarLeft,
            AppBarDockMode.Top => MenuFlyoutExPlacementMode.AppBarBottom,
            AppBarDockMode.Bottom => MenuFlyoutExPlacementMode.AppBarTop,
            _ => throw new NotImplementedException()
        };
        _contextMenu.ShowAt(Window, new MenuFlyoutExOptions()
        {
            Placement = placement,
            Position = e.GetPosition(Window),
            Window = Window
        });
        e.Handled = _handled;

        // Set context menu opened flag
        _contextMenuOpened = true;
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
