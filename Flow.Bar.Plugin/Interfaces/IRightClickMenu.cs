using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Flow.Bar.Plugin;

/// <summary>
/// Interface for Flow Bar plugins that have right-click context menus with menu items.
/// </summary>
public interface IRightClickMenu : IRightClickMenuBase
{
    /// <summary>
    /// Retrieves a list of menu items for the right-click context menu.
    /// </summary>
    /// <returns></returns>
    IList<MenuItem> GetRightClickMenuItems();
}

/// <summary>
/// Interface for Flow Bar plugins that have customized right-click context menus.
/// </summary>
public interface ICustomRightClickMenu : IRightClickMenuBase
{
    /// <summary>
    /// Retrieves style for the right-click context menu.
    /// </summary>
    /// <returns></returns>
    Style GetRightClickMenuStyle();

    /// <summary>
    /// Applies the template to the right-click context menu.
    /// </summary>
    void OnApplyRightClickMenuTemplate(ContextMenu menu);
}

/// <summary>
/// Base interface for Flow Bar plugins that have right-click context menus.
/// </summary>
/// <remarks>
/// Do not implement this interface directly.
/// Use <see cref="ICustomRightClickMenu"/> instead.
/// </remarks>
public interface IRightClickMenuBase
{
    /// <summary>
    /// Retrieves the popup mode for the right-click context menu.
    /// </summary>
    ContextMenuPopupMode RightClickMenuPopupMode { get; }
}
