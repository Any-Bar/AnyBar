using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Controls;
using Flow.Bar.Dialogs;
using Flow.Bar.Interfaces;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Services;
using Flow.Bar.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Flow.Bar.ViewModels.SettingPages;

public partial class SettingsPaneAppBarViewModel(AppBarManagementService appBarManagementService, NavigationViewService navigationService) : ObservableObject, INavigationAware
{
    private static readonly string ClassName = nameof(SettingsPaneAppBarViewModel);

    public ObservableCollection<AppBarModel> AppBars { get; } = [];

    public ScrollViewer? RootFrameScrollViewer { get; } = navigationService.ScrollViewer;

    private readonly AppBarManagementService _appBarManagementService = appBarManagementService;

    [RelayCommand]
    private async Task AddAppBarAsync(Button button)
    {
        AddAppBarDialog dialog;
        ContentDialogResult result;
        if (Window.GetWindow(button) is SettingWindow owner)
        {
            try
            {
                dialog = new AddAppBarDialog()
                {
                    Owner = owner
                };
                owner.SetDraggable(false);
                result = await dialog.ShowAsync();
                owner.SetDraggable(true);
            }
            catch (Exception ex)
            {
                App.API.LogException(ClassName, "Failed to show AddAppBarDialog", ex);
                return;
            }
        }
        else
        {
            App.API.LogError(ClassName, "Failed to get owner window for AddAppBarDialog");
            return;
        }
        if (result == ContentDialogResult.Primary)
        {
            var model = new AppBarModel
            {
                DockMode = dialog.DockMode,
                MonitorName = dialog.MonitorName,
                FollowSystemTaskbarWidthOrHeight = dialog.FollowSystemTaskbarWidthOrHeight,
                DockedWidthOrHeight = dialog.DockedWidthOrHeight,
                IsResizable = dialog.IsResizable
            };
            _appBarManagementService.AddAppBar(model, AppBars.Add);
        }
    }

    private void RefreshAppBars()
    {
        AppBars.Clear();
        foreach (var appBar in _appBarManagementService.GetAllAppBars())
        {
            AppBars.Add(appBar);
        }
    }

    public void OnNavigatedTo(object? parameter)
    {
        RefreshAppBars();
    }

    public void OnNavigatedFrom()
    {

    }
}
