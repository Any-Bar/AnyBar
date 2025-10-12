using AnyBar.Localization.Attributes;

namespace AnyBar.Enums;

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
