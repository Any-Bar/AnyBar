using Flow.Bar.Enums;
using Flow.Bar.Helper.Http;
using Flow.Bar.Helper.Monitor;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Storage;
using System;
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

    public bool HideSettingWindow { get; set; } = false;

    public bool EnableTransparencyEffects { get; set; } = true;

    public bool EnableAnimationEffects { get; set; } = true;

    private Dictionary<int, AppBarModel>? _appBars = null;
    [Obsolete("This property is for storage only. Please use AppBarManagementService instead of calling this property directly.")]
    public Dictionary<int, AppBarModel> AppBars
    {
        get
        {
            if (_appBars == null)
            {
                _appBars = [];
                var demoAppBar = GetDemoAppBar();
                _appBars.TryAdd(0, demoAppBar);
            }

            return _appBars;
        }
        set => _appBars = value;
    }

#pragma warning disable CS0618 // Type or member is obsolete
    private static AppBarModel GetDemoAppBar()
    {
        var monitor = MonitorInfoHelper.GetMonitorInfoFromName(null);
        var dockedWidthOrHeight = MonitorInfoHelper.GetMonitorTaskBarWidthOrHeight(monitor);
        var demoAppBar = new AppBarModel
        {
            Order = 0,
            Name = "Demo Appbar",
            DockMode = AppBarDockMode.Top,
            MonitorName = null,
            FollowSystemTaskbarWidthOrHeight = true,
            DockedWidthOrHeight = dockedWidthOrHeight,
            IsResizable = false,
            RightOrBottomBarElements =
            [
                new()
                {
                    Order = 0,
                    ID = Constants.FlowBarPluginClockPluginId,
                    Name = "Clock",
                }
            ]
        };
        return demoAppBar;
    }
#pragma warning restore CS0618 // Type or member is obsolete

    public HttpProxy Proxy { get; set; } = new HttpProxy();
}
