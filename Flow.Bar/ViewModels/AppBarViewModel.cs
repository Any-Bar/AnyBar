using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Bar.Models;
using Flow.Bar.Models.Enums;
using System.Linq;

namespace Flow.Bar.ViewModels;

public partial class AppBarViewModel : ObservableObject
{
    public int Order { get; set; } = -1;

    [ObservableProperty]
    private AppBarDockMode _dockMode = AppBarDockMode.Top;

    [ObservableProperty]
    private MonitorInfo? _monitor = null;

    [ObservableProperty]
    private int? _dockedWidthOrHeight = null;

    [ObservableProperty]
    private bool _isResizable = false;

    public bool IsHorizontal => DockMode is AppBarDockMode.Top or AppBarDockMode.Bottom;

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
