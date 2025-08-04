using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Models.Monitor;
using Flow.Bar.Models.UserSettings;
using Flow.Bar.Views;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Flow.Bar.Services;

#pragma warning disable CS0618 // Type or member is obsolete

public class AppBarManagementService(Settings settings)
{
    private readonly Settings _settings = settings;

    private readonly Dictionary<int, AppBarWindow> AppBarWindowPairs = [];

    private readonly Lock _appBarWindowLock = new();

    public void InitializeAllAppBarWindows()
    {
        foreach (var key in _settings.AppBars.Keys.OrderBy(k => k))
        {
            var appBar = _settings.AppBars[key];
            if (appBar.IsEnabled)
            {
                lock (_appBarWindowLock)
                {
                    var barWindow = new AppBarWindow(appBar);
                    barWindow.Show();
                    AppBarWindowPairs.TryAdd(appBar.Order, barWindow);
                }
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
            lock (_appBarWindowLock)
            {
                if (AppBarWindowPairs.TryGetValue(order, out var appBarWindow))
                {
                    if (isEnabled)
                    {
                        appBarWindow.Show();
                    }
                    else
                    {
                        appBarWindow.Close();
                        AppBarWindowPairs.Remove(order);
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

    public void SetDockMode(int order, AppBarDockMode dockMode)
    {
        if (_settings.AppBars.TryGetValue(order, out var appBar))
        {
            appBar.DockMode = dockMode;
            _settings.Save();
            lock (_appBarWindowLock)
            {
                if (AppBarWindowPairs.TryGetValue(order, out var appBarWindow))
                {
                    appBarWindow.ViewModel.DockMode = dockMode;
                }
            }
        }
    }

    public void SetMonitorName(int order, string? monitorName)
    {
        if (_settings.AppBars.TryGetValue(order, out var appBar))
        {
            appBar.MonitorName = monitorName;
            _settings.Save();
            lock (_appBarWindowLock)
            {
                if (AppBarWindowPairs.TryGetValue(order, out var appBarWindow))
                {
                    appBarWindow.ViewModel.MonitorName = monitorName;
                }
            }
        }
    }

    public void SetFollowSystemTaskbarWidthOrHeight(int order, bool followSystemTaskbarWidthOrHeight)
    {
        if (_settings.AppBars.TryGetValue(order, out var appBar))
        {
            appBar.FollowSystemTaskbarWidthOrHeight = followSystemTaskbarWidthOrHeight;
            _settings.Save();
            lock (_appBarWindowLock)
            {
                if (AppBarWindowPairs.TryGetValue(order, out var appBarWindow))
                {
                    appBarWindow.ViewModel.FollowSystemTaskbarWidthOrHeight = followSystemTaskbarWidthOrHeight;
                }
            }
        }
    }

    public void SetDockedWidthOrHeight(int order, int dockedWidthOrHeight)
    {
        if (_settings.AppBars.TryGetValue(order, out var appBar))
        {
            appBar.DockedWidthOrHeight = dockedWidthOrHeight;
            _settings.Save();
            lock (_appBarWindowLock)
            {
                if (AppBarWindowPairs.TryGetValue(order, out var appBarWindow))
                {
                    appBarWindow.ViewModel.DockedWidthOrHeight = dockedWidthOrHeight;
                }
            }
        }
    }

    public void SetIsResizable(int order, bool isResizable)
    {
        if (_settings.AppBars.TryGetValue(order, out var appBar))
        {
            appBar.IsResizable = isResizable;
            _settings.Save();
            lock (_appBarWindowLock)
            {
                if (AppBarWindowPairs.TryGetValue(order, out var appBarWindow))
                {
                    appBarWindow.ViewModel.IsResizable = isResizable;
                }
            }
        }
    }
}
