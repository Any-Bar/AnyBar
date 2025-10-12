using System.Threading.Tasks;
using System.Windows;

namespace AnyBar.Plugin;

/// <summary>
/// Interface for AnyBar plugins that support asynchronous initialization.
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
    /// Create the bar element for the specified position.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    FrameworkElement CreateBarElement(BarElementContext context);

    /// <summary>
    /// Delete the bar element with the runtime identifier.
    /// </summary>
    /// <param name="id"></param>
    void DeleteBarElement(string id);

    /// <summary>
    /// Invoked when the bar element context changes.
    /// </summary>
    /// <param name="args"></param>
    void OnBarElementContextChanged(BarElementContextChangedAgrs args);
}
