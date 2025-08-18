using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Enums;
using Flow.Bar.Helpers.Monitor;
using Flow.Bar.Interfaces;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Monitor;
using Flow.Bar.Models.Parameters;
using Flow.Bar.Services;

namespace Flow.Bar.ViewModels;

public partial class SettingsPaneAppBarSettingViewModel(AppBarManagementService appBarManagementService, NavigationViewService navigationViewService) : ObservableObject, INavigationAware, INavigationHeader
{
    private static readonly string ClassName = nameof(SettingsPaneAppBarSettingViewModel);

    private readonly AppBarManagementService _appBarManagementService = appBarManagementService;

    private readonly NavigationViewService _navigationViewService = navigationViewService;

    private bool _isInitialized = false;

    [ObservableProperty]
    private AppBarModel _appBarModel = null!;

    #region Name

    [ObservableProperty]
    private string _name = string.Empty;

    partial void OnNameChanged(string value)
    {
        if (!_isInitialized) return;
        _appBarManagementService.SetName(AppBarModel.Order, value);
    }

    #endregion

    #region Enabled

    [ObservableProperty]
    private bool _isEnabled = true;

    partial void OnIsEnabledChanged(bool value)
    {
        if (!_isInitialized) return;
        _appBarManagementService.SetEnabled(AppBarModel.Order, value);
    }

    #endregion

    #region Dock Mode

    public List<AppBarDockModeLocalized> AllDockModes { get; } = AppBarDockModeLocalized.GetValues();

    [ObservableProperty]
    private AppBarDockMode _dockMode = AppBarDockMode.Top;

    partial void OnDockModeChanged(AppBarDockMode value)
    {
        UpdateMinAndMaxDockedWidthOrHeight();
        if (!_isInitialized) return;
        _appBarManagementService.SetDockMode(AppBarModel.Order, value);
    }

    #endregion

    #region Monitor

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

    #endregion

    #region Follow System Taskbar Width or Height

    [ObservableProperty]
    private bool _followSystemTaskbarWidthOrHeight = true;

    partial void OnFollowSystemTaskbarWidthOrHeightChanged(bool value)
    {
        if (!_isInitialized) return;
        _appBarManagementService.SetFollowSystemTaskbarWidthOrHeight(AppBarModel.Order, value);
    }

    #endregion

    #region Docked Width or Height

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

    #endregion

    #region Resizable

    [ObservableProperty]
    private bool _isResizable = false;

    partial void OnIsResizableChanged(bool value)
    {
        if (!_isInitialized) return;
        _appBarManagementService.SetIsResizable(AppBarModel.Order, value);
    }

    #endregion

    #region Remove AppBar

    [RelayCommand]
    private void RemoveAppBar()
    {
        _appBarManagementService.RemoveAppBar(AppBarModel.Order);
        _navigationViewService.GoBack();
    }

    #endregion

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
            ArgumentNullException.ThrowIfNull(ActualMonitor);
        }
        var dockedWidthOrHeight = DockedWidthOrHeight;
        (MinDockedWidthOrHeight, MaxDockedWidthOrHeight, DockedWidthOrHeight) =
            MonitorInfoHelper.GetMinAndMaxDockedWidthOrHeight(dockedWidthOrHeight, DockMode, ActualMonitor);
    }

    #region Open Bar Element Setting

    [RelayCommand]
    private void OpenBarElementSetting(BarElementModelPosition elementPosition)
    {
        _navigationViewService.NavigateTo(SettingPageTag.BarElementSetting, new SettingsPaneBarElementSettingNavigationParameter
        {
            Model = AppBarModel,
            Position = elementPosition
        });
    }

    #endregion

    #region INavigationAware

    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is AppBarModel model)
        {
            if (!_isInitialized)
            {
                AppBarModel = model;
                Name = model.Name;
                IsEnabled = model.IsEnabled;
                DockMode = model.DockMode;
                MonitorName = model.MonitorName;
                FollowSystemTaskbarWidthOrHeight = model.FollowSystemTaskbarWidthOrHeight;
                DockedWidthOrHeight = model.DockedWidthOrHeight;
                IsResizable = model.IsResizable;
                UpdateMinAndMaxDockedWidthOrHeight();
                _isInitialized = true;
            }
        }
        else
        {
            App.API.LogError(ClassName, $"{nameof(parameter)} is not of type {nameof(AppBarModel)}");
        }
    }

    public void OnNavigatedFrom()
    {
        _isInitialized = false;
    }

    #endregion

    #region INavigationHeader

    public string? GetHeaderKey()
    {
        return AppBarModel == null ? nameof(Localize.SettingWindow_AppBar) : null;
    }

    public string GetHeaderValue()
    {
        return AppBarModel!.Name;
    }

    #endregion
}
