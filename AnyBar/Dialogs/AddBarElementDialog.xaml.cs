using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using AnyBar.Controls;
using AnyBar.Helpers.Plugins;
using AnyBar.Models.Plugins;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnyBar.Dialogs;

[INotifyPropertyChanged]
public partial class AddBarElementDialog : ContentDialogEx
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value)
    {
        lock (_pluginsLock)
        {
            var filteredData = _allPlugins.Where(FilterPlugin).ToList();
            RemoveNonMatchingPlugins(filteredData);
            AddBackMatchingPlugins(filteredData);
        }
    }

    [ObservableProperty]
    private PluginViewModel? _plugin;

    public ObservableCollection<PluginViewModel> AllPlugins { get; } = [];

    private readonly List<PluginViewModel> _allPlugins;

    private readonly Lock _pluginsLock = new();

    public AddBarElementDialog()
    {
        InitializeComponent();
        lock (_pluginsLock)
        {
            _allPlugins = [.. PluginManager.GetAllInitializedPlugins(false).
                Select(plugin => new PluginViewModel(plugin)).OrderBy(x => x.Name)];
            foreach (var plugin in _allPlugins)
            {
                AllPlugins.Add(plugin);
            }
        }
    }

    private void RemoveNonMatchingPlugins(List<PluginViewModel> filteredData)
    {
        for (var i = AllPlugins.Count - 1; i >= 0; i--)
        {
            var item = AllPlugins[i];
            if (!filteredData.Contains(item))
            {
                AllPlugins.Remove(item);
            }
        }
    }

    private void AddBackMatchingPlugins(List<PluginViewModel> filteredData)
    {
        var addBackDataList = new List<AddBackData>();
        for (var i = 0; i < filteredData.Count; i++)
        {
            var plugin = filteredData[i];
            if (!AllPlugins.Contains(plugin))
            {
                addBackDataList.Add(new AddBackData
                {
                    Index = i,
                    Model = plugin
                });
            }
        }

        // Add plugin back by their index with correct ordering
        foreach (var data in addBackDataList)
        {
            AllPlugins.Insert(data.Index, data.Model);
        }
    }

    private bool FilterPlugin(PluginViewModel viewModel)
    {
        return string.IsNullOrEmpty(SearchText) ||
            viewModel.Name.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase) ||
            viewModel.Description.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase);
    }

    private class AddBackData
    {
        public required int Index { get; init; }
        public required PluginViewModel Model { get; init; }
    }
}
