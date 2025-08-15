using System;
using System.Text.Json.Serialization;
using Flow.Bar.Enums;
using Flow.Bar.Interfaces;

namespace Flow.Bar.Models.AppBar;

public class BarElementModel : IEquatable<BarElementModel>, IOrder
{
    [JsonIgnore]
    public AppBarModel AppBar { get; set; } = null!;

    [JsonIgnore]
    public BarElementModelPosition BarElementPosition { get; set; } = BarElementModelPosition.LeftOrTop;

    public int Order { get; set; } = -1;

    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// This name is used to display plugin name when the plugin is missing
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as BarElementModel);
    }

    /// <inheritdoc />
    public override int GetHashCode() => AppBar.Order.GetHashCode() ^ Order.GetHashCode();

    /// <inheritdoc />
    public bool Equals(BarElementModel? other) => AppBar.Order == other?.AppBar.Order && Order == other?.Order;

    /// <inheritdoc />
    public static bool operator ==(BarElementModel? a, BarElementModel? b)
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
    public static bool operator !=(BarElementModel? a, BarElementModel? b) => !(a == b);
}
