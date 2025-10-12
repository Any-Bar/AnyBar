using AnyBar.Localization.Attributes;

namespace AnyBar.Enums;

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
