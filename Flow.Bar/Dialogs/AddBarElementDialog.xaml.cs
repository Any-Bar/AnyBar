using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Bar.Controls;
using Flow.Bar.Helper.Plugins;
using Flow.Bar.Models.Plugins;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Flow.Bar.Dialogs;

[INotifyPropertyChanged]
public partial class AddBarElementDialog : ContentDialog
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private PluginViewModel? _plugin;

    public ObservableCollection<PluginViewModel> AllPlugins { get; } = [];

    private readonly List<PluginViewModel> _allPlugins;

    public AddBarElementDialog()
    {
        InitializeComponent();
        _allPlugins = [.. PluginManager.AllPlugins.Select(plugin => new PluginViewModel(plugin)).OrderBy(x => x.Name)];
        RefreshPlugins();
    }

    private void RefreshPlugins()
    {
        AllPlugins.Clear();
        foreach (var plugin in _allPlugins)
        {
            AllPlugins.Add(plugin);
        }
    }
}
