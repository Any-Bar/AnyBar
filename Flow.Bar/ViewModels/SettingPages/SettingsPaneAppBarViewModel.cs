using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Controls;
using Flow.Bar.Dialogs;
using Flow.Bar.Enums;
using Flow.Bar.Interfaces;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Services;
using Flow.Bar.Views;
using iNKORE.UI.WPF.Modern.Controls;

namespace Flow.Bar.ViewModels;

public partial class SettingsPaneAppBarViewModel(AppBarManagementService appBarManagementService, NavigationViewService navigationViewService) : ObservableObject, INavigationAware, INavigationHeader
{
    private static readonly string ClassName = nameof(SettingsPaneAppBarViewModel);

    private readonly AppBarManagementService _appBarManagementService = appBarManagementService;

    private readonly NavigationViewService _navigationViewService = navigationViewService;

    [ObservableProperty]
    private bool _isInitialized = false;

    #region Add AppBar

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
            App.API.LogError(ClassName, $"Failed to get {nameof(ContentDialogEx.Owner)} for {nameof(AddAppBarDialog)}");
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
                    _appBars.Add(x);
                    _appBars = GetSortedAppBars(_appBars);
                    var insertIndex = _appBars.FindIndex(y => y.Order == x.Order);
                    AppBars.Insert(insertIndex, _appBars[insertIndex]);
                }
            });
        }
    }

    #endregion

    #region Sort Mode

    public List<SettingsPaneAppBarSortModeLocalized> AllSortModes { get; } = SettingsPaneAppBarSortModeLocalized.GetValues();

    [ObservableProperty]
    private SettingsPaneAppBarSortMode _sortMode = SettingsPaneAppBarSortMode.Order;

    partial void OnSortModeChanged(SettingsPaneAppBarSortMode value)
    {
        lock (_appBarsLock)
        {
            SortAppBars();
        }
    }

    #endregion

    #region AppBars

    public ObservableCollection<AppBarModel> AppBars { get; } = [];

    private List<AppBarModel> _appBars = null!;

    private readonly Lock _appBarsLock = new();

    private void InitializeAppBars()
    {
        _appBars = _appBarManagementService.GetAllAppBars();
    }

    private void SortAppBars()
    {
        AppBars.Clear();
        foreach (var appBar in GetSortedAppBars(_appBars))
        {
            AppBars.Add(appBar);
        }
    }

    private List<AppBarModel> GetSortedAppBars(List<AppBarModel> appBars)
    {
        return SortMode switch
        {
            SettingsPaneAppBarSortMode.Order => appBars,
            SettingsPaneAppBarSortMode.Status => [.. appBars.OrderBy(x => !x.IsEnabled).ThenBy(x => x.Order)],
            SettingsPaneAppBarSortMode.Name => [.. appBars.OrderBy(x => x.Name)],
            _ => appBars
        };
    }

    private void AppBars_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Move)
        {
            if (e.OldItems == null || e.NewItems == null || e.OldItems.Count == 0 || e.OldItems.Count != e.NewItems.Count)
            {
                App.API.LogError(ClassName, $"Move action in {nameof(AppBars)} collection changed with invalid parameters");
                return;
            }

            if (SortMode == SettingsPaneAppBarSortMode.Order)
            {
                _appBarManagementService.ChangeAppBarOrder(e.OldStartingIndex, e.NewStartingIndex, e.OldItems.Count);
                lock (_appBarsLock)
                {
                    InitializeAppBars();
                }
            }
            else
            {
                App.API.LogError(ClassName, $"Unsupported {nameof(SortMode)}: {SortMode} for {nameof(NotifyCollectionChangedAction.Move)} action in {nameof(AppBars)} collection");
            }
        }
    }

    #endregion

    #region Open AppBar Setting

    [RelayCommand]
    private void OpenAppBarSetting(AppBarModel model)
    {
        _navigationViewService.NavigateTo(SettingPageTag.AppBarSetting, model);
    }

    #endregion

    #region Toggle AppBar Enable

    [RelayCommand]
    private void IsEnabledToggleSwitchToggled(ToggleSwitch toggleSwitch)
    {
        if (toggleSwitch.Tag is not AppBarModel model) return;
        _appBarManagementService.SetEnabled(model.Order, toggleSwitch.IsOn);
    }

    #endregion

    #region INavigationAware

    public void OnNavigatedTo(object? parameter)
    {
        if (!IsInitialized)
        {
            lock (_appBarsLock)
            {
                InitializeAppBars();
                SortAppBars();
            }
            AppBars.CollectionChanged += AppBars_CollectionChanged;
            IsInitialized = true;
        }
    }

    public void OnNavigatedFrom()
    {
        IsInitialized = false;
        AppBars.CollectionChanged -= AppBars_CollectionChanged;
    }

    #endregion

    #region INavigationHeader

    public string? GetHeaderKey()
    {
        return nameof(Localize.SettingWindow_AppBar);
    }

    public string GetHeaderValue()
    {
        throw new NotImplementedException();
    }

    #endregion
}
