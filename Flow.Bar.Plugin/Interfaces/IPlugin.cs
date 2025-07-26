using System.Threading.Tasks;

namespace Flow.Bar.Plugin;

public interface IPlugin : IAsyncPlugin
{
    void Init(PluginInitContext context);

    Task IAsyncPlugin.InitAsync(PluginInitContext context) => Task.Run(() => Init(context));
}
