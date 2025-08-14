using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Flow.Bar.Plugin;

/// <summary>
/// Interface for right-click context menus with menu items in Flow Bar plugins.
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
/// Interface for customized right-click context menus in Flow Bar plugins.
/// </summary>
public interface ICustomRightClickMenu : IRightClickMenuBase
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

/// <summary>
/// Base interface for right-click context menus in Flow Bar plugins.
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
