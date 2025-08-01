using CommunityToolkit.Mvvm.DependencyInjection;
using Flow.Bar.Services;
using System;
using System.Windows;

namespace Flow.Bar.Views;

public partial class SettingWindow : Window
{
    private readonly NavigationViewService _navigationService = Ioc.Default.GetRequiredService<NavigationViewService>();

    public SettingWindow()
    {
        InitializeComponent();
    }

    #region Window Events

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
        _navigationService.RegisterFrameEvents(NavigationViewControl, RootFrame);
    }

    private void RootFrame_Unloaded(object sender, RoutedEventArgs e)
    {
        _navigationService.UnregisterFrameEvents(RootFrame);
    }

    #endregion
}
