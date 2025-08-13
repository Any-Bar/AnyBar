using System;
using System.IO;

namespace Flow.Bar.Extensions;

public static class DataLocation
{
    public const string PortableFolderName = "UserData";
    public const string DeletionIndicatorFile = ".dead";
    public static readonly string PortableDataPath = Path.Combine(Constants.ProgramDirectory, PortableFolderName);
    public static readonly string RoamingDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.FlowBar);
    public static string DataDirectory() =>
        PortableDataLocationInUse() ? PortableDataPath : RoamingDataPath;

    public static bool PortableDataLocationInUse() =>
        Directory.Exists(PortableDataPath) && !File.Exists(DeletionIndicatorFile);

    public static string VersionLogDirectory => Path.Combine(LogDirectory, Constants.Version);
    public static string LogDirectory => Path.Combine(DataDirectory(), Constants.Logs);

    public static readonly string CacheDirectory = Path.Combine(DataDirectory(), Constants.Cache);
    public static readonly string SettingsDirectory = Path.Combine(DataDirectory(), Constants.Settings);
    public static readonly string PluginsDirectory = Path.Combine(DataDirectory(), Constants.Plugins);
    public static readonly string ThemesDirectory = Path.Combine(DataDirectory(), Constants.Themes);

    public static readonly string PluginSettingsDirectory = Path.Combine(SettingsDirectory, Constants.Plugins);
    public static readonly string PluginCacheDirectory = Path.Combine(DataDirectory(), Constants.Cache, Constants.Plugins);
}
