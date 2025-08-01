using Flow.Bar.Models.Enums;
using System;
using System.Collections.Generic;

namespace Flow.Bar.Models.AppBar;

public class AppBarModel : IEquatable<AppBarModel>
{
    public int Order { get; set; } = -1;

    public bool IsEnabled { get; set; } = true;

    public AppBarDockMode DockMode { get; set; } = AppBarDockMode.Top;

    public string? MonitorName { get; set; } = null;

    public int? DockedWidthOrHeight { get; set; } = null;

    public bool IsResizable { get; set; } = false;

    public List<BarElementModel> LeftOrTopBarElements { get; set; } = [];

    public List<BarElementModel> RightOrBottomBarElements { get; set; } = [];

    public List<BarElementModel> CenterBarElements { get; set; } = [];

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as AppBarModel);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Order.GetHashCode();

    /// <inheritdoc />
    public bool Equals(AppBarModel? other) => Order == other?.Order;

    /// <inheritdoc />
    public static bool operator ==(AppBarModel? a, AppBarModel? b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    /// <inheritdoc />
    public static bool operator !=(AppBarModel? a, AppBarModel? b) => !(a == b);
}
