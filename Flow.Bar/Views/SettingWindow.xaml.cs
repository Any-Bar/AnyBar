using CommunityToolkit.Mvvm.DependencyInjection;
using Flow.Bar.Services;
using System;
using System.Windows;
using System.Windows.Shell;

namespace Flow.Bar.Views;

public partial class SettingWindow : Window
{
    private readonly NavigationViewService _navigationService = Ioc.Default.GetRequiredService<NavigationViewService>();

    private WindowChrome _draggableChrome = null!;
    private WindowChrome _nonDraggableChrome = null!;

    public SettingWindow()
    {
        InitializeComponent();
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

    private void Window_Closed(object sender, EventArgs e)
    {
        App.API.SaveAppAllSettings();
    }

    #endregion

    #region Navigation View Events

    private void NavigationViewControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Select the first item by default
        NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems[0]!;
    }

    #endregion

    #region Frame Events

    private void RootFrame_Loaded(object sender, RoutedEventArgs e)
    {
        _navigationService.RegisterFrameEvents(NavigationViewControl, RootFrameScrollViewer, RootFrame);
    }

    private void RootFrame_Unloaded(object sender, RoutedEventArgs e)
    {
        _navigationService.UnregisterFrameEvents(RootFrame);
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
