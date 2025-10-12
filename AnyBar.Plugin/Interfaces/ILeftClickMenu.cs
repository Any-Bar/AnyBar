using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AnyBar.Plugin;

#pragma warning disable CS0618 // Type or member is obsolete

/// <summary>
/// Interface for AnyBar plugins that have left-click context menus with menu items.
/// </summary>
public interface ILeftClickMenu : ILeftClickMenuBase
{
    /// <summary>
    /// Retrieves a list of menu items for the left-click context menu.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    IList<MenuItem> GetLeftClickMenuItems(BarElementContext context);
}

/// <summary>
/// Interface for AnyBar plugins that have customized left-click context menus.
/// </summary>
public interface ICustomLeftClickMenu : ILeftClickMenuBase
{
    /// <summary>
    /// Retrieves style for the left-click context menu.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Style GetLeftClickMenuMenuStyle(BarElementContext context);

    /// <summary>
    /// Applies the template to the left-click context menu.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="menu"></param>
    void OnApplyLeftClickMenuTemplate(BarElementContext context, ContextMenu menu);
}

/// <summary>
/// Base interface for AnyBar plugins that have left-click context menus.
/// </summary>
/// <remarks>
/// Do not implement this interface directly.
/// Please use <see cref="ILeftClickMenu"/> or <see cref="ICustomLeftClickMenu"/> instead.
/// </remarks>
[Obsolete("Do not implement this interface directly. Please use ILeftClickMenu or ICustomLeftClickMenu instead.")]
public interface ILeftClickMenuBase
{
    /// <summary>
    /// Retrieves the popup mode for the left-click context menu.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    ContextMenuPopupMode GetLeftClickMenuPopupMode(BarElementContext context);
}
