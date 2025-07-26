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
}
