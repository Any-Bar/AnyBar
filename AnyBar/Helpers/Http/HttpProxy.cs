using CommunityToolkit.Mvvm.ComponentModel;

namespace AnyBar.Helpers.Http;

public partial class HttpProxy : ObservableObject
{
    [ObservableProperty]
    private bool _enabled = false;

    [ObservableProperty]
    private string _server = string.Empty;

    [ObservableProperty]
    private int _port;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;
}
