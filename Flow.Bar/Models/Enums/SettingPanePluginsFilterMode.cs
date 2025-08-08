using Flow.Bar.Localization.Attributes;

namespace Flow.Bar.Models.Enums;

[EnumLocalize]
public enum SettingPanePluginsFilterMode
{
    [EnumLocalizeKey(nameof(Localize.SettingPanePluginsFilterMode_AllPlugins))]
    AllPlugins,

    [EnumLocalizeKey(nameof(Localize.SettingPanePluginsFilterMode_PreinstalledPlugins))]
    PreinstalledPlugins,

    [EnumLocalizeKey(nameof(Localize.SettingPanePluginsFilterMode_UserinstalledPlugins))]
    UserinstalledPlugins
}
