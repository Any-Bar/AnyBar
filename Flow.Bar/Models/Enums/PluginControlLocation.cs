using System.Text.Json.Serialization;

namespace Flow.Bar.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PluginControlLocation
{
    LeftOrTop,
    Center,
    RightOrBottom
}
