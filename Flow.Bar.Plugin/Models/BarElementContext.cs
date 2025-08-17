namespace Flow.Bar.Plugin;

/// <summary>
/// Context for bar elements of Flow Bar plugins.
/// </summary>
public sealed class BarElementContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BarElementContext"/> class.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="position"></param>
    /// <param name="dockedHeightOrWidth"></param>
    public BarElementContext(string id, BarElementPosition position, int dockedHeightOrWidth)
    {
        Id = id;
        Position = position;
        DockedHeightOrWidth = dockedHeightOrWidth;
    }

    /// <summary>
    /// Runtime identifier of the bar element.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Position of bar element.
    /// </summary>
    public BarElementPosition Position { get; set; }

    /// <summary>
    /// Height or width of the bar element when docked.
    /// </summary>
    public int DockedHeightOrWidth { get; set; }
}
