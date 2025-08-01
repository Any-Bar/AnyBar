using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Bar.Models.Enums;
using Flow.Bar.Models.Monitor;
using System.Collections.Generic;

namespace Flow.Bar.ViewModels.SettingPages;

public partial class SettingsPaneAppBarSettingViewModel : ObservableObject
{
    private static readonly string ClassName = nameof(SettingsPaneAppBarSettingViewModel);

    public List<AppBarDockModeLocalized> AllDockModes { get; } = AppBarDockModeLocalized.GetValues();

    [ObservableProperty]
    private AppBarDockMode _dockMode = AppBarDockMode.Top;

    public List<MonitorNameLocalized> AllMonitorNames { get; } = MonitorNameLocalized.GetValues(App.Settings.AppBars.Values);

    [ObservableProperty]
    private string? _monitorName = null;

    [ObservableProperty]
    private bool _isResizable = false;
}
