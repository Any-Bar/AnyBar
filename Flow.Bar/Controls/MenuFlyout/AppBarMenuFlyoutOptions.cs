using Flow.Bar.Models.Monitor;
using System;
using System.Windows;

namespace Flow.Bar.Controls;

public class AppBarMenuFlyoutOptions : IEquatable<AppBarMenuFlyoutOptions>
{
    public AppBarPlacementMode Placement { get; set; } = AppBarPlacementMode.AppBarBottom;

    public Point? Position { get; set; } = null;

    public MonitorInfo Monitor { get; set; } = null!;

    public AppBarMenuFlyoutOptions()
    {

    }

    public static bool operator ==(AppBarMenuFlyoutOptions? x, AppBarMenuFlyoutOptions? y)
    {
        return x?.Placement == y?.Placement &&
               x?.Position == y?.Position &&
               x?.Monitor == y?.Monitor;
    }

    public static bool operator !=(AppBarMenuFlyoutOptions? x, AppBarMenuFlyoutOptions? y)
    {
        return !(x == y);
    }

    public bool Equals(AppBarMenuFlyoutOptions? other)
    {
        return this == other;
    }

    public override bool Equals(object? obj)
    {
        if (obj is AppBarMenuFlyoutOptions flyoutShowOptions)
        {
            return this == flyoutShowOptions;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Placement.GetHashCode() ^
               (Position?.GetHashCode() ?? 0) ^
               Monitor.GetHashCode();
    }
}
