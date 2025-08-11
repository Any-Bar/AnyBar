using Flow.Bar.Extensions.Enumerable;
using Flow.Bar.Helper.Plugins;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Models.Monitor;
using Flow.Bar.Models.UserSettings;
using Flow.Bar.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        lock (_appBarWindowLock)
        {
            StartAppBars([.. _settings.AppBars.Values]);
        }
    }

    public List<MonitorNameLocalized> GetAllMonitorNames(bool includeSettingMonitors)
    {
        lock (_appBarWindowLock)
        {
            return MonitorNameLocalized.GetValues(includeSettingMonitors ? _settings.AppBars.Values : null);
        }
    }

    #region AppBar Management

    #region List Management

    public List<AppBarModel> GetAllAppBars()
    {
        lock (_appBarWindowLock)
        {
            return [.. _settings.AppBars.Values.OrderBy(bar => bar.Order)];
        }
    }

    public void AddAppBar(AppBarModel model, Action<AppBarModel> added)
    {
        lock (_appBarWindowLock)
        {
            model.Order = _settings.AppBars.Keys.GetMax();
            if (_settings.AppBars.TryAdd(model.Order, model))
            {
                added(model);
                _settings.Save();
                var newAppBarWindow = new AppBarWindow(model);
                newAppBarWindow.Show();
                AppBarWindowPairs.TryAdd(model.Order, newAppBarWindow);
            }
        }
    }

    public void RemoveAppBar(int order)
    {
        lock (_appBarWindowLock)
        {
            if (_settings.AppBars.RemoveOrder(order, AppBarWindowPairs, (appBarWindow) =>
            {
                appBarWindow.Close();
            }))
            {
                _settings.Save();
            }
        }
    }

    public void ChangeAppBarOrder(int oldIndex, int newIndex, int itemsCount)
    {
        lock (_appBarWindowLock)
        {
            if (_settings.AppBars.Move(oldIndex, newIndex, itemsCount))
            {
                _settings.Save();
                RestartAppBarsFrom(Math.Min(oldIndex, newIndex) + 1);
            }
        }
    }

    #endregion

    #region Model Management

    public void SetName(int order, string name)
    {
        lock (_appBarWindowLock)
        {
            if (_settings.AppBars.TryGetValue(order, out var model))
            {
                model.Name = name;
                _settings.Save();
            }
        }
    }

    public void SetEnabled(int order, bool isEnabled)
    {
        lock (_appBarWindowLock)
        {
            if (_settings.AppBars.TryGetValue(order, out var model))
            {
                model.IsEnabled = isEnabled;
                _settings.Save();
                if (AppBarWindowPairs.TryGetValue(order, out var appBarWindow))
                {
                    if (isEnabled)
                    {
                        RestartAppBarsFrom(order + 1, () =>
                        {
                            try
                            {
                                appBarWindow.Show();
                            }
                            catch
                            {
                                var newAppBarWindow = new AppBarWindow(model);
                                newAppBarWindow.Show();
                                AppBarWindowPairs[order] = newAppBarWindow;
                            }
                        });
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
                        RestartAppBarsFrom(order + 1, () =>
                        {
                            var newAppBarWindow = new AppBarWindow(model);
                            newAppBarWindow.Show();
                            AppBarWindowPairs.TryAdd(order, newAppBarWindow);
                        });
                    }
                }
            }
        }
    }

    public void SetDockMode(int order, AppBarDockMode dockMode)
    {
        lock (_appBarWindowLock)
        {
            if (_settings.AppBars.TryGetValue(order, out var model))
            {
                model.DockMode = dockMode;
                _settings.Save();
                if (AppBarWindowPairs.TryGetValue(order, out var appBarWindow))
                {
                    appBarWindow.ViewModel.DockMode = dockMode;
                }
            }
        }
    }

    public void SetMonitorName(int order, string? monitorName)
    {
        lock (_appBarWindowLock)
        {
            if (_settings.AppBars.TryGetValue(order, out var model))
            {
                model.MonitorName = monitorName;
                _settings.Save();
                if (AppBarWindowPairs.TryGetValue(order, out var appBarWindow))
                {
                    appBarWindow.ViewModel.MonitorName = monitorName;
                }
            }
        }
    }

    public void SetFollowSystemTaskbarWidthOrHeight(int order, bool followSystemTaskbarWidthOrHeight)
    {
        lock (_appBarWindowLock)
        {
            if (_settings.AppBars.TryGetValue(order, out var model))
            {
                model.FollowSystemTaskbarWidthOrHeight = followSystemTaskbarWidthOrHeight;
                _settings.Save();
                if (AppBarWindowPairs.TryGetValue(order, out var appBarWindow))
                {
                    appBarWindow.ViewModel.FollowSystemTaskbarWidthOrHeight = followSystemTaskbarWidthOrHeight;
                }
            }
        }
    }

    public void SetDockedWidthOrHeight(int order, int dockedWidthOrHeight)
    {
        lock (_appBarWindowLock)
        {
            if (_settings.AppBars.TryGetValue(order, out var model))
            {
                model.DockedWidthOrHeight = dockedWidthOrHeight;
                _settings.Save();
                if (AppBarWindowPairs.TryGetValue(order, out var appBarWindow))
                {
                    appBarWindow.ViewModel.DockedWidthOrHeight = dockedWidthOrHeight;
                }
            }
        }
    }

    public void SetIsResizable(int order, bool isResizable)
    {
        lock (_appBarWindowLock)
        {
            if (_settings.AppBars.TryGetValue(order, out var model))
            {
                model.IsResizable = isResizable;
                _settings.Save();
                if (AppBarWindowPairs.TryGetValue(order, out var appBarWindow))
                {
                    appBarWindow.ViewModel.IsResizable = isResizable;
                }
            }
        }
    }

    #endregion

    #region AppBarWindow Management

    private void StartAppBars(List<AppBarModel> models)
    {
        foreach (var model in models.OrderBy(m => m.Order))
        {
            if (model.IsEnabled)
            {
                var barWindow = new AppBarWindow(model);
                barWindow.Show();
                AppBarWindowPairs.TryAdd(model.Order, barWindow);
            }
        }
    }

    private void RestartAppBarsFrom(int startOrder, Action? action = null)
    {
        var pairs = new List<AppBarModel>();
        foreach (var order in AppBarWindowPairs.Keys)
        {
            if (AppBarWindowPairs.TryGetValue(order, out var appBarWindow))
            {
                var model = appBarWindow.Model;
                if (model.Order >= startOrder)
                {
                    appBarWindow.Close();
                    AppBarWindowPairs.Remove(order);
                    pairs.Add(model);
                }
            }
        }
        action?.Invoke();
        StartAppBars(pairs);
    }

    #endregion

    #endregion

    #region Bar Element Management

    public List<BarElementModel> GetOrderedBarElements(BarElementModelPosition position, AppBarModel model)
    {
        lock (_appBarWindowLock)
        {
            return [.. GetBarElements(position, model).OrderBy(c => c.Order)];
        }
    }

    public void AddBarElement(BarElementModelPosition position, AppBarModel model, string id, Action<BarElementModel> added)
    {
        lock (_appBarWindowLock)
        {
            var barElements = GetBarElements(position, model);
            var elementOrder = barElements.GetMax(x => x.Order);
            var barElement = new BarElementModel()
            {
                Order = elementOrder,
                ID = id,
                Name = PluginManager.AllPlugins.FirstOrDefault(p => p.Metadata.ID == id)?.Metadata.Name ?? string.Empty,
            };
            barElements.Add(barElement);
            added(barElement);
            _settings.Save();
            if (AppBarWindowPairs.TryGetValue(model.Order, out var appBarWindow))
            {
                var viewModelBarElements = GetViewModelBarElements(position, appBarWindow);
                viewModelBarElements.Add(barElement);
            }
        }
    }

    public void RemoveBarElement(BarElementModelPosition position, AppBarModel model, int order)
    {
        lock (_appBarWindowLock)
        {
            var barElements = GetBarElements(position, model);
            if (barElements.RemoveOrder(order))
            {
                _settings.Save();
                if (AppBarWindowPairs.TryGetValue(model.Order, out var appBarWindow))
                {
                    var viewModelBarElements = GetViewModelBarElements(position, appBarWindow);
                    viewModelBarElements.RemoveAll(x => x.Order == order);
                }
            }
        }
    }

    public void ChangeBarElementOrder(BarElementModelPosition position, AppBarModel model, int oldIndex, int newIndex, int itemsCount)
    {
        lock (_appBarWindowLock)
        {
            var barElements = GetBarElements(position, model);
            if (barElements.Move(oldIndex, newIndex, itemsCount))
            {
                _settings.Save();
                if (AppBarWindowPairs.TryGetValue(model.Order, out var appBarWindow))
                {
                    var viewModelBarElements = GetViewModelBarElements(position, appBarWindow);
                    viewModelBarElements.Move(oldIndex, newIndex, itemsCount);
                }
            }
        }
    }

    private static List<BarElementModel> GetBarElements(BarElementModelPosition position, AppBarModel model)
    {
        return position switch
        {
            BarElementModelPosition.LeftOrTop => model.LeftOrTopBarElements,
            BarElementModelPosition.Center => model.CenterBarElements,
            BarElementModelPosition.RightOrBottom => model.RightOrBottomBarElements,
            _ => throw new NotImplementedException()
        };
    }

    private static ObservableCollection<BarElementModel> GetViewModelBarElements(BarElementModelPosition position, AppBarWindow window)
    {
        return position switch
        {
            BarElementModelPosition.LeftOrTop => window.ViewModel.LeftOrTopBarElements,
            BarElementModelPosition.Center => window.ViewModel.CenterBarElements,
            BarElementModelPosition.RightOrBottom => window.ViewModel.RightOrBottomBarElements,
            _ => throw new NotImplementedException()
        };
    }

    #endregion
}
