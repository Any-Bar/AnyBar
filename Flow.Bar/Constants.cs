using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Flow.Bar;

public static class Constants
{
    public const string FlowBar = "FlowBar";
    public const string FlowBarFullName = "Flow Bar";
    public const string Plugins = "Plugins";
    public const string PluginMetadataFileName = "plugin.json";

    public const string ApplicationFileName = FlowBar + ".exe";

    private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
    public static readonly string ProgramDirectory = Directory.GetParent(Assembly.Location)!.ToString();
    public static readonly string ExecutablePath = Path.Combine(ProgramDirectory, FlowBar + ".exe");
    public static readonly string ApplicationDirectory = Directory.GetParent(ProgramDirectory)!.ToString();
    public static readonly string RootDirectory = Directory.GetParent(ApplicationDirectory)!.ToString();

    public static readonly string PreinstalledDirectory = Path.Combine(ProgramDirectory, Plugins);
    public const string IssuesUrl = "https://github.com/Flow-Bar/Flow.Bar/issues";
    public static readonly string Version = FileVersionInfo.GetVersionInfo(Assembly.Location).ProductVersion!;
    public static readonly string Dev = "Dev";

    private static readonly string ImagesDirectory = Path.Combine(ProgramDirectory, "Images");
    public static readonly string DefaultIcon = Path.Combine(ImagesDirectory, "app.png");

    public const string Themes = "Themes";
    public const string Settings = "Settings";
    public const string Logs = "Logs";
    public const string Cache = "Cache";

    public const string SystemLanguageCode = "system";
}
