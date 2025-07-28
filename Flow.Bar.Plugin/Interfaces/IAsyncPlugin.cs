using System.Threading.Tasks;
using System.Windows;

namespace Flow.Bar.Plugin;

public interface IAsyncPlugin
{
    FrameworkElement GetBarElement(BarElementPosition position);

    Task InitAsync(PluginInitContext context);
}
