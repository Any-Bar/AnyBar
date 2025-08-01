using Flow.Bar.Controls.NavigationView;
using Flow.Bar.Interfaces;
using Flow.Bar.Models.Enums;
using iNKORE.UI.WPF.Modern.Media.Animation;
using System;
using System.Collections.Generic;
using System.Windows.Navigation;
using Frame = iNKORE.UI.WPF.Modern.Controls.Frame;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace Flow.Bar.Services;

public class NavigationViewService(PageService pageService)
{
    private readonly PageService _pageService = pageService;

    private NavigationView? _navigationView;
    private Frame? _frame;

    private readonly Dictionary<Type, object?> _nextParameter = [];

    // Due to the limitation of WPF & iNKORE.UI.WPF.Modern framewoork.
    // we cannot use a stack for the parameters so that we can go back to the previous parameter.
    private readonly Stack<object?> _parameterStack = [];

    /// <summary>
    /// Registers the frame events for navigation.
    /// </summary>
    /// <param name="view"></param>
    /// <param name="frame"></param>
    /// <param name="parameter"></param>
    /// <param name="navigate"></param>
    public void RegisterFrameEvents(NavigationView view, Frame frame, object? parameter = null, bool navigate = true)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(frame);

        UnregisterFrameEvents(frame);
        _navigationView = view;
        _navigationView.BackRequested += NavigationView_BackRequested;
        _navigationView.SelectionChanged += NavigationView_SelectionChanged;
        _frame = frame;
        _frame.Navigating += Frame_OnNavigating;
        _frame.Navigated += Frame_OnNavigated;

        if (navigate && view.SelectedItem is NavigationViewItem item && item.Tag is SettingPageTag tag)
        {
            // Navigate to the default page
            NavigateTo(tag, parameter);
        }
    }

    /// <summary>
    /// Unregisters the frame events for navigation.
    /// </summary>
    /// <param name="frame"></param>
    public void UnregisterFrameEvents(Frame frame)
    {
        if (_frame != null)
        {
            _frame.Navigating -= Frame_OnNavigating;
            _frame.Navigated -= Frame_OnNavigated;
            _navigationView = null;
            _frame = null;
        }
        if (frame != null)
        {
            frame.Navigating -= Frame_OnNavigating;
            frame.Navigated -= Frame_OnNavigated;
        }
    }

    /// <summary>
    /// Go back to the previous page in the navigation stack.
    /// </summary>
    /// <returns></returns>
    public bool GoBack()
    {
        ArgumentNullException.ThrowIfNull(_frame, $"Frame is not registered in RegisterFrameEvents.");

        if (_frame.CanGoBack)
        {
            _frame.GoBack();
            _parameterStack.Pop();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Navigates to a specific page type with an optional parameter.
    /// </summary>
    /// <param name="pageTag"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    /// <remarks>
    /// Parameter type must have correct == & != operators defined for comparison.
    /// </remarks>
    public bool NavigateTo(SettingPageTag pageTag, object? parameter = null)
    {
        ArgumentNullException.ThrowIfNull(_frame, $"Frame is not registered in RegisterFrameEvents.");

        var pageType = _pageService.GetPageType(pageTag);
        if (_nextParameter.TryGetValue(pageType, out var value) && parameter == null)
        {
            parameter = value;
            _nextParameter.Remove(pageType);
        }

        if (_frame.Content?.GetType() != pageType || (parameter != null && parameter != _parameterStack.Peek()))
        {
            var navigated = _frame.Navigate(pageType,
                parameter: parameter, 
                infoOverride: App.Settings.EnableAnimationEffects ? null : // Default transition animation
                new SuppressNavigationTransitionInfo()); // Suppress animation if disabled);
            if (navigated)
            {
                _parameterStack.Push(parameter);
            }

            return navigated;
        }

        return false;
    }

    /// <summary>
    /// Sets the next parameter for a specific page key.
    /// </summary>
    /// <param name="pageKey"></param>
    /// <param name="parameter"></param>
    public void SetNextParameter(Type pageKey, object? parameter)
    {
        _nextParameter[pageKey] = parameter;
    }

    #region Events

    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var selectedItem = args.SelectedItemContainer;
        if (selectedItem?.Tag is SettingPageTag tag)
        {
            NavigateTo(tag, null);
        }
    }

    private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        GoBack();
    }

    private void Frame_OnNavigating(object sender, NavigatingCancelEventArgs e)
    {
        // Update the back button state
        _navigationView!.IsBackEnabled = _frame!.CanGoBack;

        if (sender is not Frame frame) return;
        if (GetPageViewModel(frame) is INavigationAware navigationAware)
        {
            navigationAware.OnNavigatedFrom();
        }
    }

    private void Frame_OnNavigated(object sender, NavigationEventArgs e)
    {
        // Update the back button state
        _navigationView!.IsBackEnabled = _frame!.CanGoBack;

        if (sender is not Frame frame) return;
        if (GetPageViewModel(frame) is INavigationAware navigationAware)
        {
            navigationAware.OnNavigatedTo(_parameterStack.Peek());
        }

        // Update the selected NavigationViewItem based on the page type
        var currentTag = _pageService.GetPageTag(frame.SourcePageType);
        foreach (var item in _navigationView.MenuItems)
        {
            if (item is NavigationViewItem newItem &&
                newItem.Tag is SettingPageTag tag &&
                tag == currentTag &&
                _navigationView.SelectedItem != newItem)
            {
                _navigationView.SelectedItem = newItem;
            }
        }
    }

    private static object? GetPageViewModel(Frame frame) => (frame?.Content as Page)?.DataContext as INavigationAware;

    #endregion
}
