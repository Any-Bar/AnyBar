using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AnyBar.Enums;
using AnyBar.Helpers.Http;
using AnyBar.Helpers.Monitor;
using AnyBar.Models.AppBar;
using AnyBar.Models.Storage;

namespace AnyBar.Models.UserSettings;

public class Settings
{
    #region Storage

    private AnyBarJsonStorage<Settings> _storage = null!;

    public void SetStorage(AnyBarJsonStorage<Settings> storage)
    {
        _storage = storage;
    }

    public void Save()
    {
        _storage.Save();
    }

    #endregion

    public string Language { get; set; } = Constants.SystemLanguageCode;

    public bool StartOnSystemStartup { get; set; } = false;
    public bool UseLogonTaskForStartup { get; set; } = false;

    public bool AlwaysRunAsAdministrator { get; set; } = false;

    public bool HideSettingWindow { get; set; } = false;

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
                    ID = Constants.AnyBarPluginDateTimePluginId,
                    Name = "Date and time",
                }
            ]
        };
        return demoAppBar;
    }
#pragma warning restore CS0618 // Type or member is obsolete

    public HttpProxy Proxy { get; set; } = new HttpProxy();

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WindowBackdropType WindowBackdropType { get; set; } = WindowBackdropType.Mica;
}
