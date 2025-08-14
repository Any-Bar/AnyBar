using System;
using System.Windows;

namespace Flow.Bar.Controls;

public class MenuFlyoutExOptions : IEquatable<MenuFlyoutExOptions>
{
    public MenuFlyoutExPlacementMode Placement { get; set; } = MenuFlyoutExPlacementMode.AppBarBottom;

    public Point? Position { get; set; } = null;

    public Window? Window { get; set; } = null;

    public MenuFlyoutExOptions()
    {

    }

    public static bool operator ==(MenuFlyoutExOptions? x, MenuFlyoutExOptions? y)
    {
        return x?.Placement == y?.Placement &&
               x?.Position == y?.Position &&
               x?.Window == y?.Window;
    }

    public static bool operator !=(MenuFlyoutExOptions? x, MenuFlyoutExOptions? y)
    {
        return !(x == y);
    }

    public bool Equals(MenuFlyoutExOptions? other)
    {
        return this == other;
    }

    public override bool Equals(object? obj)
    {
        if (obj is MenuFlyoutExOptions flyoutShowOptions)
        {
            return this == flyoutShowOptions;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Placement.GetHashCode() ^
               (Position?.GetHashCode() ?? 0) ^
               (Window?.GetHashCode() ?? 0);
    }
}
