using System;
using System.IO;
using System.Windows;
using AnyBar.Helpers.Plugins;

namespace AnyBar.Services;

public class ResourcesService
{
    private const string Folder = "Styles";

    public static void LoadPluginResources()
    {
        foreach (var pluginsDir in PluginManager.Directories)
        {
            if (!Directory.Exists(pluginsDir)) continue;

            // Enumerate all top directories in the plugin directory
            foreach (var dir in Directory.GetDirectories(pluginsDir))
            {
                // Check if the directory contains a resource folder
                var pluginResourcesDir = Path.Combine(dir, Folder);
                if (!Directory.Exists(pluginResourcesDir)) continue;

                // Enumerate all files in the resource folder
                foreach (var file in Directory.GetFiles(pluginResourcesDir))
                {
                    if (!file.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase)) continue;

                    // Load the resource dictionary
                    var resourceDictionary = new ResourceDictionary
                    {
                        Source = new Uri(file, UriKind.Absolute)
                    };
                    // Add the resource dictionary to the application resources
                    Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
                }
            }
        }
    }
}
