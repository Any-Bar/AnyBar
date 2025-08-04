using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Bar.Helper.Monitor;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Models.Monitor;

namespace Flow.Bar.ViewModels;

public partial class AppBarViewModel : ObservableObject
{
    private static readonly string ClassName = nameof(AppBarViewModel);

    public int Order { get; set; } = -1;

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

    public bool IsHorizontal => DockMode is AppBarDockMode.Top or AppBarDockMode.Bottom;

    public void Initialize(AppBarModel model)
    {
        Order = model.Order;
        Order = model.Order;
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

    private void UpdateActualMonitor(string? monitorName)
    {
        var monitor = MonitorInfoHelper.GetMonitorInfoFromName(monitorName);
        if (monitor != null)
        {
            ActualMonitor = monitor;
        }
        else
        {
            App.API.LogError(ClassName, "Monitor not found: " + monitorName);
        }
    }
}
