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
            var model = _settings.AppBars[key];
            if (model.IsEnabled)
            {
                lock (_appBarWindowLock)
                {
                    var barWindow = new AppBarWindow(model);
                    barWindow.Show();
                    AppBarWindowPairs.TryAdd(model.Order, barWindow);
                }
            }
        }
    }

    public List<MonitorNameLocalized> GetAllMonitorNames(bool includeSettingMonitors)
    {
        return MonitorNameLocalized.GetValues(includeSettingMonitors ? _settings.AppBars.Values : null);
    }

    public List<AppBarModel> GetAllAppBars()
    {
        return [.. _settings.AppBars.Values.OrderBy(bar => bar.Order)];
    }

    public void AddAppBar(AppBarModel model)
    {
        model.Order = _settings.AppBars.Keys.Max() + 1;
        _settings.AppBars.TryAdd(model.Order, model);
        _settings.Save();
        lock (_appBarWindowLock)
        {
            var newAppBarWindow = new AppBarWindow(model);
            newAppBarWindow.Show();
            AppBarWindowPairs.TryAdd(model.Order, newAppBarWindow);
        }
    }

    public void SetEnabled(int order, bool isEnabled)
    {
        if (_settings.AppBars.TryGetValue(order, out var model))
        {
            model.IsEnabled = isEnabled;
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
                        var newAppBarWindow = new AppBarWindow(model);
                        newAppBarWindow.Show();
                        AppBarWindowPairs.TryAdd(order, newAppBarWindow);
                    }
                }
            }
        }
    }

    public void SetDockMode(int order, AppBarDockMode dockMode)
    {
        if (_settings.AppBars.TryGetValue(order, out var model))
        {
            model.DockMode = dockMode;
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
        if (_settings.AppBars.TryGetValue(order, out var model))
        {
            model.MonitorName = monitorName;
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
        if (_settings.AppBars.TryGetValue(order, out var model))
        {
            model.FollowSystemTaskbarWidthOrHeight = followSystemTaskbarWidthOrHeight;
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
        if (_settings.AppBars.TryGetValue(order, out var model))
        {
            model.DockedWidthOrHeight = dockedWidthOrHeight;
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
        if (_settings.AppBars.TryGetValue(order, out var model))
        {
            model.IsResizable = isResizable;
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
