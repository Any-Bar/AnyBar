using System.Collections.Generic;
using System.Windows.Controls;

namespace Flow.Bar.Plugin;

/// <summary>
/// Interface for right-click context menus in Flow Bar plugins.
/// </summary>
public interface IRightClickMenu
{
    /// <summary>
    /// Retrieves a list of menu items for the right-click context menu.
    /// </summary>
    /// <returns></returns>
    IList<MenuItem> GetRightClickMenuItems();
}
