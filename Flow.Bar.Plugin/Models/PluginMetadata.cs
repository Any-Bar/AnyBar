using System.IO;
using System.Text.Json.Serialization;

namespace Flow.Bar.Plugin;

/// <summary>
/// Plugin metadata.
/// </summary>
public class PluginMetadata
{
    /// <summary>
    /// Plugin ID.
    /// </summary>
    public string ID { get; set; }

    /// <summary>
    /// Plugin name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Plugin author.
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// Plugin version.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Plugin description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Plugin website.
    /// </summary>
    public string Website { get; set; }

    /// <summary>
    /// Whether plugin is disabled.
    /// </summary>
    /// <remarks>
    /// If plugin failed to load or init, it will be disabled.
    /// </remarks>
    public bool Disabled { get; set; }

    /// <summary>
    /// Plugin execute file path.
    /// </summary>
    public string ExecuteFilePath { get; private set; }

    /// <summary>
    /// Plugin execute file name.
    /// </summary>
    public string ExecuteFileName { get; set; }

    /// <summary>
    /// Plugin assembly name.
    /// </summary>
    [JsonIgnore]
    public string AssemblyName { get; set; }

    private string _pluginDirectory;

    /// <summary>
    /// Plugin source directory.
    /// </summary>
    public string PluginDirectory
    {
        get => _pluginDirectory;
        set
        {
            _pluginDirectory = value;
            ExecuteFilePath = Path.Combine(value, ExecuteFileName);
            IcoPath = Path.Combine(value, IcoPath);
        }
    }

    /// <summary>
    /// Plugin icon path.
    /// </summary>
    public string IcoPath { get; set;}

    /// <summary>
    /// Init time include both plugin load time and init time.
    /// </summary>
    [JsonIgnore]
    public long InitTime { get; set; }

    /// <summary>
    /// The path to the plugin settings directory which is not validated.
    /// It is used to store plugin settings files and data files.
    /// When plugin is deleted, FL will ask users whether to keep its settings.
    /// If users do not want to keep, this directory will be deleted.
    /// </summary>
    public string PluginSettingsDirectoryPath { get; set; }

    /// <summary>
    /// The path to the plugin cache directory which is not validated.
    /// It is used to store cache files.
    /// When plugin is deleted, this directory will be deleted as well.
    /// </summary>
    public string PluginCacheDirectoryPath { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }
}
