using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Controls;
using Flow.Bar.Dialogs;
using Flow.Bar.Interfaces;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Models.Parameter;
using Flow.Bar.Services;
using Flow.Bar.Views;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
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
        AddBarElementDialog dialog;
        ContentDialogResult result;
        if (Window.GetWindow(button) is SettingWindow owner)
        {
            try
            {
                dialog = new AddBarElementDialog()
                {
                    Owner = owner
                };
                owner.SetDraggable(false);
                result = await dialog.ShowAsync();
                owner.SetDraggable(true);
            }
            catch (Exception ex)
            {
                App.API.LogFatal(ClassName, $"Failed to show {nameof(AddBarElementDialog)}", ex);
                return;
            }
        }
        else
        {
            App.API.LogError(ClassName, $"Failed to get {nameof(ContentDialog.Owner)} for {nameof(AddBarElementDialog)}");
            return;
        }
        if (result == ContentDialogResult.Primary && dialog.Plugin != null)
        {
            _appBarManagementService.AddBarElement(_position, _model, dialog.Plugin.ID, BarElements.Add);
        }
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
            App.API.LogError(ClassName, $"{nameof(parameter)} is not of type {nameof(SettingsPaneBarElementSettingNavigationParameter)}");
        }
        BarElements.CollectionChanged += BarElements_CollectionChanged;
    }

    public void OnNavigatedFrom()
    {
        IsInitialized = false;
        BarElements.CollectionChanged -= BarElements_CollectionChanged;
    }

    private void RefreshBarElements()
    {
        BarElements.Clear();
        foreach (var element in _appBarManagementService.GetOrderedBarElements(_position, _model))
        {
            BarElements.Add(element);
        }
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
            _appBarManagementService.ChangeBarElementOrder(_position, _model, e.OldStartingIndex, e.NewStartingIndex, e.OldItems.Count);
        }
    }
}
