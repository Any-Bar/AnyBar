using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Controls;
using Flow.Bar.Enums;
using Flow.Bar.Enums;
using Flow.Bar.Helper.MenuFlyout;
using Flow.Bar.Helper.Plugins;
using Flow.Bar.Interfaces;
using Flow.Bar.Models.Plugins;
using Flow.Bar.Plugin;

namespace Flow.Bar.ViewModels;

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
                _allPlugins = [.. PluginManager.GetAllLoadedPlugins().Select(plugin => new PluginViewModel(plugin))];
                UpdateFilteredPlugins();
                UpdateSortedPlugins();
                FilterSortedPlugins();
            }
            _isInitialized = true;
        }

        // Only need to initialize once
        InitializeMenuFlyoutHelper();
    }

    public void OnNavigatedFrom()
    {
        _isInitialized = false;
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

    #region Menu Flyout

    private static readonly double ContextMenuWidth = (double)Application.Current.TryFindResource("CustomContextMenuWidth");
    private static readonly double SecondaryContextMenuWidth = (double)Application.Current.TryFindResource("SecondaryContextMenuWidth");
    private static readonly double SecondaryContextMenuHeight = (double)Application.Current.TryFindResource("SecondaryContextMenuHeight");
    private static readonly Style PluginUninstallationContextMenuStyle = (Style)Application.Current.TryFindResource("PluginUninstallationContextMenuStyle");

    private PluginUninstallationMenuFlyoutHelper<PluginViewModel> _menuFlyoutHelper = null!;

    private void InitializeMenuFlyoutHelper()
    {
        _menuFlyoutHelper = new(
            ContextMenuWidth,
            SecondaryContextMenuWidth,
            SecondaryContextMenuHeight,
            PluginUninstallationContextMenuStyle,
            "UninstallButton",
            UninstallPlugin);
        var uninstallItem = new MenuItem();
        uninstallItem.SetResourceReference(HeaderedItemsControl.HeaderProperty, nameof(Localize.SettingPanePlugins_Uninstall));
        uninstallItem.Click += UninstallItem_Click;
        _menuFlyoutHelper.Items.Add(uninstallItem);
    }

    [RelayCommand]
    private void ShowPluginMoreOptions(FontIconButton button)
    {
        _menuFlyoutHelper.ButtonClick(button);
    }

    private void UninstallItem_Click(object sender, RoutedEventArgs e)
    {
        _menuFlyoutHelper.UninstallItemClick();
    }

    private async void UninstallPlugin(PluginViewModel plugin)
    {
        var oldPlugin = plugin.PluginPair.Metadata;
        if (await PluginInstaller.UninstallPluginAndCheckRestartAsync(oldPlugin))
        {
            lock (_pluginsLock)
            {
                AllPlugins.Remove(AllPlugins.First(x => x.ID == oldPlugin.ID));
                _allPlugins.Remove(_allPlugins.First(x => x.ID == oldPlugin.ID));
                _filteredPlugins.Remove(_filteredPlugins.First(x => x.ID == oldPlugin.ID));
                _sortedPlugins.Remove(_sortedPlugins.First(x => x.ID == oldPlugin.ID));
            }
        }
    }

    #endregion
}
