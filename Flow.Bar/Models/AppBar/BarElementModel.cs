using System.Text.Json.Serialization;

namespace Flow.Bar.Models.AppBar;

public class BarElementModel
{
    [JsonIgnore]
    public AppBarModel AppBar { get; set; } = null!;

    public int Order { get; set; } = -1;

    public string ID { get; set; } = string.Empty;
}
