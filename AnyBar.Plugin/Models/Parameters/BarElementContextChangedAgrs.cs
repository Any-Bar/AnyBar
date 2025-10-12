namespace AnyBar.Plugin;

/// <summary>
/// Arguments for the bar element context changed event.
/// </summary>
public sealed class BarElementContextChangedAgrs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BarElementContextChangedAgrs"/> class.
    /// </summary>
    /// <param name="context"></param>
    public BarElementContextChangedAgrs(BarElementContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Bar element context that has changed.
    /// </summary>
    public BarElementContext Context { get; }
}
