using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Bar.Interfaces;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Models.Monitor;
using Flow.Bar.Services;
using System.Collections.Generic;

namespace Flow.Bar.ViewModels.SettingPages;

public partial class SettingsPaneAppBarSettingViewModel(AppBarManagementService appBarManagementService) : ObservableObject, INavigationAware
{
    private static readonly string ClassName = nameof(SettingsPaneAppBarSettingViewModel);

    private readonly AppBarManagementService _appBarManagementService = appBarManagementService;

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
        if (!_isInitialized) return;
        _appBarManagementService.SetDockMode(AppBarModel.Order, value);
    }

    public List<MonitorNameLocalized> AllMonitorNames { get; } = appBarManagementService.GetAllMonitorNames();

    [ObservableProperty]
    private string? _monitorName = null;

    partial void OnMonitorNameChanged(string? value)
    {
        if (!_isInitialized) return;
        _appBarManagementService.SetMonitorName(AppBarModel.Order, value);
    }

    [ObservableProperty]
    public bool _isResizable = false;

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
            IsResizable = model.IsResizable;
            _isInitialized = true;
        }
        else
        {
            App.API.LogError(ClassName, "Parameter is not of type AppBarModel.");
        }
    }

    public void OnNavigatedFrom()
    {

    }
}
