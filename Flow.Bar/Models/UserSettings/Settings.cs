using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Storage;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Flow.Bar.Models.UserSettings;

public class Settings
{
    #region Storage

    private FlowBarJsonStorage<Settings> _storage = null!;

    public void SetStorage(FlowBarJsonStorage<Settings> storage)
    {
        _storage = storage;
    }

    public void Save()
    {
        _storage.Save();
    }

    #endregion

    public string Language { get; set; } = Constants.SystemLanguageCode;

    public ConcurrentDictionary<int, AppBarModel> AppBars { get; set; } = new(
        [new KeyValuePair<int, AppBarModel>(0, new AppBarModel() { ID = 0 })]
    );
}
