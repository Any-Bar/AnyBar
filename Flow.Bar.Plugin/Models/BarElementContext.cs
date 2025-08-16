namespace Flow.Bar.Plugin;

/// <summary>
/// Context for bar elements of Flow Bar plugins.
/// </summary>
public class BarElementContext
{
    /// <summary>
    /// Unique identifier of the bar element.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Position of bar element.
    /// </summary>
    BarElementPosition position { get; }
}
