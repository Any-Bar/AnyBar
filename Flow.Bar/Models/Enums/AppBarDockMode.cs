using System.Text.Json.Serialization;
using Flow.Bar.Localization.Attributes;

namespace Flow.Bar.Models.Enums;

[EnumLocalize]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AppBarDockMode
{
    [EnumLocalizeKey(nameof(Localize.AppBarDockMode_Left))]
    Left = 0,

    [EnumLocalizeKey(nameof(Localize.AppBarDockMode_Top))]
    Top,

    [EnumLocalizeKey(nameof(Localize.AppBarDockMode_Right))]
    Right,

    [EnumLocalizeKey(nameof(Localize.AppBarDockMode_Bottom))]
    Bottom
}
