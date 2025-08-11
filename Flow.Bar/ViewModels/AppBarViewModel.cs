using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Bar.Helper.Monitor;
using Flow.Bar.Helper.Plugins;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Models.Monitor;
using Flow.Bar.Models.Parameter;
using Flow.Bar.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Flow.Bar.ViewModels;

public partial class AppBarViewModel(AppBarManagementService appBarManagementService, NavigationViewService navigationViewService) : ObservableObject, IDisposable
{
    private static readonly string ClassName = nameof(AppBarViewModel);

    private readonly AppBarManagementService _appBarManagementService = appBarManagementService;

    private readonly NavigationViewService _navigationViewService = navigationViewService;

    public AppBarModel Model { get; set; } = null!;

    public ObservableCollection<BarElementModel> LeftOrTopBarElements { get; } = [];

    public ObservableCollection<BarElementModel> CenterBarElements { get; } = [];

    public ObservableCollection<BarElementModel> RightOrBottomBarElements { get; } = [];

    [ObservableProperty]
    private AppBarDockMode _dockMode = AppBarDockMode.Top;

    [ObservableProperty]
    private MonitorInfo _actualMonitor = null!;

    [ObservableProperty]
    private string? _monitorName = null;

    partial void OnMonitorNameChanged(string? value)
    {
        UpdateActualMonitor(value);
    }

    [ObservableProperty]
    private int _actualDockedWidthOrHeight = -1;

    [ObservableProperty]
    private bool _followSystemTaskbarWidthOrHeight = true;

    [ObservableProperty]
    private int _dockedWidthOrHeight = MonitorInfoHelper.DefaultDockedWidthOrHeight;

    public int GetDockedWidthOrHeight()
    {
        if (!FollowSystemTaskbarWidthOrHeight)
        {
            return DockedWidthOrHeight;
        }

        var monitor = MonitorInfo.GetPrimaryDisplayMonitor();
        var taskBarWidthOrHeight = MonitorInfoHelper.GetMonitorTaskBarWidthOrHeight(monitor);
        return taskBarWidthOrHeight;
    }

    [ObservableProperty]
    private bool _isResizable = false;

    public void Initialize(AppBarModel model)
    {
        Model = model;
        DockMode = model.DockMode;
        MonitorName = model.MonitorName;
        FollowSystemTaskbarWidthOrHeight = model.FollowSystemTaskbarWidthOrHeight;
        DockedWidthOrHeight = model.DockedWidthOrHeight;
        IsResizable = model.IsResizable;
        if (ActualMonitor == null)
        {
            UpdateActualMonitor(MonitorName);
        }
        // Initialize the MonitorTaskBarWidthOrHeight for the monitor before any AppBarWindow is created on this monitor
        MonitorInfoHelper.GetMonitorTaskBarWidthOrHeight(ActualMonitor);
    }

    public void InitializeBarElements()
    {
        LeftOrTopBarElements.Clear();
        foreach (var element in _appBarManagementService.GetOrderedBarElements(BarElementModelPosition.LeftOrTop, Model))
        {
            if (PluginManager.CheckBarElement(element))
            {
                LeftOrTopBarElements.Add(element);
            }
        }
        LeftOrTopBarElements.CollectionChanged += LeftOrTopBarElements_CollectionChanged;
        RightOrBottomBarElements.Clear();
        foreach (var element in _appBarManagementService.GetOrderedBarElements(BarElementModelPosition.RightOrBottom, Model))
        {
            if (PluginManager.CheckBarElement(element))
            {
                RightOrBottomBarElements.Add(element);
            }
        }
        RightOrBottomBarElements.CollectionChanged += RightOrBottomBarElements_CollectionChanged;
        CenterBarElements.Clear();
        foreach (var element in _appBarManagementService.GetOrderedBarElements(BarElementModelPosition.Center, Model))
        {
            if (PluginManager.CheckBarElement(element))
            {
                CenterBarElements.Add(element);
            }
        }
        CenterBarElements.CollectionChanged += CenterBarElements_CollectionChanged;
    }

    private void LeftOrTopBarElements_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        BarElements_CollectionChanged(BarElementModelPosition.LeftOrTop, e);
    }

    private void RightOrBottomBarElements_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        BarElements_CollectionChanged(BarElementModelPosition.RightOrBottom, e);
    }

    private void CenterBarElements_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        BarElements_CollectionChanged(BarElementModelPosition.Center, e);
    }

    private void BarElements_CollectionChanged(BarElementModelPosition position, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Move)
        {
            if (e.OldItems == null ||
                e.NewItems == null ||
                e.OldItems.Count != e.NewItems.Count)
            {
                switch (position)
                {
                    case BarElementModelPosition.LeftOrTop:
                        App.API.LogError(ClassName, $"{nameof(NotifyCollectionChangedAction.Move)} action in {nameof(LeftOrTopBarElements)} collection changed with different item counts");
                        break;
                    case BarElementModelPosition.RightOrBottom:
                        App.API.LogError(ClassName, $"{nameof(NotifyCollectionChangedAction.Move)} action in {nameof(RightOrBottomBarElements)} collection changed with different item counts");
                        break;
                    case BarElementModelPosition.Center:
                        App.API.LogError(ClassName, $"{nameof(NotifyCollectionChangedAction.Move)} action in {nameof(CenterBarElements)} collection changed with different item counts");
                        break;
                }
                return;
            }

            var collection = position switch
            {
                BarElementModelPosition.LeftOrTop => LeftOrTopBarElements,
                BarElementModelPosition.RightOrBottom => RightOrBottomBarElements,
                BarElementModelPosition.Center => CenterBarElements,
                _ => throw new NotImplementedException()
            };
            var oldStartingOrder = collection[e.OldStartingIndex].Order;
            var newStartingOrder = collection[e.NewStartingIndex].Order;
            var oldItemMinimalOrder = ((BarElementModel)e.OldItems[0]!).Order;
            var oldItemMaximalOrder = ((BarElementModel)e.OldItems[^1]!).Order;
            var itemsCount = oldItemMaximalOrder - oldItemMinimalOrder + 1;
            _appBarManagementService.ChangeBarElementOrder(position, Model, oldStartingOrder, newStartingOrder, itemsCount, false);
            _navigationViewService.OnNavigateTo(SettingPageTag.BarElementSetting, new SettingsPaneBarElementSettingReorderParameter()
            {
                Position = position,
                Model = Model
            });
        }
    }

    private void UpdateActualMonitor(string? monitorName)
    {
        var monitor = MonitorInfoHelper.GetMonitorInfoFromName(monitorName);
        if (monitor != null)
        {
            ActualMonitor = monitor;
        }
        else
        {
            App.API.LogError(ClassName, $"Monitor not found: {monitorName}");
        }
    }

    public void Dispose()
    {
        LeftOrTopBarElements.CollectionChanged -= LeftOrTopBarElements_CollectionChanged;
        RightOrBottomBarElements.CollectionChanged -= RightOrBottomBarElements_CollectionChanged;
        CenterBarElements.CollectionChanged -= CenterBarElements_CollectionChanged;
    }
}
