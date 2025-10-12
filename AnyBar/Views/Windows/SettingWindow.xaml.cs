using System.Windows;
using System.Windows.Shell;
using CommunityToolkit.Mvvm.DependencyInjection;
using AnyBar.Helpers.Windows;
using AnyBar.ViewModels;

namespace AnyBar.Views;

public partial class SettingWindow : Window
{
    public SettingViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<SettingViewModel>();

    private WindowChrome _draggableChrome = null!;
    private WindowChrome _nonDraggableChrome = null!;

    public SettingWindow()
    {
        DataContext = ViewModel;
        InitializeComponent();
        WindowTracker.TrackWindow(this);
    }

    #region Window Events

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _draggableChrome = WindowChrome.GetWindowChrome(this);
        _nonDraggableChrome = new WindowChrome
        {
            NonClientFrameEdges = _draggableChrome.NonClientFrameEdges,
            CaptionHeight = 0,  // Disable caption height for non-draggable chrome
            CornerRadius = _draggableChrome.CornerRadius,
            GlassFrameThickness = _draggableChrome.GlassFrameThickness,
            ResizeBorderThickness = _draggableChrome.ResizeBorderThickness,
            UseAeroCaptionButtons = _draggableChrome.UseAeroCaptionButtons
        };
    }

    #endregion

    #region Draggable

    public void SetDraggable(bool isDraggable)
    {
        if (isDraggable)
        {
            WindowChrome.SetWindowChrome(this, _draggableChrome);
        }
        else
        {
            WindowChrome.SetWindowChrome(this, _nonDraggableChrome);
        }
    }

    #endregion
}
