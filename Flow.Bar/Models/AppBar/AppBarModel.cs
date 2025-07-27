using Flow.Bar.Models.Enums;
using System.Collections.Generic;

namespace Flow.Bar.Models.AppBar;

public class AppBarModel
{
    public int Order { get; set; } = -1;

    public AppBarDockMode DockMode { get; set; } = AppBarDockMode.Top;

    public string? MonitorName { get; set; } = null;

    public int? DockedWidthOrHeight { get; set; } = null;

    public bool IsResizable { get; set; } = false;

    public List<PluginControlModel> LeftOrTopPluginControls { get; set; } = [];

    public List<PluginControlModel> RightOrBottomPluginControls { get; set; } = [];

    public List<PluginControlModel> CenterPluginControls { get; set; } = [];
}
