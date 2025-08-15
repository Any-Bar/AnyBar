namespace Flow.Bar.Plugin.Interfaces;

/// <summary>
/// Interface for bar elements of Flow Bar plugins.
/// </summary>
/// <remarks>
/// This interface should be implemented by bar elements returned from <see cref="IAsyncPlugin.GetBarElement(BarElementPosition)"/>.
/// </remarks>
public interface IDockModeChanged
{
    /// <summary>
    /// Invoked when the dock mode changes.
    /// </summary>
    /// <param name="position"></param>
    void OnDockModeChanged(BarElementPosition position);
}
