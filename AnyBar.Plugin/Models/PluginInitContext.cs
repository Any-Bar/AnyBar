namespace AnyBar.Plugin;

/// <summary>
/// Context for AnyBar plugin initialization.
/// </summary>
public sealed class PluginInitContext
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
    public PluginMetadata CurrentPluginMetadata { get; }

    /// <summary>
    /// Public APIs for plugin invocation.
    /// </summary>
    public IPublicAPI API { get; }
}
