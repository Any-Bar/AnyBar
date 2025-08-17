using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flow.Bar.Enums;
using Flow.Bar.Extensions;
using Flow.Bar.Helpers.Plugins;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Explorer;
using Flow.Bar.Models.Monitor;
using Flow.Bar.Models.UserSettings;
using Flow.Bar.Views;

namespace Flow.Bar.Services;

#pragma warning disable CS0618 // Type or member is obsolete

public class AppBarManagementService(Settings settings)
{
    private readonly Settings _settings = settings;

    private readonly Dictionary<int, AppBarWindow> AppBarWindowPairs = [];
    private readonly Lock _appBarWindowLock = new();

    private readonly ExplorerWatcher _explorerWatcher = new();
    private bool _isExplorerRestarting = false;

    #region Initialization & Disposal

    public void InitializeAllAppBarWindows()
    {
        lock (_appBarWindowLock)
        {
            StartAppBars([.. _settings.AppBars.Values]);
        }
        InitializaExplorerWatcher();
    }

    private void InitializaExplorerWatcher()
    {
        _explorerWatcher.ExplorerRestarted += async () =>
        {
            await Task.Delay(300);

            if (_isExplorerRestarting) return;
            _isExplorerRestarting = true;

            lock (_appBarWindowLock)
            {
                foreach (var appbarWindow in AppBarWindowPairs.Values.OrderBy(x => x.Model.Order))
                {
                    appbarWindow.ResetAppBarData();
                }
            }

            _isExplorerRestarting = false;
        };
        _explorerWatcher.Start();
    }

    public void Dispose()
    {
        _explorerWatcher.Dispose();
        lock (_appBarWindowLock)
        {
            foreach (var appBarWindow in AppBarWindowPairs.Values)
            {
                try
                {
                    appBarWindow.Close();
                }
                catch (Exception e)
                {
                    // Log the exception but do not throw it, as we are disposing
                    App.API.LogError(nameof(AppBarManagementService), "Error while closing AppBarWindow", e);
                }
            }
            AppBarWindowPairs.Clear();
        }
    }

    #endregion

    #region Monitor Names

    public List<MonitorNameLocalized> GetAllMonitorNames(bool includeSettingMonitors)
    {
        lock (_appBarWindowLock)
        {
            return MonitorNameLocalized.GetValues(includeSettingMonitors ? _settings.AppBars.Values : null);
        }
    }

    #endregion

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
                RestartAppBarsFrom(Math.Min(oldIndex, newIndex));
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

    private void RestartAppBarsFrom(int startOrder, Action? openWindow = null)
    {
        var pairsToRestart = AppBarWindowPairs.Where(x => x.Value.Model.Order >= startOrder);
        // Just one app bar and no window need to open - just update the order and do not need to close and reopen the window
        if (pairsToRestart.Count() == 1 && openWindow == null)
        {
            var pair = pairsToRestart.First();
            AppBarWindowPairs.Remove(pair.Key);
            AppBarWindowPairs.TryAdd(pair.Value.Model.Order, pair.Value);
            return;
        }
        var pairs = new List<AppBarModel>();
        foreach (var (order, appbarWindow) in pairsToRestart)
        {
            var model = appbarWindow.Model;
            appbarWindow.Close();
            AppBarWindowPairs.Remove(order);
            pairs.Add(model);
        }
        openWindow?.Invoke();
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

    public void AddBarElement(BarElementModelPosition position, AppBarModel model, string id, Action<BarElementModel> added, bool changeViewModel)
    {
        lock (_appBarWindowLock)
        {
            var barElements = GetBarElements(position, model);
            var elementOrder = barElements.GetMax(x => x.Order);
            var barElement = new BarElementModel()
            {
                Order = elementOrder,
                ID = id,
                Name = PluginManager.GetPluginForId(id)?.Metadata.Name ?? string.Empty,
            };
            barElements.Add(barElement);
            added(barElement);
            _settings.Save();
            if (changeViewModel)
            {
                if (AppBarWindowPairs.TryGetValue(model.Order, out var appBarWindow))
                {
                    appBarWindow.ViewModel.IgnoreCollectionChangedEvents = true;
                    try
                    {
                        var viewModelBarElements = GetViewModelBarElements(position, appBarWindow);
                        viewModelBarElements.Add(barElement);
                    }
                    finally
                    {
                        appBarWindow.ViewModel.IgnoreCollectionChangedEvents = false;
                    }
                }
            }
        }
    }

    public void RemoveBarElement(BarElementModelPosition position, AppBarModel model, int order, bool changeViewModel)
    {
        lock (_appBarWindowLock)
        {
            var barElements = GetBarElements(position, model);
            if (barElements.RemoveOrder(order, out var barElement))
            {
                _settings.Save();
                if (PluginManager.GetPluginForId(barElement.ID) is { } pair)
                {
                    pair.Plugin.DeleteBarElement(barElement.Context!.Id);
                }
                if (changeViewModel)
                {
                    if (AppBarWindowPairs.TryGetValue(model.Order, out var appBarWindow))
                    {
                        appBarWindow.ViewModel.IgnoreCollectionChangedEvents = true;
                        try
                        {
                            var viewModelBarElements = GetViewModelBarElements(position, appBarWindow);
                            viewModelBarElements.RemoveAll(x => x.Order == order);
                        }
                        finally
                        {
                            appBarWindow.ViewModel.IgnoreCollectionChangedEvents = false;
                        }
                    }
                }
            }
        }
    }

    public void ChangeBarElementOrder(BarElementModelPosition position, AppBarModel model, int oldIndex, int newIndex, int itemsCount, bool changeViewModel)
    {
        lock (_appBarWindowLock)
        {
            var barElements = GetBarElements(position, model);
            if (barElements.Move(oldIndex, newIndex, itemsCount))
            {
                _settings.Save();
                if (changeViewModel)
                {
                    if (AppBarWindowPairs.TryGetValue(model.Order, out var appBarWindow))
                    {
                        appBarWindow.ViewModel.IgnoreCollectionChangedEvents = true;
                        try
                        {
                            var viewModelBarElements = GetViewModelBarElements(position, appBarWindow);
                            viewModelBarElements.Move(oldIndex, newIndex, itemsCount);
                        }
                        finally
                        {
                            appBarWindow.ViewModel.IgnoreCollectionChangedEvents = false;
                        }
                    }
                }
            }
        }
    }

    public void InsertBarElement(BarElementModelPosition position, AppBarModel model, int order, BarElementModel barElement, bool changeViewModel)
    {
        lock (_appBarWindowLock)
        {
            var barElements = GetBarElements(position, model);
            if (barElements.InsertOrder(order, barElement))
            {
                _settings.Save();
                if (changeViewModel)
                {
                    if (AppBarWindowPairs.TryGetValue(model.Order, out var appBarWindow))
                    {
                        appBarWindow.ViewModel.IgnoreCollectionChangedEvents = true;
                        try
                        {
                            var viewModelBarElements = GetViewModelBarElements(position, appBarWindow);
                            viewModelBarElements.Insert(order, barElement);
                        }
                        finally
                        {
                            appBarWindow.ViewModel.IgnoreCollectionChangedEvents = false;
                        }
                    }
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
