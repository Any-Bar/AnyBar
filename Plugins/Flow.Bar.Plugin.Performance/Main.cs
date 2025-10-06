using System;
using System.Collections.Concurrent;
using System.Windows;
using Flow.Bar.Plugin.Performance.Services;
using Flow.Bar.Plugin.Performance.Views;

namespace Flow.Bar.Plugin.Performance;

public class Main : IPlugin, IPluginI18n, IDisposable
{
    private static readonly string ClassName = nameof(Main);

    internal static PluginInitContext Context { get; private set; }

    private readonly PerformanceMonitorService _performanceMonitorService = new();

    private readonly ConcurrentDictionary<string, PerformanceControl> _runningBarElements = new();

    public void Init(PluginInitContext context)
    {
        Context = context;
    }

    public FrameworkElement CreateBarElement(BarElementContext context)
    {
        Context.API.LogVerbose(ClassName, $"Creating Performance control for position: {context.Position}");

        _performanceMonitorService.StartMonitoring();

        if (_runningBarElements.TryGetValue(context.Id, out var existingControl))
        {
            existingControl.OnDockModeChanged(context.Position);
            return existingControl;
        }

        var control = new PerformanceControl(_performanceMonitorService, context.Position);
        control.OnBarElementCreated();
        _runningBarElements.TryAdd(context.Id, control);

        return control;
    }

    public void DeleteBarElement(string id)
    {
        Context.API.LogVerbose(ClassName, $"Deleting Performance control for id: {id}");

        if (_runningBarElements.TryGetValue(id, out var deletedControl))
        {
            deletedControl.OnBarElementDeleted();
            _runningBarElements.TryRemove(id, out _);
        }

        if (_runningBarElements.IsEmpty)
        {
            _performanceMonitorService.StopMonitoring();
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
        return Localize.FlowBarPlugin_Performance_PluginName();
    }

    public string GetTranslatedPluginDescription()
    {
        return Localize.FlowBarPlugin_Performance_PluginDescription();
    }

    public void Dispose()
    {
        _performanceMonitorService.Dispose();
    }
}
