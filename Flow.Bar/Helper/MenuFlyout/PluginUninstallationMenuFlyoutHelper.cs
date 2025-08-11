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
    private bool _openUninstallationContextMenu = new();

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
        _uninstallContextMenu.ButtonClickEvents.Add(uninstallButtonName, (s, e) => UninstallButtonClick());

        _contextMenu.Closed += ContextMenu_Closed;
        _uninstallContextMenu.Closed += UninstallContextMenu_Closed;
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
            _openUninstallationContextMenu = true;
        }
    }

    private void ContextMenu_Closed(object? sender, object? e)
    {
        if (_openUninstallationContextMenu)
        {
            _openUninstallationContextMenu = false;
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

    private void UninstallContextMenu_Closed(object? sender, object? e)
    {
        _openUninstallationContextMenu = false;
        _plugin = default;
        _button = null;
    }

    private void UninstallButtonClick()
    {
        var oldPlugin = _plugin;
        _uninstallContextMenu.Hide();
        if (oldPlugin != null)
        {
            _uninstallationAction(oldPlugin);
        }
    }

    public void Dispose()
    {
        _uninstallContextMenu.Hide();
        _contextMenu.Hide();
        _contextMenu.Closed -= ContextMenu_Closed;
        _contextMenu.Items.Clear();
        _uninstallContextMenu.Closed -= UninstallContextMenu_Closed;
        _plugin = default;
        _button = null;
    }
}
