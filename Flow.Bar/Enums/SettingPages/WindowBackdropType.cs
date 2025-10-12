using Flow.Bar.Localization.Attributes;

namespace Flow.Bar.Enums;

[EnumLocalize]
public enum WindowBackdropType
{
    [EnumLocalizeKey(nameof(Localize.WindowBackdropType_None))]
    None,

    [EnumLocalizeKey(nameof(Localize.WindowBackdropType_Mica))]
    Mica,

    [EnumLocalizeKey(nameof(Localize.WindowBackdropType_Acrylic))]
    Acrylic
}
