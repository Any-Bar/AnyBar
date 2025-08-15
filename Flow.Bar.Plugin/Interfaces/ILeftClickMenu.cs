using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Flow.Bar.Plugin;

/// <summary>
/// Interface for Flow Bar plugins that have left-click context menus with menu items.
/// </summary>
public interface ILeftClickMenu : ILeftClickMenuBase
{
    /// <summary>
    /// Retrieves a list of menu items for the left-click context menu.
    /// </summary>
    /// <returns></returns>
    IList<MenuItem> GetLeftClickMenuItems();
}

/// <summary>
/// Interface for Flow Bar plugins that have customized left-click context menus.
/// </summary>
public interface ICustomLeftClickMenu : ILeftClickMenuBase
{
    /// <summary>
    /// Retrieves style for the left-click context menu.
    /// </summary>
    /// <returns></returns>
    Style GetLeftClickMenuMenuStyle();

    /// <summary>
    /// Applies the template to the left-click context menu.
    /// </summary>
    void OnApplyLeftClickMenuTemplate(ContextMenu menu);
}

/// <summary>
/// Base interface for Flow Bar plugins that have left-click context menus.
/// </summary>
/// <remarks>
/// Do not implement this interface directly.
/// Use <see cref="ICustomLeftClickMenu"/> instead.
/// </remarks>
public interface ILeftClickMenuBase
{
    /// <summary>
    /// Retrieves the popup mode for the left-click context menu.
    /// </summary>
    ContextMenuPopupMode LeftClickMenuPopupMode { get; }
}
