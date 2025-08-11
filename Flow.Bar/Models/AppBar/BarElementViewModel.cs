using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Bar.Helper.Image;
using Flow.Bar.Helper.Plugins;
using Flow.Bar.Models.Plugins;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Flow.Bar.Models.AppBar;

public partial class BarElementViewModel : ObservableObject
{
    public BarElementModel BarElementModel { get; }

    public PluginPair? PluginPair { get; }

    public int Order => BarElementModel.Order;

    public BarElementViewModel(BarElementModel barElementModel)
    {
        BarElementModel = barElementModel;
        PluginPair = PluginManager.AllPlugins.FirstOrDefault(p => p.Metadata.ID == barElementModel.ID);
        if (PluginPair != null)
        {
            Disabled = PluginPair.Metadata.Disabled;
            Name = PluginPair.Metadata.Name;
        }
        else
        {
            Disabled = true;
            Name = barElementModel.Name;
        }
    }

    private ImageSource _image = ImageLoader.MissingImage;
    public ImageSource Image
    {
        get
        {
            if (_image == ImageLoader.MissingImage && PluginPair != null)
            {
                _ = LoadIconAsync();
            }

            return _image;
        }
        set => _image = value;
    }

    private async Task LoadIconAsync()
    {
        Image = await App.API.LoadImageAsync(PluginPair!.Metadata.IcoPath);
        OnPropertyChanged(nameof(Image));
    }

    [ObservableProperty]
    private bool _disabled = false;

    [ObservableProperty]
    private string _name = string.Empty;
}
