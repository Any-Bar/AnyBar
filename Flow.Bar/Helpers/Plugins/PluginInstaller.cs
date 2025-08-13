using System;
using System.Threading.Tasks;
using Flow.Bar.Plugin;

namespace Flow.Bar.Helpers.Plugins;

/// <summary>
/// Class for installing, updating, and uninstalling plugins.
/// </summary>
public static class PluginInstaller
{
    private static readonly string ClassName = nameof(PluginInstaller);

    public static async Task<bool> UninstallPluginAndCheckRestartAsync(PluginMetadata oldPlugin)
    {
        try
        {
            if (!await PluginManager.UninstallPluginAsync(oldPlugin))
            {
                return false;
            }
        }
        catch (Exception e)
        {
            App.API.ShowMsgError(Localize.PluginInstaller_FailedToUninstall(oldPlugin));
            App.API.LogFatal(ClassName, $"Failed to uninstall plugin <{oldPlugin}>", e);
            return false;
        }

        return true;
    }
}
