using Flow.Bar.Controls;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Flow.Bar.Helper.MenuFlyout;

public class PluginUninstallationMenuFlyoutHelper<T> : IDisposable
{
    public ItemCollection Items => _contextMenu.Items;

    private readonly MenuFlyoutEx _contextMenu = new();
    private Button? _button = null;
    private T? _plugin = default;
    private bool _openUninstallConfirmationContextMenu = new();

    private readonly MenuFlyoutEx _uninstallContextMenu = new();
    private readonly Action<T> _uninstallationAction;

    public PluginUninstallationMenuFlyoutHelper(double contextMenuWidth, double secondContextMenuWidth, double secondContextMenuHeight,
        Style secondContextMenuStyle, string uninstallButtonName, Action<T> uninstallationAction)
    {
        _contextMenu.Width = contextMenuWidth;
        _uninstallContextMenu.Width = secondContextMenuWidth;
        _uninstallContextMenu.Height = secondContextMenuHeight;
        _uninstallContextMenu.MenuFlyoutPresenterStyle = secondContextMenuStyle;
        _uninstallationAction = uninstallationAction;
        _uninstallContextMenu.ButtonClickEvents.Add(uninstallButtonName, (s, e) => UninstallButton_Click());

        _contextMenu.Closed += ContextMenu_Closed;
        _uninstallContextMenu.Closed += UninstallConfirmationContextMenu_Closed;
    }

    public void ButtonClick(object sender, RoutedEventArgs e)
    {
        _plugin = default;
        _button = null;
        if (sender is not Button button) return;
        if (button.Tag is not T plugin) return;
        _button = button;
        _plugin = plugin;
        _contextMenu.ShowAt(button, new MenuFlyoutExOptions()
        {
            Placement = MenuFlyoutExPlacementMode.BottomEdgeAlignedRight
        });
    }

    public void UninstallItemClick(object sender, RoutedEventArgs e)
    {
        if (_plugin != null && _button != null)
        {
            _openUninstallConfirmationContextMenu = true;
        }
    }

    private void ContextMenu_Closed(object? sender, object? e)
    {
        if (_openUninstallConfirmationContextMenu)
        {
            _openUninstallConfirmationContextMenu = false;
            if (_plugin != null && _button != null)
            {
                _uninstallContextMenu.ShowAt(_button, new MenuFlyoutExOptions()
                {
                    Placement = MenuFlyoutExPlacementMode.BottomEdgeAlignedRight
                });
            }
        }
        else
        {
            _plugin = default;
            _button = null;
        }
    }

    private void UninstallButton_Click()
    {
        var oldPlugin = _plugin;
        _uninstallContextMenu.Hide();
        if (oldPlugin != null)
        {
            _uninstallationAction(oldPlugin);
        }
    }

    private void UninstallConfirmationContextMenu_Closed(object? sender, object? e)
    {
        _openUninstallConfirmationContextMenu = false;
        _plugin = default;
        _button = null;
    }

    public void Dispose()
    {
        _uninstallContextMenu.Hide();
        _contextMenu.Hide();
        _contextMenu.Closed -= ContextMenu_Closed;
        _contextMenu.Items.Clear();
        _uninstallContextMenu.Closed -= UninstallConfirmationContextMenu_Closed;
        _plugin = default;
        _button = null;
    }
}
