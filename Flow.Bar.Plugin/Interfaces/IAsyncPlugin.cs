using System.Threading.Tasks;
using System.Windows;

namespace Flow.Bar.Plugin;

/// <summary>
/// Interface for Flow Bar plugins that support asynchronous initialization.
/// </summary>
public interface IAsyncPlugin
{
    /// <summary>
    /// Initializes the plugin with the provided context asynchronously.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task InitAsync(PluginInitContext context);

    /// <summary>
    /// Get the bar element for the specified position.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    FrameworkElement GetBarElement(BarElementContext context);
}
