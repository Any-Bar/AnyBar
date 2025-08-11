using Flow.Bar.Localization.Attributes;

namespace Flow.Bar.Models.Enums;

[EnumLocalize]
public enum SettingsPaneBarElementSettingHorizontalSortMode
{
    [EnumLocalizeKey(nameof(Localize.SettingsPaneBarElementSettingSortMode_LeftToRight))]
    LeftToRight,

    [EnumLocalizeKey(nameof(Localize.SettingsPaneBarElementSettingSortMode_RightToLeft))]
    RightToLeft,

    [EnumLocalizeKey(nameof(Localize.SettingPanePluginsSortMode_Status))]
    Status,

    [EnumLocalizeKey(nameof(Localize.SettingsPaneAppBarSortMode_Name))]
    Name,
}

[EnumLocalize]
public enum SettingsPaneBarElementSettingVerticalSortMode
{
    [EnumLocalizeKey(nameof(Localize.SettingsPaneBarElementSettingSortMode_TopToBottom))]
    TopToBottom,

    [EnumLocalizeKey(nameof(Localize.SettingsPaneBarElementSettingSortMode_BottomToTop))]
    BottomToTop,

    [EnumLocalizeKey(nameof(Localize.SettingPanePluginsSortMode_Status))]
    Status,

    [EnumLocalizeKey(nameof(Localize.SettingsPaneAppBarSortMode_Name))]
    Name,
}
