using System.Text.Json.Serialization;

namespace Flow.Bar.Models.AppBar;

public class BarElementModel
{
    [JsonIgnore]
    public AppBarModel AppBar { get; set; } = null!;

    [JsonIgnore]
    public Position BarElementPosition { get; set; } = Position.LeftOrTop;

    public int Order { get; set; } = -1;

    public string ID { get; set; } = string.Empty;

    public enum Position
    {
        LeftOrTop,
        Center,
        RightOrBottom
    }
}
