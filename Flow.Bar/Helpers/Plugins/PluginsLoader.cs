using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Flow.Bar.Models.Plugins;
using Flow.Bar.Plugin;

namespace Flow.Bar.Helpers.Plugins;

public static class PluginsLoader
{
    private static readonly string ClassName = nameof(PluginsLoader);

    public static List<PluginPair> Plugins(List<PluginMetadata> metadatas)
    {
        var dotnetPlugins = DotNetPlugins(metadatas);

        var plugins = dotnetPlugins
            .ToList();
        return plugins;
    }

    private static List<PluginPair> DotNetPlugins(List<PluginMetadata> source)
    {
        var erroredPlugins = new List<string>();

        var plugins = new List<PluginPair>();

        foreach (var metadata in source)
        {
            var milliseconds = App.API.StopwatchLogDebug(ClassName, $"Constructor init cost for {metadata.Name}", () =>
            {
                Assembly? assembly = null;
                IAsyncPlugin? plugin = null;

                try
                {
                    var assemblyLoader = new PluginAssemblyLoader(metadata.ExecuteFilePath);
                    assembly = assemblyLoader.LoadAssemblyAndDependencies();

                    var type = PluginAssemblyLoader.FromAssemblyGetTypeOfInterface(assembly, typeof(IAsyncPlugin));

                    plugin = Activator.CreateInstance(type) as IAsyncPlugin;

                    metadata.AssemblyName = assembly.GetName().Name;
                }
#if !DEBUG
                catch (Exception)
                {
                    throw;
                }
#else
                catch (Exception e) when (assembly == null)
                {
                    App.API.LogFatal(ClassName, $"Couldn't load assembly for the plugin: {metadata.Name}", e);
                }
                catch (InvalidOperationException e)
                {
                    App.API.LogFatal(ClassName, $"Can't find the required {nameof(IPlugin)} interface for the plugin: <{metadata.Name}>", e);
                }
                catch (ReflectionTypeLoadException e)
                {
                    App.API.LogFatal(ClassName, $"The GetTypes method was unable to load assembly types for the plugin: <{metadata.Name}>", e);
                }
                catch (Exception e)
                {
                    App.API.LogFatal(ClassName, $"The following plugin has errored and can not be loaded: <{metadata.Name}>", e);
                }
#endif

                if (plugin == null)
                {
                    erroredPlugins.Add(metadata.Name);
                    return;
                }

                plugins.Add(new PluginPair { Plugin = plugin, Metadata = metadata });
            });

            metadata.InitTime += milliseconds;
            App.API.LogInfo(ClassName,
                $"Constructor cost for <{metadata.Name}> is <{metadata.InitTime}ms>");
        }

        if (erroredPlugins.Count > 0)
        {
            var errorPluginString = string.Join(Environment.NewLine, erroredPlugins);

            var errorMessage = erroredPlugins.Count > 1 ?
                Localize.PluginLoader_PluginsHaveErrored() :
                Localize.PluginLoader_PluginHasErrored();

            App.API.ShowMsgError($"{errorMessage}{Environment.NewLine}{Environment.NewLine}" +
                $"{errorPluginString}{Environment.NewLine}{Environment.NewLine}" +
                Localize.PluginLoader_ReferToLogs());
        }

        return plugins;
    }
}
