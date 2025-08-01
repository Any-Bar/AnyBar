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

    public List<AppBarDockModeLocalized> AllDockModes { get; } = AppBarDockModeLocalized.GetValues();

    public List<MonitorNameLocalized> AllMonitorNames { get; } = appBarManagementService.GetAllMonitorNames();

    [ObservableProperty]
    private AppBarModel? _appBarModel;

    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is AppBarModel model)
        {
            AppBarModel = model;
        }
    }

    public void OnNavigatedFrom()
    {

    }
}
