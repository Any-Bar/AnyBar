using System.Threading.Tasks;

namespace Flow.Bar.Plugin;

/// <summary>
/// Interface for Flow Bar plugins.
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
