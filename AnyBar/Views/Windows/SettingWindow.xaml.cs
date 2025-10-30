using System.Windows;
using System.Windows.Shell;
using AnyBar.Helpers.Windows;
using AnyBar.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

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
        _nonDraggableChrome = (WindowChrome)_draggableChrome.Clone();
        _nonDraggableChrome.CaptionHeight = 0; // Disable caption height for non-draggable chrome
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
