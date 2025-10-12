using AnyBar.Enums;
using AnyBar.Models.AppBar;

namespace AnyBar.Models.Parameters;

public class SettingsPaneBarElementSettingRemoveParameter
{
    public required BarElementModelPosition Position { get; init; }

    public required AppBarModel Model { get; init; }

    public required int Order { get; init; }
}
