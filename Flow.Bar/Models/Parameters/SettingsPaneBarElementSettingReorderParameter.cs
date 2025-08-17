using Flow.Bar.Enums;
using Flow.Bar.Models.AppBar;

namespace Flow.Bar.Models.Parameters;

public class SettingsPaneBarElementSettingReorderParameter
{
    public required BarElementModelPosition Position { get; init; }

    public required AppBarModel Model { get; init; }
}
