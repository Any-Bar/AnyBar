using System.Text.Json.Serialization;

namespace Flow.Bar.Models.AppBar;

public class BarElementModel
{
    [JsonIgnore]
    public required AppBarModel AppBar { get; set; }

    public int Order { get; set; } = -1;

    public string ID { get; set; } = string.Empty;
}
