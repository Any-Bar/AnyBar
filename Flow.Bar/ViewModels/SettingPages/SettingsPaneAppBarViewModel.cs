using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Controls;
using Flow.Bar.Converters;
using Flow.Bar.Dialogs;
using Flow.Bar.Interfaces;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Services;
using Flow.Bar.Views;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
                App.API.LogFatal(ClassName, "Failed to show AddAppBarDialog", ex);
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
                Name = dialog.AppBarName,
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

    public async void OnNavigatedTo(object? parameter)
    {
        await DockModeToImageSourceConverter.InitializeAsync();
        RefreshAppBars();
        AppBars.CollectionChanged += AppBars_CollectionChanged;
    }

    public void OnNavigatedFrom()
    {
        AppBars.CollectionChanged -= AppBars_CollectionChanged;
    }

    private void AppBars_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Move)
        {
            if (e.OldItems == null ||
                e.NewItems == null ||
                e.OldItems.Count != e.NewItems.Count)
            {
                App.API.LogError(ClassName, "Move action in AppBars collection changed with different item counts");
                return;
            }
            _appBarManagementService.ChangeAppBarOrder(e.OldStartingIndex, e.NewStartingIndex, e.OldItems.Count);
        }
    }
}
