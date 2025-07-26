using Flow.Bar.Models.Storage;

namespace Flow.Bar.Models.UserSettings;

public class Settings
{
    private FlowBarJsonStorage<Settings> _storage = null!;

    public void SetStorage(FlowBarJsonStorage<Settings> storage)
    {
        _storage = storage;
    }

    public void Save()
    {
        _storage.Save();
    }
}
