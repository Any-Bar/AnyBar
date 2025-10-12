using System.Threading.Tasks;
using System.Windows.Media;
using AnyBar.Helpers.Image;
using AnyBar.Helpers.Plugins;
using AnyBar.Models.Plugins;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnyBar.Models.AppBar;

public partial class BarElementViewModel : ObservableObject
{
    public BarElementModel BarElementModel { get; }

    public PluginPair? PluginPair { get; }

    public int Order => BarElementModel.Order;

    public BarElementViewModel(BarElementModel barElementModel)
    {
        BarElementModel = barElementModel;
        PluginPair = PluginManager.GetPluginForId(barElementModel.ID);
        if (PluginPair != null)
        {
            Disabled = !PluginManager.IsInitialized(PluginPair.Metadata.ID);
            Name = PluginPair.Metadata.Name;
        }
        else
        {
            Disabled = true;
            Name = barElementModel.Name;
        }
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
        if (PluginPair != null)
        {
            Image = await App.API.LoadImageAsync(PluginPair!.Metadata.IcoPath);
            OnPropertyChanged(nameof(Image));
        }
    }

    [ObservableProperty]
    private bool _disabled = false;

    [ObservableProperty]
    private string _name = string.Empty;
}
