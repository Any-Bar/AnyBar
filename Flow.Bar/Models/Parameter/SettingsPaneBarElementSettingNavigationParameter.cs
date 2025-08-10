using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;

namespace Flow.Bar.Models.Parameter;

public class SettingsPaneBarElementSettingNavigationParameter
{
    public required BarElementModelPosition Position { get; init; }

    public required AppBarModel Model { get; init; }
}
