namespace Flow.Bar.Plugin;

/// <summary>
/// Context for plugin initialization.
/// </summary>
public class PluginInitContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginInitContext"/> class.
    /// </summary>
    /// <param name="currentPluginMetadata"></param>
    /// <param name="api"></param>
    public PluginInitContext(PluginMetadata currentPluginMetadata, IPublicAPI api)
    {
        CurrentPluginMetadata = currentPluginMetadata;
        API = api;
    }

    /// <summary>
    /// The metadata of the plugin being initialized.
    /// </summary>
    public PluginMetadata CurrentPluginMetadata { get; internal set; }

    /// <summary>
    /// Public APIs for plugin invocation.
    /// </summary>
    public IPublicAPI API { get; set; }
}
