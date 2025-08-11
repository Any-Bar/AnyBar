using Flow.Bar.Interfaces.Enumerable;
using Flow.Bar.Models.Enums;
using System.Text.Json.Serialization;

namespace Flow.Bar.Models.AppBar;

public class BarElementModel : IOrder
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
}
