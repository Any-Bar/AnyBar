using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Interfaces;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Models.Parameter;
using Flow.Bar.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Flow.Bar.ViewModels.SettingPages;

public partial class SettingsPaneBarElementSettingViewModel(AppBarManagementService appBarManagementService, NavigationViewService navigationService) : ObservableObject, INavigationAware
{
    private static readonly string ClassName = nameof(SettingsPaneBarElementSettingViewModel);

    private readonly AppBarManagementService _appBarManagementService = appBarManagementService;

    private BarElementModelPosition _position = BarElementModelPosition.LeftOrTop;

    private AppBarModel _model = null!;

    [ObservableProperty]
    private bool _isInitialized = false;

    public ScrollViewer? RootFrameScrollViewer { get; } = navigationService.ScrollViewer;

    [RelayCommand]
    private async Task AddBarElementAsync(Button button)
    {
        /*AddAppBarDialog dialog;
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
        }*/
    }

    public ObservableCollection<BarElementModel> BarElements { get; } = [];

    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is SettingsPaneBarElementSettingNavigationParameter args)
        {
            if (!IsInitialized)
            {
                _position = args.Position;
                _model = args.Model;
                RefreshBarElements();
                IsInitialized = true;
            }
        }
        else
        {
            App.API.LogError(ClassName, "Parameter is not of type AppBarModel");
        }
        BarElements.CollectionChanged += BarElements_CollectionChanged;
    }

    private void RefreshBarElements()
    {
        BarElements.Clear();
        var barElements = _position switch
        {
            BarElementModelPosition.LeftOrTop => AppBarManagementService.GetOrderedLeftOrTopBarElements(_model),
            BarElementModelPosition.Center => AppBarManagementService.GetOrderedCenterBarElements(_model),
            BarElementModelPosition.RightOrBottom => AppBarManagementService.GetOrderedRightOrBottomBarElements(_model),
            _ => throw new NotSupportedException($"Unsupported {nameof(BarElementModelPosition)}: {_position}")
        };
        foreach (var element in barElements)
        {
            BarElements.Add(element);
        }
    }

    public void OnNavigatedFrom()
    {
        IsInitialized = false;
        BarElements.CollectionChanged -= BarElements_CollectionChanged;
    }

    private void BarElements_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Move)
        {
            if (e.OldItems == null ||
                e.NewItems == null ||
                e.OldItems.Count != e.NewItems.Count)
            {
                App.API.LogError(ClassName, $"Move action in {nameof(BarElements)} collection changed with different item counts");
                return;
            }
            /*_appBarManagementService.ChangeBarElementOrder(e.OldStartingIndex, e.NewStartingIndex, e.OldItems.Count);*/
        }
    }
}
