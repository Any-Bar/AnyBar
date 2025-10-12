using System;
using System.Collections.Concurrent;
using System.Windows;
using AnyBar.Plugin.Network.Services;
using AnyBar.Plugin.Network.Views;

namespace AnyBar.Plugin.Network;

public class Main : IPlugin, IPluginI18n, IDisposable
{
    private static readonly string ClassName = nameof(Main);

    internal static PluginInitContext Context { get; private set; }

    private readonly NetworkMonitorService _networkMonitorService = new();

    private readonly ConcurrentDictionary<string, NetworkControl> _runningBarElements = new();

    public void Init(PluginInitContext context)
    {
        Context = context;
    }

    public FrameworkElement CreateBarElement(BarElementContext context)
    {
        Context.API.LogVerbose(ClassName, $"Creating Network control for position: {context.Position}");

        _networkMonitorService.StartMonitoring();

        if (_runningBarElements.TryGetValue(context.Id, out var existingControl))
        {
            existingControl.OnDockModeChanged(context.Position);
            return existingControl;
        }

        var control = new NetworkControl(_networkMonitorService, context.Position);
        control.OnBarElementCreated();
        _runningBarElements.TryAdd(context.Id, control);

        return control;
    }

    public void DeleteBarElement(string id)
    {
        Context.API.LogVerbose(ClassName, $"Deleting Network control for id: {id}");

        if (_runningBarElements.TryGetValue(id, out var deletedControl))
        {
            deletedControl.OnBarElementDeleted();
            _runningBarElements.TryRemove(id, out _);
        }

        if (_runningBarElements.IsEmpty)
        {
            _networkMonitorService.StopMonitoring();
        }
    }

    public void OnBarElementContextChanged(BarElementContextChangedAgrs args)
    {
        Context.API.LogVerbose(ClassName, $"Bar element context changed for id: {args.Context.Id}");

        if (_runningBarElements.TryGetValue(args.Context.Id, out var existingControl))
        {
            existingControl.OnDockModeChanged(args.Context.Position);
        }
    }

    public string GetTranslatedPluginTitle()
    {
        return Localize.AnyBarPlugin_Network_PluginName();
    }

    public string GetTranslatedPluginDescription()
    {
        return Localize.AnyBarPlugin_Network_PluginDescription();
    }

    public void Dispose()
    {
        _networkMonitorService.Dispose();
    }
}
