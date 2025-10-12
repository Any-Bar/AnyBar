using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AnyBar.Controls;
using AnyBar.Dialogs;
using AnyBar.Enums;
using AnyBar.Extensions;
using AnyBar.Helpers.MenuFlyout;
using AnyBar.Interfaces;
using AnyBar.Models.AppBar;
using AnyBar.Models.Parameters;
using AnyBar.Services;
using AnyBar.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iNKORE.UI.WPF.Modern.Controls;

namespace AnyBar.ViewModels;

public partial class SettingsPaneBarElementSettingViewModel(AppBarManagementService appBarManagementService) : ObservableObject, INavigationAware, INavigationHeader
{
    private static readonly string ClassName = nameof(SettingsPaneBarElementSettingViewModel);

    private readonly AppBarManagementService _appBarManagementService = appBarManagementService;

    private BarElementModelPosition _position = BarElementModelPosition.LeftOrTop;

    private AppBarModel _model = null!;

    [ObservableProperty]
    private bool _isInitialized = false;

    #region Add Bar Element

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
            App.API.LogError(ClassName, $"Failed to get {nameof(ContentDialogEx.Owner)} for {nameof(AddBarElementDialog)}");
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
            }, true);
        }
    }

    #endregion

    #region Sort Mode

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

    #endregion

    #region Bar Elements

    public ObservableCollection<BarElementViewModel> BarElements { get; } = [];

    private List<BarElementViewModel> _barElements = null!;

    private readonly Lock _barElementsLock = new();

    private void InitializeBarElements()
    {
        _barElements = [.. _appBarManagementService.GetOrderedBarElements(_position, _model).Select(x => new BarElementViewModel(x))];
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
            if (e.OldItems == null || e.NewItems == null || e.OldItems.Count == 0 || e.OldItems.Count != e.NewItems.Count)
            {
                App.API.LogError(ClassName, $"{nameof(NotifyCollectionChangedAction.Move)} action in {nameof(BarElements)} collection changed with invalid parameters");
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

    #endregion

    #region Menu Flyout

    private static readonly double ContextMenuWidth = (double)Application.Current.TryFindResource("CustomContextMenuWidth");
    private static readonly double SecondaryContextMenuWidth = (double)Application.Current.TryFindResource("SecondaryContextMenuWidth");
    private static readonly double SecondaryContextMenuHeight = (double)Application.Current.TryFindResource("SecondaryContextMenuHeight");
    private static readonly Style BarElementRemoveContextMenuStyle = (Style)Application.Current.TryFindResource("BarElementRemoveContextMenuStyle");

    private DoubleMenuFlyoutHelper<BarElementViewModel> _menuFlyoutHelper = null!;

    private void InitializeMenuFlyoutHelper()
    {
        _menuFlyoutHelper = new(
            ContextMenuWidth,
            SecondaryContextMenuWidth,
            SecondaryContextMenuHeight,
            BarElementRemoveContextMenuStyle,
            "RemoveButton",
            RemoveBarElement);
        var removeItem = new MenuItem();
        removeItem.SetResourceReference(HeaderedItemsControl.HeaderProperty, nameof(Localize.SettingPaneAppBarSetting_Remove));
        removeItem.Click += RemoveItem_Click;
        _menuFlyoutHelper.Items.Add(removeItem);
    }

    [RelayCommand]
    private void ShowBarElementMoreOptions(Button button)
    {
        _menuFlyoutHelper.ButtonClick(button);
    }

    private void RemoveItem_Click(object sender, RoutedEventArgs e)
    {
        _menuFlyoutHelper.MenuItemClick();
    }

    private void RemoveBarElement(BarElementViewModel oldBarElement)
    {
        _appBarManagementService.RemoveBarElement(_position, _model, oldBarElement.Order, true);
        lock (_barElementsLock)
        {
            BarElements.Remove(BarElements.First(x => x.Order == oldBarElement.Order));
            _barElements.RemoveAll(x => x.Order == oldBarElement.Order);
        }
    }

    #endregion

    #region INavigationAware

    public void OnNavigatedTo(object? parameter)
    {
        void UpdateSortModeLocalization(AppBarModel model)
        {
            var leftTopToRightBottom = AllSortModes.Find(x => x.Value == SettingsPaneBarElementSettingSortMode.LeftTopToRightBottom)!;
            var rightBottomToLeftTop = AllSortModes.Find(x => x.Value == SettingsPaneBarElementSettingSortMode.RightBottomToLeftTop)!;
            // Horizontal
            if (model.DockMode == AppBarDockMode.Top || model.DockMode == AppBarDockMode.Bottom)
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
                UpdateSortModeLocalization(_model);
                lock (_barElementsLock)
                {
                    InitializeBarElements();
                    SortBarElements();
                }
                BarElements.CollectionChanged += BarElements_CollectionChanged;
                IsInitialized = true;
            }
        }
        else if (parameter is SettingsPaneBarElementSettingReorderParameter reorder)
        {
            if (reorder.Model == _model && reorder.Position == _position && IsInitialized)
            {
                lock (_barElementsLock)
                {
                    InitializeBarElements();
                    SortBarElements();
                }
            }
        }
        else if (parameter is SettingsPaneBarElementSettingInsertParameter insert)
        {
            if (insert.Model == _model && insert.Position == _position && IsInitialized)
            {
                lock (_barElementsLock)
                {
                    _barElements.Insert(insert.Order, new BarElementViewModel(insert.BarElement));
                    _barElements = GetSortedBarElements(_barElements);
                    var insertIndex = _barElements.FindIndex(y => y.Order == insert.BarElement.Order);
                    BarElements.Insert(insertIndex, _barElements[insertIndex]);
                }
            }
        }
        else if (parameter is SettingsPaneBarElementSettingRemoveParameter remove)
        {
            if (remove.Model == _model && remove.Position == _position && IsInitialized)
            {
                lock (_barElementsLock)
                {
                    BarElements.Remove(BarElements.First(x => x.Order == remove.Order));
                    _barElements.RemoveAll(x => x.Order == remove.Order);
                }
            }
        }
        else
        {
            App.API.LogError(ClassName, $"{nameof(parameter)} is not of type {nameof(SettingsPaneBarElementSettingNavigationParameter)} or {nameof(SettingsPaneBarElementSettingReorderParameter)}");
        }

        // Only need to initialize once
        InitializeMenuFlyoutHelper();
    }

    public void OnNavigatedFrom()
    {
        IsInitialized = false;
        BarElements.CollectionChanged -= BarElements_CollectionChanged;
    }

    #endregion

    #region INavigationHeader

    public string? GetHeaderKey()
    {
        if (_model == null)
        {
            return nameof(Localize.SettingPaneAppBarSetting_BarElements);
        }
        else
        {
            return _position switch
            {
                BarElementModelPosition.Center => nameof(Localize.SettingPaneAppBarSetting_CenterBarElements),
                BarElementModelPosition.LeftOrTop => _model.DockMode switch
                {
                    AppBarDockMode.Top or AppBarDockMode.Bottom => nameof(Localize.SettingPaneAppBarSetting_LeftBarElements),
                    AppBarDockMode.Left or AppBarDockMode.Right => nameof(Localize.SettingPaneAppBarSetting_TopBarElements),
                    _ => throw new NotImplementedException(),
                },
                BarElementModelPosition.RightOrBottom => _model.DockMode switch
                {
                    AppBarDockMode.Top or AppBarDockMode.Bottom => nameof(Localize.SettingPaneAppBarSetting_RightBarElements),
                    AppBarDockMode.Left or AppBarDockMode.Right => nameof(Localize.SettingPaneAppBarSetting_BottomBarElements),
                    _ => throw new NotImplementedException(),
                },
                _ => throw new NotImplementedException(),
            };
        }
    }

    public string GetHeaderValue()
    {
        throw new NotImplementedException();
    }

    #endregion
}
