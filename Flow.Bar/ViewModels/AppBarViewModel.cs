using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Bar.Models;
using System.Linq;

namespace Flow.Bar.ViewModels;

public partial class AppBarViewModel : ObservableObject
{
    [ObservableProperty]
    private AppBarDockMode _dockMode = AppBarDockMode.Top;

    [ObservableProperty]
    private MonitorInfo? _monitor;

    [ObservableProperty]
    private int _dockedWidthOrHeight = 200;

    [ObservableProperty]
    private bool _isResizable = false;

    public MonitorInfo GetSelectedMonitor()
    {
        var monitor = Monitor;
        var allMonitors = MonitorInfo.GetDisplayMonitors();
        if (monitor == null || !allMonitors.Contains(monitor))
        {
            monitor = allMonitors.First(f => f.IsPrimary);
        }
        return monitor;
    }
}

public enum AppBarDockMode
{
    Left = 0,
    Top,
    Right,
    Bottom
}
