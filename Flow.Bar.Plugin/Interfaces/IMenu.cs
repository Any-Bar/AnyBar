using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Flow.Bar.Plugin;

/// <summary>
/// Interface for context menus with menu items.
/// </summary>
/// <remarks>
/// Do not implement this interface directly.
/// Use <see cref="ILeftClickMenu"/> or <see cref="IRightClickMenu"/> instead.
/// </remarks>
public interface IMenu
{
    /// <summary>
    /// Retrieves a list of menu items for the context menu.
    /// </summary>
    /// <returns></returns>
    IList<MenuItem> GetRightClickMenuItems();
}

/// <summary>
/// Interface for customized context menus.
/// </summary>
/// <remarks>
/// Do not implement this interface directly.
/// Use <see cref="ICustomLeftClickMenu"/> or <see cref="ICustomRightClickMenu"/> instead.
/// </remarks>
public interface ICustomMenu
{
    /// <summary>
    /// Retrieves style for the context menu.
    /// </summary>
    /// <returns></returns>
    Style GetContextMenuStyle();

    /// <summary>
    /// Applies the template to the context menu.
    /// </summary>
    void OnApplyTemplate(ContextMenu menu);
}
