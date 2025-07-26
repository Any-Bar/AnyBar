using System.Threading.Tasks;
using System.Windows;

namespace Flow.Bar.Plugin;

public interface IAsyncPlugin
{
    FrameworkElement GetBarElement();

    Task InitAsync(PluginInitContext context);
}
