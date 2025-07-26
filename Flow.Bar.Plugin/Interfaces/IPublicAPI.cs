namespace Flow.Bar.Plugin;

/// <summary>
/// Public APIs that plugin can use
/// </summary>
public interface IPublicAPI
{
    /// <summary>
    /// Save everything, all of Flow Bar and plugins' data and settings
    /// </summary>
    void SaveAppAllSettings();

    /// <summary>
    /// Get translation of current language
    /// You need to implement IPluginI18n if you want to support multiple languages for your plugin
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    string GetTranslation(string key);
}
