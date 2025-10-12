using System;
using System.Windows;
using System.Windows.Controls;
using AnyBar.Controls;
using iNKORE.UI.WPF.Helpers;

namespace AnyBar.Helpers.MenuFlyout;

public class DoubleMenuFlyoutHelper<T> : IDisposable
{
    public ItemCollection Items => _contextMenu.Items;

    private readonly MenuFlyoutEx _contextMenu = new();
    private Button? _button = null;
    private T? _plugin = default;
    private bool _openSecondContextMenu = new();

    private readonly MenuFlyoutEx _secondContextMenu = new();
    private readonly string _secondMenuButtonName;
    private readonly Action<T> _secondMenuButtonAction;

    public DoubleMenuFlyoutHelper(
        double contextMenuWidth,
        double secondContextMenuWidth,
        double secondContextMenuHeight,
        Style secondContextMenuStyle,
        string secondMenuButtonName,
        Action<T> secondMenuButtonAction)
    {
        _contextMenu.Placement = MenuFlyoutExPlacementMode.BottomEdgeAlignedRight;
        _contextMenu.Width = contextMenuWidth;
        _secondContextMenu.Placement = MenuFlyoutExPlacementMode.BottomEdgeAlignedRight;
        _secondContextMenu.Width = secondContextMenuWidth;
        _secondContextMenu.Height = secondContextMenuHeight;
        _secondContextMenu.MenuFlyoutPresenterStyle = secondContextMenuStyle;
        _secondMenuButtonName = secondMenuButtonName;
        _secondMenuButtonAction = secondMenuButtonAction;
        _secondContextMenu.OnApplyTemplateAction = OnApplyTemplate;

        _contextMenu.Closed += ContextMenu_Closed;
        _secondContextMenu.Closed += SecondContextMenu_Closed;
    }

    public void OnApplyTemplate(ContextMenu menu)
    {
        if (menu.GetTemplateChild<Button>(_secondMenuButtonName) is { } button)
        {
            button.Click += (s, e) => SecondMenuButtonClick();
        }
    }

    public void ButtonClick(Button button)
    {
        _plugin = default;
        _button = null;
        if (button.Tag is not T plugin) throw new ArgumentException($"{nameof(Button)}.{nameof(Button.Tag)} must be of type {nameof(T)}", nameof(button));
        _button = button;
        _plugin = plugin;
        _contextMenu.ShowAt(button);
    }

    public void MenuItemClick()
    {
        if (_plugin != null && _button != null)
        {
            _openSecondContextMenu = true;
        }
    }

    private void ContextMenu_Closed(object? sender, object? e)
    {
        if (_openSecondContextMenu)
        {
            _openSecondContextMenu = false;
            if (_plugin != null && _button != null)
            {
                _secondContextMenu.ShowAt(_button);
            }
        }
        else
        {
            _plugin = default;
            _button = null;
        }
    }

    private void SecondContextMenu_Closed(object? sender, object? e)
    {
        _openSecondContextMenu = false;
        _plugin = default;
        _button = null;
    }

    private void SecondMenuButtonClick()
    {
        var oldPlugin = _plugin;
        _secondContextMenu.Hide();
        if (oldPlugin != null)
        {
            _secondMenuButtonAction(oldPlugin);
        }
    }

    public void Dispose()
    {
        _secondContextMenu.Hide();
        _contextMenu.Hide();
        _contextMenu.Closed -= ContextMenu_Closed;
        _contextMenu.Items.Clear();
        _secondContextMenu.Closed -= SecondContextMenu_Closed;
        _plugin = default;
        _button = null;
    }
}
