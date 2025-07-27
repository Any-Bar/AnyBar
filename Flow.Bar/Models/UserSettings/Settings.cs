using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Models.Storage;
using System.Collections.Concurrent;

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

    public bool HideSettingWindow { get; set; } = false;

    private ConcurrentDictionary<int, AppBarModel>? _appBars = null;
    public ConcurrentDictionary<int, AppBarModel> AppBars
    {
        get
        {
            if (_appBars == null)
            {
                _appBars = new();
                var defaultAppBar = new AppBarModel
                {
                    Order = 0,
                    DockMode = AppBarDockMode.Top,
                    MonitorName = null,
                    DockedWidthOrHeight = null,
                    IsResizable = false,
                    RightOrBottomPluginControls =
                    [
                        new()
                        {
                            Order = 0,
                            ID = "3675a0dd-af3b-412f-b257-5e004dea2bd0", // Flow.Bar.Plugin.Clock
                        }
                    ]
                };
                _appBars.TryAdd(0, defaultAppBar);
            }
            return _appBars;
        }
        set => _appBars = value;
    }
}
