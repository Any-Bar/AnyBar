using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Monitor;
using Flow.Bar.Models.UserSettings;
using Flow.Bar.Views;
using System.Collections.Generic;
using System.Linq;

namespace Flow.Bar.Services;

#pragma warning disable CS0618 // Type or member is obsolete

public class AppBarManagementService(Settings settings)
{
    private readonly Settings _settings = settings;

    public void InitializeAllAppBarWindows()
    {
        foreach (var key in _settings.AppBars.Keys.OrderBy(k => k))
        {
            var barWindow = new AppBarWindow(_settings.AppBars[key]);
            barWindow.Show();
        }
    }

    public List<MonitorNameLocalized> GetAllMonitorNames()
    {
        return MonitorNameLocalized.GetValues(_settings.AppBars.Values);
    }

    public List<AppBarModel> GetAllAppBars()
    {
        return [.. _settings.AppBars.Values.OrderBy(bar => bar.Order)];
    }
}
