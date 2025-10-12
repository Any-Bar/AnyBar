using AnyBar.Localization.Attributes;

namespace AnyBar.Enums;

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
