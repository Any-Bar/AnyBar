using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AnyBar.Plugin.Network.Services;
using AnyBar.Plugin.Network.Utils;

namespace AnyBar.Plugin.Network.Views;

public partial class NetworkControl : UserControl
{
    private static readonly string ClassName = nameof(NetworkControl);

    private readonly INetworkMonitorService _networkMonitorService;

    private bool _imageInitialized = false;

    public NetworkControl(INetworkMonitorService performanceMonitorService, BarElementPosition position)
    {
        _networkMonitorService = performanceMonitorService;
        InitializeComponent();
        OnDockModeChanged(position);
    }

    private void InitializeImages()
    {
        if (_imageInitialized) return;
        var uploadIconPath = Path.Combine(Main.Context.CurrentPluginMetadata.PluginDirectory, "Images", "upload.png");
        var downloadIconPath = Path.Combine(Main.Context.CurrentPluginMetadata.PluginDirectory, "Images", "download.png");
        UploadImage.Source = new BitmapImage(new Uri(uploadIconPath));
        DownloadImage.Source = new BitmapImage(new Uri(downloadIconPath));
        _imageInitialized = true;
    }

    private void NetworkMonitorService_PerformanceDataUpdated(object sender, NetworkDataEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            InitializeImages();
            UploadTextBlock.Text = FormatNetworkSpeed(e.Data.UploadSpeed);
            DownloadTextBlock.Text = FormatNetworkSpeed(e.Data.DownloadSpeed);
        });
    }

    private static string FormatNetworkSpeed(float bytes, bool useBps = false)
    {
        var unit = useBps ? "bps" : "B/s";
        if (useBps)
        {
            bytes *= 8;
        }

        return FormatUtils.FormatBytes(bytes, unit);
    }

    public void OnBarElementCreated()
    {
        _networkMonitorService.NetworkDataUpdated -= NetworkMonitorService_PerformanceDataUpdated;
        _networkMonitorService.NetworkDataUpdated += NetworkMonitorService_PerformanceDataUpdated;
    }

    public void OnBarElementDeleted()
    {
        _networkMonitorService.NetworkDataUpdated -= NetworkMonitorService_PerformanceDataUpdated;
    }

    public void OnDockModeChanged(BarElementPosition position)
    {
        Main.Context.API.LogVerbose(ClassName, $"Changed dock mode to {position}");
        switch (position)
        {
            case BarElementPosition.Left:
            case BarElementPosition.HorizontalCenter:
                SetOrientation(Orientation.Horizontal);
                break;
            case BarElementPosition.Right:
                SetOrientation(Orientation.Horizontal);
                break;
            case BarElementPosition.Top:
            case BarElementPosition.VerticalCenter:
                SetOrientation(Orientation.Vertical);
                break;
            case BarElementPosition.Bottom:
                SetOrientation(Orientation.Vertical);
                break;
        }
    }

    private void SetOrientation(Orientation orientation)
    {
        MainPanel.Orientation = orientation;
        UploadPanel.Orientation = orientation;
        DownloadPanel.Orientation = orientation;

        UploadPanel.Margin = orientation == Orientation.Horizontal ? new Thickness(0, 0, 6, 0) : new Thickness(0, 0, 0, 6);
        UploadImage.Margin = orientation == Orientation.Horizontal ? new Thickness(0, 0, 3, 0) : new Thickness(0, 0, 0, 3);
        DownloadImage.Margin = orientation == Orientation.Horizontal ? new Thickness(0, 0, 3, 0) : new Thickness(0, 0, 0, 3);
    }
}
