using System.Threading.Tasks;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Bar.Helpers.Image;
using Flow.Bar.Helpers.Plugins;

namespace Flow.Bar.Models.Plugins;

public partial class PluginViewModel : ObservableObject
{
    public PluginPair PluginPair { get; }

    public string ID { get; }

    public PluginViewModel(PluginPair pluginPair)
    {
        PluginPair = pluginPair;
        ID = pluginPair.Metadata.ID;
        Disabled = !PluginManager.IsInitialized(pluginPair.Metadata.ID);
        Name = pluginPair.Metadata.Name;
        Description = pluginPair.Metadata.Description;
        VersionAndAuthor = Localize.SettingPanePlugins_VersionAndAuthor(pluginPair.Metadata.Version, pluginPair.Metadata.Author);
    }

    private bool _imageLoaded = false;

    private ImageSource _image = ImageLoader.MissingImage;
    public ImageSource Image
    {
        get
        {
            if (!_imageLoaded)
            {
                _imageLoaded = true;
                _ = LoadIconAsync();
            }

            return _image;
        }
        set => _image = value;
    }

    private async Task LoadIconAsync()
    {
        Image = await App.API.LoadImageAsync(PluginPair.Metadata.IcoPath);
        OnPropertyChanged(nameof(Image));
    }

    [ObservableProperty]
    private bool _disabled = false;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _versionAndAuthor = string.Empty;
}
