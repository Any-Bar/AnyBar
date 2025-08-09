using Flow.Bar.Extensions.Data;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Plugins;
using Flow.Bar.Plugin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Flow.Bar.Helper.Plugins;

/// <summary>
/// Class for co-ordinating and managing all plugin lifecycle.
/// </summary>
public static class PluginManager
{
    private static readonly string ClassName = nameof(PluginManager);

    public static List<PluginPair> AllPlugins { get; private set; } = [];

    private static readonly ConcurrentBag<string> ModifiedPlugins = [];

    private static PluginPair[] _translationPlugins = null!;

    /// <summary>
    /// Directories that will hold Flow Bar plugin directory
    /// </summary>
    public static readonly string[] Directories =
    {
        Constants.PreinstalledDirectory, DataLocation.PluginsDirectory
    };

    static PluginManager()
    {
        // validate user directory
        if (!Directory.Exists(DataLocation.PluginsDirectory))
        {
            Directory.CreateDirectory(DataLocation.PluginsDirectory);
        }
    }

    #region Loading & Initialization

    public static void LoadPlugins()
    {
        var metadatas = PluginConfig.Parse(Directories);
        AllPlugins = PluginsLoader.Plugins(metadatas);
        // Since dotnet plugins need to get assembly name first, we should update plugin directory after loading plugins
        UpdatePluginDirectory(metadatas);
        // Initialize plugin enumerable after all plugins are initialized
        _translationPlugins = GetPluginsForInterface<IPluginI18n>();
    }

    private static void UpdatePluginDirectory(List<PluginMetadata> metadatas)
    {
        foreach (var metadata in metadatas)
        {
            if (string.IsNullOrEmpty(metadata.AssemblyName))
            {
                App.API.LogWarning(ClassName, $"AssemblyName is empty for plugin with metadata: {metadata.Name}");
                continue; // Skip if AssemblyName is not set, which can happen for erroneous plugins
            }
            metadata.PluginSettingsDirectoryPath = Path.Combine(DataLocation.PluginSettingsDirectory, metadata.AssemblyName);
            metadata.PluginCacheDirectoryPath = Path.Combine(DataLocation.PluginCacheDirectory, metadata.AssemblyName);
        }
    }

    private static PluginPair[] GetPluginsForInterface<T>()
    {
        // Handle scenario where this is called before all plugins are instantiated, e.g. language change on startup
        return AllPlugins?.Where(p => p.Plugin is T).ToArray() ?? [];
    }

    public static async Task InitializePluginsAsync()
    {
        var failedPlugins = new ConcurrentQueue<PluginPair>();

        var InitTasks = AllPlugins.Select(pair => Task.Run(async () =>
        {
            try
            {
                var milliseconds = await App.API.StopwatchLogDebugAsync(ClassName, $"Init method time cost for <{pair.Metadata.Name}>",
                    () => pair.Plugin.InitAsync(new PluginInitContext(pair.Metadata, App.API)));

                pair.Metadata.InitTime += milliseconds;
                App.API.LogInfo(ClassName,
                    $"Total init cost for <{pair.Metadata.Name}> is <{pair.Metadata.InitTime}ms>");
            }
            catch (Exception e)
            {
                App.API.LogFatal(ClassName, $"Fail to Init plugin: {pair.Metadata.Name}", e);
                pair.Metadata.Disabled = true;
                failedPlugins.Enqueue(pair);
                App.API.LogDebug(ClassName, $"Disable plugin <{pair.Metadata.Name}> because init failed");
            }
        }));

        await Task.WhenAll(InitTasks);

        if (!failedPlugins.IsEmpty)
        {
            var failedPluginsStr = string.Join(",", failedPlugins.Select(x => x.Metadata.Name));
            App.API.ShowMsg(
                Localize.PluginManager_FailedToInitializePluginsTitle(),
                Localize.PluginManager_FailedToInitializePluginsMessage(failedPluginsStr));
        }
    }

    #endregion

    #region Plugin List

    public static PluginPair[] GetTranslationPlugins()
    {
        return [.. _translationPlugins.Where(p => !PluginModified(p.Metadata.ID))];
    }

    private static bool PluginModified(string id)
    {
        return ModifiedPlugins.Contains(id);
    }

    #endregion

    #region Bar Elements

    public static bool CheckBarElement(BarElementModel element)
    {
        var pluginId = element.ID;
        return AllPlugins.Any(p => p.Metadata.ID == pluginId);
    }

    public static FrameworkElement? GetBarElement(BarElementModel element, BarElementPosition position)
    {
        var pluginId = element.ID;
        var plugin = AllPlugins.FirstOrDefault(p => p.Metadata.ID == pluginId);
        if (plugin == null)
        {
            App.API.LogError(ClassName, $"Plugin with ID {pluginId} not found");
            return null;
        }

        return plugin.Plugin.GetBarElement(position);
    }

    #endregion
}
