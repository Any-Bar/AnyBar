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

    public const string ApplicationFileName = "Flow.Bar.exe";

    private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
    public static readonly string ProgramDirectory = Directory.GetParent(Assembly.Location)!.ToString();
    public static readonly string ExecutablePath = Path.Combine(ProgramDirectory, ApplicationFileName);
    public static readonly string ApplicationDirectory = Directory.GetParent(ProgramDirectory)!.ToString();
    public static readonly string RootDirectory = Directory.GetParent(ApplicationDirectory)!.ToString();

    public static readonly string PreinstalledDirectory = Path.Combine(ProgramDirectory, Plugins);
    public const string IssuesUrl = "https://github.com/Flow-Bar/Flow.Bar/issues";
    public static readonly string Version = FileVersionInfo.GetVersionInfo(Assembly.Location).ProductVersion!;
    public static readonly string Dev = "Dev";

    public static readonly string Images = "Images";
    private static readonly string ImagesDirectory = Path.Combine(ProgramDirectory, Images);
    public static readonly string DefaultIcon = Path.Combine(ImagesDirectory, "app.png");
    public static readonly string ErrorIcon = Path.Combine(ImagesDirectory, "error.png");
    public static readonly string QuestionIcon = Path.Combine(ImagesDirectory, "question.png");
    public static readonly string WarningIcon = Path.Combine(ImagesDirectory, "warning.png");
    public static readonly string InformationIcon = Path.Combine(ImagesDirectory, "information.png");
    public static readonly string MissingImgIcon = Path.Combine(ImagesDirectory, "app_missing_img.png");
    public static readonly string ImageIcon = Path.Combine(ImagesDirectory, "image.png");
    public static readonly string TopAppBarIcon = Path.Combine(ImagesDirectory, "top-toolbar.png");
    public static readonly string BottomAppBarIcon = Path.Combine(ImagesDirectory, "bottom-toolbar.png");
    public static readonly string LeftAppBarIcon = Path.Combine(ImagesDirectory, "left-toolbar.png");
    public static readonly string RightAppBarIcon = Path.Combine(ImagesDirectory, "right-toolbar.png");

    public const string Themes = "Themes";
    public const string Settings = "Settings";
    public const string Logs = "Logs";
    public const string Cache = "Cache";

    public const string SystemLanguageCode = "system";

    public const string FlowBarPluginDateTimePluginId = "3675a0dd-af3b-412f-b257-5e004dea2bd0";

    public const string NeedDeleteMarkFile = ".need_delete";
}
