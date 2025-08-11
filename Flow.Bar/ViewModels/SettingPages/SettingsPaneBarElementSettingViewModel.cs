using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Controls;
using Flow.Bar.Dialogs;
using Flow.Bar.Extensions.Enumerable;
using Flow.Bar.Interfaces.Navigation;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Models.Parameter;
using Flow.Bar.Services;
using Flow.Bar.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
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
            _appBarManagementService.AddBarElement(_position, _model, dialog.Plugin.ID, (x) =>
            {
                lock (_barElementsLock)
                {
                    _barElements.Add(new BarElementViewModel(x));
                    _barElements = GetSortedBarElements(_barElements);
                    var insertIndex = _barElements.FindIndex(y => y.Order == x.Order);
                    BarElements.Insert(insertIndex, _barElements[insertIndex]);
                }
            });
        }
    }

    public List<SettingsPaneBarElementSettingSortModeLocalized> AllSortModes { get; } = SettingsPaneBarElementSettingSortModeLocalized.GetValues();

    [ObservableProperty]
    private SettingsPaneBarElementSettingSortMode _sortMode = SettingsPaneBarElementSettingSortMode.LeftTopToRightBottom;

    partial void OnSortModeChanged(SettingsPaneBarElementSettingSortMode value)
    {
        lock (_barElementsLock)
        {
            SortBarElements();
        }
    }

    public ObservableCollection<BarElementViewModel> BarElements { get; } = [];

    private List<BarElementViewModel> _barElements = null!;

    private readonly Lock _barElementsLock = new();

    public void OnNavigatedTo(object? parameter)
    {
        void UpdateSortModeLocalization()
        {
            var leftTopToRightBottom = AllSortModes.Find(x => x.Value == SettingsPaneBarElementSettingSortMode.LeftTopToRightBottom)!;
            var rightBottomToLeftTop = AllSortModes.Find(x => x.Value == SettingsPaneBarElementSettingSortMode.RightBottomToLeftTop)!;
            // Horizontal
            if (_model.DockMode == AppBarDockMode.Top || _model.DockMode == AppBarDockMode.Bottom)
            {
                leftTopToRightBottom.LocalizationKey = nameof(Localize.SettingsPaneBarElementSettingSortMode_LeftToRight);
                leftTopToRightBottom.Display = Localize.SettingsPaneBarElementSettingSortMode_LeftToRight();
                rightBottomToLeftTop.LocalizationKey = nameof(Localize.SettingsPaneBarElementSettingSortMode_RightToLeft);
                rightBottomToLeftTop.Display = Localize.SettingsPaneBarElementSettingSortMode_RightToLeft();
            }
            // Vertical
            else
            {
                leftTopToRightBottom.LocalizationKey = nameof(Localize.SettingsPaneBarElementSettingSortMode_TopToBottom);
                leftTopToRightBottom.Display = Localize.SettingsPaneBarElementSettingSortMode_TopToBottom();
                rightBottomToLeftTop.LocalizationKey = nameof(Localize.SettingsPaneBarElementSettingSortMode_BottomToTop);
                rightBottomToLeftTop.Display = Localize.SettingsPaneBarElementSettingSortMode_BottomToTop();
            }
        }

        if (parameter is SettingsPaneBarElementSettingNavigationParameter args)
        {
            if (!IsInitialized)
            {
                _position = args.Position;
                _model = args.Model;
                UpdateSortModeLocalization();
                lock (_barElementsLock)
                {
                    InitializeBarElements();
                    SortBarElements();
                }
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

    private void InitializeBarElements()
    {
        _barElements = [.. _appBarManagementService.GetOrderedBarElements(_position, _model).Select(x => new BarElementViewModel(x))];
    }

    public void RemoveBarElement(BarElementViewModel oldBarElement)
    {
        _appBarManagementService.RemoveBarElement(_position, _model, oldBarElement.Order);
        lock (_barElementsLock)
        {
            _barElements.Remove(oldBarElement);
            BarElements.Remove(BarElements.First(x => x.Order == oldBarElement.Order));
        }
    }

    private void SortBarElements()
    {
        BarElements.Clear();
        foreach (var element in GetSortedBarElements(_barElements))
        {
            BarElements.Add(element);
        }
    }

    private List<BarElementViewModel> GetSortedBarElements(List<BarElementViewModel> allBarElements)
    {
        return SortMode switch
        {
            SettingsPaneBarElementSettingSortMode.LeftTopToRightBottom => allBarElements,
            SettingsPaneBarElementSettingSortMode.RightBottomToLeftTop => allBarElements.Reversed(),
            SettingsPaneBarElementSettingSortMode.Status => [.. allBarElements.OrderBy(x => x.Disabled).ThenBy(x => x.Name)],
            SettingsPaneBarElementSettingSortMode.Name => [.. allBarElements.OrderBy(x => x.Name)],
            _ => allBarElements
        };
    }

    private void BarElements_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Move)
        {
            if (e.OldItems == null ||
                e.NewItems == null ||
                e.OldItems.Count != e.NewItems.Count)
            {
                App.API.LogError(ClassName, $"{nameof(NotifyCollectionChangedAction.Move)} action in {nameof(BarElements)} collection changed with different item counts");
                return;
            }

            if (SortMode == SettingsPaneBarElementSettingSortMode.LeftTopToRightBottom)
            {
                _appBarManagementService.ChangeBarElementOrder(_position, _model, e.OldStartingIndex, e.NewStartingIndex, e.OldItems.Count, true);
                lock (_barElementsLock)
                {
                    InitializeBarElements();
                }
            }
            else if (SortMode == SettingsPaneBarElementSettingSortMode.RightBottomToLeftTop)
            {
                var reversedOldStartingIndex = BarElements.Count - 1 - e.OldStartingIndex;
                var reversedNewStartingIndex = BarElements.Count - 1 - e.NewStartingIndex;
                _appBarManagementService.ChangeBarElementOrder(_position, _model, reversedOldStartingIndex, reversedNewStartingIndex, e.OldItems.Count, true);
                lock (_barElementsLock)
                {
                    InitializeBarElements();
                }
            }
            else
            {
                App.API.LogError(ClassName, $"Unsupported {nameof(SortMode)}: {SortMode} for {nameof(NotifyCollectionChangedAction.Move)} action in {nameof(BarElements)} collection");
            }
        }
    }
}
