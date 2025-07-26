using Flow.Bar.Models.UserSettings;
using Flow.Bar.Plugin;

namespace Flow.Bar.Models;

public class PublicAPIInstance(Settings settings) : IPublicAPI
{
    private readonly Settings _settings = settings;

    private readonly Lock _saveSettingsLock = new();

    public void SaveAppAllSettings()
    {
        lock (_saveSettingsLock)
        {
            _settings.Save();
        }
    }
}
