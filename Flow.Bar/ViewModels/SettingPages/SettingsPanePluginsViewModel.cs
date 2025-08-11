using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Bar.Helper.Plugins;
using Flow.Bar.Interfaces;
using Flow.Bar.Models.Enums;
using Flow.Bar.Models.Plugins;
using Flow.Bar.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Flow.Bar.ViewModels.SettingPages;

public partial class SettingsPanePluginsViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized = false;

    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value)
    {
        lock (_pluginsLock)
        {
            FilterSortedPlugins();
            UpdateFilteredPlugins();
        }
    }

    public List<SettingPanePluginsFilterModeLocalized> AllFilterModes { get; } = SettingPanePluginsFilterModeLocalized.GetValues();

    [ObservableProperty]
    private SettingPanePluginsFilterMode _filterMode = SettingPanePluginsFilterMode.AllPlugins;

    partial void OnFilterModeChanged(SettingPanePluginsFilterMode value)
    {
        lock (_pluginsLock)
        {
            FilterSortedPlugins();
            UpdateFilteredPlugins();
        }
    }

    public List<SettingPanePluginsSortModeLocalized> AllSortModes { get; } = SettingPanePluginsSortModeLocalized.GetValues();

    [ObservableProperty]
    private SettingPanePluginsSortMode _sortMode = SettingPanePluginsSortMode.Status;

    partial void OnSortModeChanged(SettingPanePluginsSortMode value)
    {
        lock (_pluginsLock)
        {
            SortFilteredPlugins();
            UpdateSortedPlugins();
        }
    }

    public ObservableCollection<PluginViewModel> AllPlugins { get; } = [];

    private List<PluginViewModel> _allPlugins = null!;
    private List<PluginViewModel> _filteredPlugins = null!;
    private List<PluginViewModel> _sortedPlugins = null!;

    private readonly Lock _pluginsLock = new();

    public void OnNavigatedTo(object? parameter)
    {
        if (!_isInitialized)
        {
            lock (_pluginsLock)
            {
                _allPlugins = [.. PluginManager.AllPlugins.Select(plugin => new PluginViewModel(plugin))];
                UpdateFilteredPlugins();
                UpdateSortedPlugins();
                FilterSortedPlugins();
            }
            _isInitialized = true;
        }
    }

    public void OnNavigatedFrom()
    {
        _isInitialized = false;
    }

    public void UninstallPlugin(PluginMetadata oldPlugin)
    {
        lock (_pluginsLock)
        {
            AllPlugins.Remove(AllPlugins.First(x => x.ID == oldPlugin.ID));
            _allPlugins.Remove(_allPlugins.First(x => x.ID == oldPlugin.ID));
            _filteredPlugins.Remove(_filteredPlugins.First(x => x.ID == oldPlugin.ID));
            _sortedPlugins.Remove(_sortedPlugins.First(x => x.ID == oldPlugin.ID));
        }
    }

    private void UpdateFilteredPlugins()
    {
        _filteredPlugins = [.. _allPlugins.Where(FilterPlugin)];
    }

    private void UpdateSortedPlugins()
    {
        _sortedPlugins = GetSortedPlugins(_allPlugins);
    }

    private void FilterSortedPlugins()
    {
        AllPlugins.Clear();
        foreach (var plugin in _sortedPlugins)
        {
            if (FilterPlugin(plugin))
            {
                AllPlugins.Add(plugin);
            }
        }
    }

    private void SortFilteredPlugins()
    {
        AllPlugins.Clear();
        foreach (var plugin in GetSortedPlugins(_filteredPlugins))
        {
            AllPlugins.Add(plugin);
        }
    }

    private List<PluginViewModel> GetSortedPlugins(List<PluginViewModel> allPlugins)
    {
        return SortMode switch
        {
            SettingPanePluginsSortMode.Status => [.. allPlugins.OrderBy(x => x.Disabled).ThenBy(x => x.Name)],
            SettingPanePluginsSortMode.NameAToZ => [.. allPlugins.OrderBy(x => x.Name)],
            SettingPanePluginsSortMode.NameZToA => [.. allPlugins.OrderByDescending(x => x.Name)],
            _ => allPlugins
        };
    }

    private bool FilterPlugin(PluginViewModel viewModel)
    {
        return (string.IsNullOrEmpty(SearchText) ||
            viewModel.Name.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase) ||
            viewModel.Description.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase)) &&
            (FilterMode == SettingPanePluginsFilterMode.AllPlugins ||
            (PluginManager.IsPreinstalled(viewModel.ID) ? 
                FilterMode == SettingPanePluginsFilterMode.PreinstalledPlugins :
                FilterMode == SettingPanePluginsFilterMode.UserinstalledPlugins));
    }
}
