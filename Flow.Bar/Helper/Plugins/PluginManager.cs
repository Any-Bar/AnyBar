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

    private static List<PluginPair> _allLoadedPlugins = null!;
    private static readonly ConcurrentDictionary<string, PluginPair> _allInitializedPlugins = [];
    private static readonly ConcurrentDictionary<string, PluginPair> _initFailedPlugins = [];

    private static readonly ConcurrentBag<string> _uninstalledPlugins = [];

    private static readonly ConcurrentBag<PluginPair> _translationPlugins = [];
    private static readonly ConcurrentDictionary<string, PluginPair> _rightClickMenuPlugins = [];

    /// <summary>
    /// Directories that will hold Flow Bar plugin directory.
    /// </summary>
    public static readonly string[] Directories =
    [
        Constants.PreinstalledDirectory, DataLocation.PluginsDirectory
    ];

    static PluginManager()
    {
        // Validate user directory
        if (!Directory.Exists(DataLocation.PluginsDirectory))
        {
            Directory.CreateDirectory(DataLocation.PluginsDirectory);
        }
    }

    #region Loading & Initialize Plugins

    /// <summary>
    /// Load plugins from the directories specified in Directories.
    /// </summary>
    public static void LoadPlugins()
    {
        var metadatas = PluginConfig.Parse(Directories);

        // Load plugins
        _allLoadedPlugins = PluginsLoader.Plugins(metadatas);

        // Since dotnet plugins need to get assembly name first, we should update plugin directory after loading plugins
        UpdatePluginDirectory(metadatas);
    }

    private static void UpdatePluginDirectory(List<PluginMetadata> metadatas)
    {
        foreach (var metadata in metadatas)
        {
            if (string.IsNullOrEmpty(metadata.AssemblyName))
            {
                App.API.LogWarning(ClassName, $"{nameof(metadata.AssemblyName)} is empty for plugin with metadata: {metadata.Name}");
                continue; // Skip if AssemblyName is not set, which can happen for erroneous plugins
            }
            metadata.PluginSettingsDirectoryPath = Path.Combine(DataLocation.PluginSettingsDirectory, metadata.AssemblyName);
            metadata.PluginCacheDirectoryPath = Path.Combine(DataLocation.PluginCacheDirectory, metadata.AssemblyName);
        }
    }

    /// <summary>
    /// Initialize all plugins asynchronously.
    /// </summary>
    public static async Task InitializePluginsAsync()
    {
        var InitTasks = _allLoadedPlugins.Select(pair => Task.Run(async () =>
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
                App.API.LogDebug(ClassName, $"Disable plugin <{pair.Metadata.Name}> because initialization failed");

                // Even if the plugin cannot be initialized, we still need to add it in all plugin list so that
                // we can remove the plugin from Plugin or Store page or Plugin Manager plugin.
                _allInitializedPlugins.TryAdd(pair.Metadata.ID, pair);
                _initFailedPlugins.TryAdd(pair.Metadata.ID, pair);
                return;
            }

            // Add plugin to lists after the plugin is initialized
            AddPluginToLists(pair);
        }));

        await Task.WhenAll(InitTasks);

        if (!_initFailedPlugins.IsEmpty)
        {
            var failedPluginsStr = string.Join(",", _initFailedPlugins.Values.Select(x => x.Metadata.Name));
            App.API.ShowMsg(
                Localize.PluginManager_FailedToInitializePluginsTitle(),
                Localize.PluginManager_FailedToInitializePluginsMessage(failedPluginsStr));
        }
    }

    private static void AddPluginToLists(PluginPair pair)
    {
        if (pair.Plugin is IPluginI18n)
        {
            _translationPlugins.Add(pair);
        }
        if (pair.Plugin is IRightClickMenu)
        {
            _rightClickMenuPlugins.TryAdd(pair.Metadata.ID, pair);
        }
        _allInitializedPlugins.TryAdd(pair.Metadata.ID, pair);
    }

    #endregion

    #region Get Plugin List

    public static List<PluginPair> GetAllLoadedPlugins()
    {
        return [.. _allLoadedPlugins];
    }

    public static List<PluginPair> GetAllInitializedPlugins(bool includeFailed)
    {
        if (includeFailed)
        {
            return [.. _allInitializedPlugins.Values];
        }
        else
        {
            return [.. _allInitializedPlugins.Values
                    .Where(p => !_initFailedPlugins.ContainsKey(p.Metadata.ID))];
        }
    }

    public static PluginPair[] GetTranslationPlugins()
    {
        return [.. _translationPlugins.Where(p => !PluginModified(p.Metadata.ID))];
    }

    #endregion

    #region Get Plugin

    /// <summary>
    /// Get specified plugin, return null if not found.
    /// </summary>
    /// <remarks>
    /// Plugin may not be initialized, so do not use its plugin model to execute any commands.
    /// </remarks>
    /// <param name="id"></param>
    /// <returns></returns>
    public static PluginPair? GetPluginForId(string id)
    {
        return GetAllLoadedPlugins().FirstOrDefault(o => o.Metadata.ID == id);
    }

    #endregion

    #region Check Plugin

    public static bool IsPreinstalled(string id)
    {
        return id == Constants.FlowBarPluginClockPluginId;
    }

    public static bool IsInitialized(string id)
    {
        return _allInitializedPlugins.ContainsKey(id) && !_initFailedPlugins.ContainsKey(id);
    }

    private static bool PluginModified(string id)
    {
        return _uninstalledPlugins.Contains(id);
    }

    #endregion

    #region Bar Elements

    public static bool CheckBarElement(BarElementModel element)
    {
        var pluginId = element.ID;
        return GetAllInitializedPlugins(false).Any(p => p.Metadata.ID == pluginId);
    }

    public static FrameworkElement? GetBarElement(BarElementModel element, BarElementPosition position)
    {
        var pluginId = element.ID;
        var plugin = GetAllInitializedPlugins(false).FirstOrDefault(p => p.Metadata.ID == pluginId);
        if (plugin == null)
        {
            App.API.LogError(ClassName, $"Plugin with ID <{pluginId}> not found");
            return null;
        }

        return plugin.Plugin.GetBarElement(position);
    }

    #endregion

    #region Right Click Menu

    public static IRightClickMenu? GetRightClickMenu(string id)
    {
        if (_rightClickMenuPlugins.TryGetValue(id, out var pair))
        {
            return (IRightClickMenu)pair.Plugin;
        }

        return null;
    }

    #endregion

    #region Plugin Install & Uninstall & Update

    internal static async Task<bool> UninstallPluginAsync(PluginMetadata plugin)
    {
        {
            _allLoadedPlugins.RemoveAll(p => p.Metadata.ID == plugin.ID);
        }
        {
            _allInitializedPlugins.TryRemove(plugin.ID, out var _);
        }
        {
            _initFailedPlugins.TryRemove(plugin.ID, out var _);
        }

        // Marked for deletion. Will be deleted on next start up
        using var _ = File.CreateText(Path.Combine(plugin.PluginDirectory, Constants.NeedDeleteMarkFile));

        _uninstalledPlugins.Add(plugin.ID);

        await Task.CompletedTask;

        return true;
    }

    #endregion
}
