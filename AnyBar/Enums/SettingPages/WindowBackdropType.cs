using AnyBar.Localization.Attributes;

namespace AnyBar.Enums;

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
