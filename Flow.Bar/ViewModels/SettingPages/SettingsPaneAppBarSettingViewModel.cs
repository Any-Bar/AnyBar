using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Helper.Monitor;
using Flow.Bar.Interfaces;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Models.Monitor;
using Flow.Bar.Services;
using System;
using System.Collections.Generic;

namespace Flow.Bar.ViewModels.SettingPages;

public partial class SettingsPaneAppBarSettingViewModel(AppBarManagementService appBarManagementService, NavigationViewService navigationViewService) : ObservableObject, INavigationAware
{
    private static readonly string ClassName = nameof(SettingsPaneAppBarSettingViewModel);

    private readonly AppBarManagementService _appBarManagementService = appBarManagementService;

    private readonly NavigationViewService _navigationViewService = navigationViewService;

    private bool _isInitialized = false;

    [ObservableProperty]
    private AppBarModel _appBarModel = null!;

    [ObservableProperty]
    private bool _isEnabled = true;

    partial void OnIsEnabledChanged(bool value)
    {
        if (!_isInitialized) return;
        _appBarManagementService.SetEnabled(AppBarModel.Order, value);
    }

    public List<AppBarDockModeLocalized> AllDockModes { get; } = AppBarDockModeLocalized.GetValues();

    [ObservableProperty]
    private AppBarDockMode _dockMode = AppBarDockMode.Top;

    partial void OnDockModeChanged(AppBarDockMode value)
    {
        UpdateMinAndMaxDockedWidthOrHeight();
        if (!_isInitialized) return;
        _appBarManagementService.SetDockMode(AppBarModel.Order, value);
    }

    public List<MonitorNameLocalized> AllMonitorNames { get; } = appBarManagementService.GetAllMonitorNames(true);

    [ObservableProperty]
    private MonitorInfo _actualMonitor = null!;

    partial void OnActualMonitorChanged(MonitorInfo value)
    {
        UpdateMinAndMaxDockedWidthOrHeight();
    }

    [ObservableProperty]
    private string? _monitorName = null;

    partial void OnMonitorNameChanged(string? value)
    {
        UpdateActualMonitor(value);
        if (!_isInitialized) return;
        _appBarManagementService.SetMonitorName(AppBarModel.Order, value);
    }

    [ObservableProperty]
    private bool _followSystemTaskbarWidthOrHeight = true;

    partial void OnFollowSystemTaskbarWidthOrHeightChanged(bool value)
    {
        if (!_isInitialized) return;
        _appBarManagementService.SetFollowSystemTaskbarWidthOrHeight(AppBarModel.Order, value);
    }

    [ObservableProperty]
    private int _minDockedWidthOrHeight = 0;

    [ObservableProperty]
    private int _maxDockedWidthOrHeight = int.MaxValue;

    [ObservableProperty]
    private int _dockedWidthOrHeight = MonitorInfoHelper.DefaultDockedWidthOrHeight;

    partial void OnDockedWidthOrHeightChanged(int value)
    {
        if (!_isInitialized) return;
        _appBarManagementService.SetDockedWidthOrHeight(AppBarModel.Order, value);
    }

    [ObservableProperty]
    private bool _isResizable = false;

    partial void OnIsResizableChanged(bool value)
    {
        if (!_isInitialized) return;
        _appBarManagementService.SetIsResizable(AppBarModel.Order, value);
    }

    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is AppBarModel model)
        {
            AppBarModel = model;
            IsEnabled = model.IsEnabled;
            DockMode = model.DockMode;
            MonitorName = model.MonitorName;
            FollowSystemTaskbarWidthOrHeight = model.FollowSystemTaskbarWidthOrHeight;
            DockedWidthOrHeight = model.DockedWidthOrHeight;
            IsResizable = model.IsResizable;
            UpdateMinAndMaxDockedWidthOrHeight();
            _isInitialized = true;
        }
        else
        {
            App.API.LogError(ClassName, "Parameter is not of type AppBarModel");
        }
    }

    public void OnNavigatedFrom()
    {
        _isInitialized = false;
    }

    [RelayCommand]
    private void RemoveAppBar()
    {
        _appBarManagementService.RemoveAppBar(AppBarModel.Order);
        _navigationViewService.GoBack();
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

    private void UpdateMinAndMaxDockedWidthOrHeight()
    {
        if (ActualMonitor == null)
        {
            UpdateActualMonitor(MonitorName);
        }
        if (ActualMonitor == null)
        {
            // ActualMonitor should never be null here since users should connect to a monitor to use this app :?
            // So we throw an exception to indicate this issue.
            throw new InvalidOperationException("ActualMonitor cannot be null");
        }
        var dockedWidthOrHeight = DockedWidthOrHeight;
        (MinDockedWidthOrHeight, MaxDockedWidthOrHeight, DockedWidthOrHeight) =
            MonitorInfoHelper.GetMinAndMaxDockedWidthOrHeight(dockedWidthOrHeight, DockMode, ActualMonitor);
    }
}
