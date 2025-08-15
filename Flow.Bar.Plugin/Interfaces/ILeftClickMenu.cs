using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Flow.Bar.Plugin;

/// <summary>
/// Interface for left-click context menus with menu items in Flow Bar plugins.
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
/// Interface for customized left-click context menus in Flow Bar plugins.
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
/// Base interface for left-click context menus in Flow Bar plugins.
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
