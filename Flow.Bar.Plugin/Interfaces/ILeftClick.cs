using System.Windows.Input;

namespace Flow.Bar.Plugin;

/// <summary>
/// Interface for Flow Bar plugins that have left-click events.
/// </summary>
public interface ILeftClick
{
    /// <summary>
    /// Invoked when the left mouse button is pressed on the bar element.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e);

    /// <summary>
    /// Invoked when the left mouse button is released on the bar element.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e);
}
