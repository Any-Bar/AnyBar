using Flow.Bar.Models.Enums;

namespace Flow.Bar.Models.AppBar;

public class AppBarModel
{
    public int ID { get; set; } = -1;

    public AppBarDockMode DockMode { get; set; } = AppBarDockMode.Top;

    public string? MonitorName { get; set; } = null;

    public int? DockedWidthOrHeight { get; set; } = null;

    public bool IsResizable { get; set; } = false;
}
