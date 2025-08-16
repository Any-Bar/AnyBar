using System.Windows.Input;

namespace Flow.Bar.Plugin.Interfaces;

/// <summary>
/// Interface for Flow Bar plugins that have right-click events.
/// </summary>
public interface IRightClick
{
    /// <summary>
    /// Invoked when the right mouse button is pressed on the bar element.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e);

    /// <summary>
    /// Invoked when the right mouse button is released on the bar element.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e);
}
