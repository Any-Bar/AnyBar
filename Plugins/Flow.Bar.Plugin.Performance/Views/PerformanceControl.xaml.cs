using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Flow.Bar.Plugin.Performance.Services;

namespace Flow.Bar.Plugin.Performance.Views;

public partial class PerformanceControl : UserControl
{
    private static readonly string ClassName = nameof(PerformanceControl);

    private readonly IPerformanceMonitorService _performanceMonitorService;

    private bool _imageInitialized = false;

    public PerformanceControl(IPerformanceMonitorService performanceMonitorService, BarElementPosition position)
    {
        _performanceMonitorService = performanceMonitorService;
        InitializeComponent();
        OnDockModeChanged(position);
    }

    private void InitializeImages()
    {
        if (_imageInitialized) return;
        var cpuIconPath = Path.Combine(Main.Context.CurrentPluginMetadata.PluginDirectory, "Images", "cpu.png");
        var gpuIconPath = Path.Combine(Main.Context.CurrentPluginMetadata.PluginDirectory, "Images", "gpu.png");
        var memoryIconPath = Path.Combine(Main.Context.CurrentPluginMetadata.PluginDirectory, "Images", "memory.png");
        CpuImage.Source = new BitmapImage(new Uri(cpuIconPath));
        GpuImage.Source = new BitmapImage(new Uri(gpuIconPath));
        MemoryImage.Source = new BitmapImage(new Uri(memoryIconPath));
        _imageInitialized = true;
    }

    private void PerformanceMonitorService_PerformanceDataUpdated(object sender, PerformanceDataEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            InitializeImages();
            CpuTextBlock.Text = $"{e.Data.CpuUsage:F1}%";
            MemoryTextBlock.Text = $"{e.Data.MemoryUsage:F1}%";
            if (e.Data.GpuUsage.HasValue)
            {
                GpuPanel.Visibility = Visibility.Visible;
                GpuTextBlock.Text = $"{e.Data.GpuUsage.Value:F1}%";
            }
            else
            {
                GpuPanel.Visibility = Visibility.Collapsed;
            }
        });
    }

    public void OnBarElementCreated()
    {
        _performanceMonitorService.PerformanceDataUpdated -= PerformanceMonitorService_PerformanceDataUpdated;
        _performanceMonitorService.PerformanceDataUpdated += PerformanceMonitorService_PerformanceDataUpdated;
    }

    public void OnBarElementDeleted()
    {
        _performanceMonitorService.PerformanceDataUpdated -= PerformanceMonitorService_PerformanceDataUpdated;
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
        CpuPanel.Orientation = orientation;
        GpuPanel.Orientation = orientation;
        MemoryPanel.Orientation = orientation;

        GpuPanel.Margin = orientation == Orientation.Horizontal ? new Thickness(6, 0, 6, 0) : new Thickness(0, 6, 0, 6);
        CpuImage.Margin = orientation == Orientation.Horizontal ? new Thickness(0, 0, 6, 0) : new Thickness(0, 0, 0, 6);
        GpuImage.Margin = orientation == Orientation.Horizontal ? new Thickness(0, 0, 6, 0) : new Thickness(0, 0, 0, 6);
        MemoryImage.Margin = orientation == Orientation.Horizontal ? new Thickness(0, 0, 6, 0) : new Thickness(0, 0, 0, 6);
    }
}
