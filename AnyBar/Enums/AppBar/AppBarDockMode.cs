using System.Text.Json.Serialization;
using AnyBar.Localization.Attributes;

namespace AnyBar.Enums;

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
