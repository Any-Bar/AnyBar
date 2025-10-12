using AnyBar.Localization.Attributes;

namespace AnyBar.Enums;

[EnumLocalize]
public enum SettingPanePluginsSortMode
{
    [EnumLocalizeKey(nameof(Localize.SettingPanePluginsSortMode_Status))]
    Status,

    [EnumLocalizeKey(nameof(Localize.SettingPanePluginsSortMode_NameAToZ))]
    NameAToZ,

    [EnumLocalizeKey(nameof(Localize.SettingPanePluginsSortMode_NameZToA))]
    NameZToA,
}
