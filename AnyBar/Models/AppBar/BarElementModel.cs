using System.Text.Json.Serialization;
using AnyBar.Enums;
using AnyBar.Interfaces;
using AnyBar.Plugin;

namespace AnyBar.Models.AppBar;

public class BarElementModel : IOrder
{
    [JsonIgnore]
    public AppBarModel AppBar { get; set; } = null!;

    [JsonIgnore]
    public BarElementModelPosition BarElementPosition { get; set; } = BarElementModelPosition.LeftOrTop;

    [JsonIgnore]
    public BarElementContext? Context { get; set; } = null;

    public int Order { get; set; } = -1;

    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// This name is used to display plugin name when the plugin is missing
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
