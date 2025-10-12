using System.Threading.Tasks;

namespace AnyBar.Plugin;

/// <summary>
/// Interface for AnyBar plugins.
/// </summary>
public interface IPlugin : IAsyncPlugin
{
    /// <summary>
    /// Initializes the plugin with the provided context.
    /// </summary>
    /// <param name="context"></param>
    void Init(PluginInitContext context);

    Task IAsyncPlugin.InitAsync(PluginInitContext context) => Task.Run(() => Init(context));
}
