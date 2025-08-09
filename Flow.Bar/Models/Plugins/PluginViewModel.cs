using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Bar.Helper.Image;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Flow.Bar.Models.Plugins;

public partial class PluginViewModel : ObservableObject
{
    public PluginPair PluginPair { get; }

    public string ID { get; }

    public PluginViewModel(PluginPair pluginPair)
    {
        PluginPair = pluginPair;
        ID = pluginPair.Metadata.ID;
        Name = pluginPair.Metadata.Name;
        Description = pluginPair.Metadata.Description;
        Version = pluginPair.Metadata.Version;
        Author = pluginPair.Metadata.Author;
    }

    private ImageSource _image = ImageLoader.MissingImage;
    public ImageSource Image
    {
        get
        {
            if (_image == ImageLoader.MissingImage)
            {
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
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _version = string.Empty;

    [ObservableProperty]
    private string _author = string.Empty;
}
