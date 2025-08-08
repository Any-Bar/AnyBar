using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Bar.Models.Enums;
using System.Collections.Generic;

namespace Flow.Bar.ViewModels.SettingPages;

public partial class SettingsPanePluginsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    public List<SettingPanePluginsFilterModeLocalized> AllFilterModes { get; } = SettingPanePluginsFilterModeLocalized.GetValues();

    [ObservableProperty]
    private SettingPanePluginsFilterMode _filterMode = SettingPanePluginsFilterMode.AllPlugins;

    public List<SettingPanePluginsSortModeLocalized> AllSortModes { get; } = SettingPanePluginsSortModeLocalized.GetValues();

    [ObservableProperty]
    private SettingPanePluginsSortMode _sortMode = SettingPanePluginsSortMode.NameAToZ;
}
