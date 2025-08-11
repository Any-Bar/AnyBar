using Flow.Bar.Localization.Attributes;

namespace Flow.Bar.Models.Enums;

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
