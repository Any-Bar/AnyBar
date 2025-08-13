using Flow.Bar.Localization.Attributes;

namespace Flow.Bar.Enums;

[EnumLocalize]
public enum SettingsPaneBarElementSettingSortMode
{
    [EnumLocalizeValue("")]
    LeftTopToRightBottom,

    [EnumLocalizeValue("")]
    RightBottomToLeftTop,

    [EnumLocalizeKey(nameof(Localize.SettingPanePluginsSortMode_Status))]
    Status,

    [EnumLocalizeKey(nameof(Localize.SettingsPaneAppBarSortMode_Name))]
    Name,
}
