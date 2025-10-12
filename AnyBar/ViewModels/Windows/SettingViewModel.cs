using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AnyBar.Controls;
using AnyBar.Services;
using Frame = iNKORE.UI.WPF.Modern.Controls.Frame;

namespace AnyBar.ViewModels;

public partial class SettingViewModel(NavigationViewService navigationViewService) : ObservableObject
{
    private readonly NavigationViewService _navigationViewService = navigationViewService;

    private NavigationView _navigationView = null!;

    [RelayCommand]
    private void SaveAppAllSettings()
    {
        App.API.SaveAppAllSettings();
    }

    [RelayCommand]
    private void NavigationViewControlLoaded(NavigationView view)
    {
        _navigationView = view;
        if (view.MenuItems.Count > 0)
        {
            view.SelectedItem = view.MenuItems[0]!;
        }
    }

    [RelayCommand]
    private void RootFrameLoaded(Frame frame)
    {
        _navigationViewService.RegisterFrameEvents(_navigationView, frame);
    }

    [RelayCommand]
    private void RootFrameUnloaded(Frame frame)
    {
        _navigationViewService.UnregisterFrameEvents(frame);
    }
}
