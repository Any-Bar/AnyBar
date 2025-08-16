using System.Windows.Input;

namespace Flow.Bar.Plugin;

/// <summary>
/// Interface for Flow Bar plugins that have right-click events.
/// </summary>
public interface IRightClick
{
    /// <summary>
    /// Invoked when the right mouse button is pressed on the bar element.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnMouseRightButtonDown(BarElementContext context, object sender, MouseButtonEventArgs e);

    /// <summary>
    /// Invoked when the right mouse button is released on the bar element.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnMouseRightButtonUp(BarElementContext context, object sender, MouseButtonEventArgs e);
}
