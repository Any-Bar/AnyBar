using System.Globalization;

namespace AnyBar.Plugin;

/// <summary>
/// Interface for AnyBar plugins that support internationalization.
/// </summary>
public interface IPluginI18n
{
    /// <summary>
    /// Get a localized version of the plugin's title.
    /// </summary>
    string GetTranslatedPluginTitle();

    /// <summary>
    /// Get a localized version of the plugin's description.
    /// </summary>
    string GetTranslatedPluginDescription();

    /// <summary>
    /// The method will be invoked when language of AnyBar changed.
    /// </summary>
    void OnCultureInfoChanged(CultureInfo newCulture)
    {

    }
}
