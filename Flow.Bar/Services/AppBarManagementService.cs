using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Monitor;
using Flow.Bar.Models.UserSettings;
using Flow.Bar.Views;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Flow.Bar.Services;

#pragma warning disable CS0618 // Type or member is obsolete

public class AppBarManagementService(Settings settings)
{
    private readonly Settings _settings = settings;

    private readonly ConcurrentDictionary<int, AppBarWindow> AppBarWindowPairs = [];

    public void InitializeAllAppBarWindows()
    {
        foreach (var key in _settings.AppBars.Keys.OrderBy(k => k))
        {
            var appBar = _settings.AppBars[key];
            if (appBar.IsEnabled)
            {
                var barWindow = new AppBarWindow(appBar);
                barWindow.Show();
                AppBarWindowPairs.TryAdd(appBar.Order, barWindow);
            }
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

    public void SetEnabled(int order, bool isEnabled)
    {
        if (_settings.AppBars.TryGetValue(order, out var appBar))
        {
            appBar.IsEnabled = isEnabled;
            _settings.Save();
            if (AppBarWindowPairs.TryGetValue(order, out var appBarWindow))
            {
                if (isEnabled)
                {
                    appBarWindow.Show();
                }
                else
                {
                    appBarWindow.Close();
                    AppBarWindowPairs.TryRemove(order, out _);
                }
            }
            else
            {
                if (isEnabled)
                {
                    var newAppBarWindow = new AppBarWindow(appBar);
                    newAppBarWindow.Show();
                    AppBarWindowPairs.TryAdd(order, newAppBarWindow);
                }
            }
        }
    }
}
