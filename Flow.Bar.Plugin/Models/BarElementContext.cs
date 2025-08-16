namespace Flow.Bar.Plugin;

/// <summary>
/// Context for bar elements of Flow Bar plugins.
/// </summary>
public sealed class BarElementContext
{
    /// <summary>
    /// Runtime identifier of the bar element.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// Position of bar element.
    /// </summary>
    BarElementPosition Position { get; set; }
}
