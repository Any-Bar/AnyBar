using Flow.Bar.Localization.Attributes;

namespace Flow.Bar.Models.Enums;

[EnumLocalize]
public enum SettingsPaneAppBarSortMode
{
    [EnumLocalizeKey(nameof(Localize.SettingsPaneAppBarSortMode_Order))]
    Order,

    [EnumLocalizeKey(nameof(Localize.SettingPanePluginsSortMode_Status))]
    Status,

    [EnumLocalizeKey(nameof(Localize.SettingsPaneAppBarSortMode_Name))]
    Name,
}
