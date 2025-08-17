using Flow.Bar.Enums;
using Flow.Bar.Models.AppBar;

namespace Flow.Bar.Models.Parameters;

public class SettingsPaneBarElementSettingRemoveParameter
{
    public required BarElementModelPosition Position { get; init; }

    public required AppBarModel Model { get; init; }

    public required int Order { get; init; }
}
