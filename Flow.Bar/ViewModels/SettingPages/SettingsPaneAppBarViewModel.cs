using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Controls;
using Flow.Bar.Dialogs;
using Flow.Bar.Interfaces;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Services;
using Flow.Bar.Views;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Flow.Bar.ViewModels.SettingPages;

public partial class SettingsPaneAppBarViewModel(AppBarManagementService appBarManagementService, NavigationViewService navigationService) : ObservableObject, INavigationAware
{
    private static readonly string ClassName = nameof(SettingsPaneAppBarViewModel);

    private readonly AppBarManagementService _appBarManagementService = appBarManagementService;

    private readonly NavigationViewService _navigationService = navigationService;

    [ObservableProperty]
    private bool _isInitialized = false;

    public ScrollViewer? RootFrameScrollViewer { get; } = navigationService.ScrollViewer;

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
                App.API.LogFatal(ClassName, $"Failed to show {nameof(AddAppBarDialog)}", ex);
                return;
            }
        }
        else
        {
            App.API.LogError(ClassName, $"Failed to get {nameof(ContentDialog.Owner)} for {nameof(AddAppBarDialog)}");
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
            _appBarManagementService.AddAppBar(model, (x) =>
            {
                lock (_appBarsLock)
                {
                    AppBars.Add(x);
                }
            });
        }
    }

    public ObservableCollection<AppBarModel> AppBars { get; } = [];

    private readonly Lock _appBarsLock = new();

    public void OnNavigatedTo(object? parameter)
    {
        if (!IsInitialized)
        {
            RefreshAppBars();
            IsInitialized = true;
        }
        AppBars.CollectionChanged += AppBars_CollectionChanged;
    }

    public void OnNavigatedFrom()
    {
        IsInitialized = false;
        AppBars.CollectionChanged -= AppBars_CollectionChanged;
    }

    private void RefreshAppBars()
    {
        lock (_appBarsLock)
        {
            AppBars.Clear();
            foreach (var appBar in _appBarManagementService.GetAllAppBars())
            {
                AppBars.Add(appBar);
            }
        }
    }

    private void AppBars_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Move)
        {
            if (e.OldItems == null ||
                e.NewItems == null ||
                e.OldItems.Count != e.NewItems.Count)
            {
                App.API.LogError(ClassName, $"Move action in {nameof(AppBars)} collection changed with different item counts");
                return;
            }
            _appBarManagementService.ChangeAppBarOrder(e.OldStartingIndex, e.NewStartingIndex, e.OldItems.Count);
        }
    }

    [RelayCommand]
    private void OpenAppBarSetting(AppBarModel model)
    {
        _navigationService.NavigateTo(SettingPageTag.AppBarSetting, model);
    }
}
