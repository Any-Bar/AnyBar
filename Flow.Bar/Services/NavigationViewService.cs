using Flow.Bar.Controls;
using Flow.Bar.Interfaces.Navigation;
using Flow.Bar.Models.Enums;
using iNKORE.UI.WPF.Modern.Media.Animation;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Navigation;
using Frame = iNKORE.UI.WPF.Modern.Controls.Frame;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace Flow.Bar.Services;

public class NavigationViewService(PageService pageService)
{
    private readonly PageService _pageService = pageService;

    private NavigationView? _navigationView;
    private ScrollViewer? _scrollViewer;
    private Frame? _frame;

    private Tuple<SettingPageTag, object?>? _nextNavigation;

    // Due to the limitation of WPF & iNKORE.UI.WPF.Modern framewoork.
    // we cannot use a stack for the parameters so that we can go back to the previous parameter.
    private readonly Stack<object?> _parameterStack = [];

    public NavigationView? NavigationView => _navigationView;
    public ScrollViewer? ScrollViewer => _scrollViewer;
    public Frame? Frame => _frame;

    /// <summary>
    /// Registers the frame events for navigation.
    /// </summary>
    /// <param name="navigationView"></param>
    /// <param name="frame"></param>
    /// <param name="parameter"></param>
    /// <param name="navigate"></param>
    public void RegisterFrameEvents(NavigationView navigationView, ScrollViewer scrollViewer, Frame frame, object? parameter = null, bool navigate = true)
    {
        ArgumentNullException.ThrowIfNull(navigationView);
        ArgumentNullException.ThrowIfNull(frame);

        UnregisterFrameEvents(frame);
        _navigationView = navigationView;
        _navigationView.BackRequested += NavigationView_BackRequested;
        _navigationView.ItemInvoked += NavigationView_ItemInvoked;
        _scrollViewer = scrollViewer;
        _frame = frame;
        _frame.Navigating += Frame_OnNavigating;
        _frame.Navigated += Frame_OnNavigated;

        if (_nextNavigation != null)
        {
            var nextNavigationTag = _nextNavigation.Item1;
            var nextNavigationParameter = _nextNavigation.Item2;
            _nextNavigation = null;
            NavigateTo(nextNavigationTag, nextNavigationParameter);
        }
        else if (navigate && navigationView.SelectedItem is NavigationViewItem item && item.Tag is SettingPageTag tag)
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
            _scrollViewer = null;
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
            _frame.GoBack(
                transitionInfoOverride: App.Settings.EnableAnimationEffects ? null : // Default transition animation
                new SuppressNavigationTransitionInfo()); // Suppress animation if disabled);
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
    /// Trigger OnNavigatedTo to with an optional parameter,
    /// if the page is of the given tag and page view model implements INavigationAware.
    /// </summary>
    /// <param name="pageTag"></param>
    /// <param name="parameter"></param>
    public bool OnNavigateTo(SettingPageTag pageTag, object? parameter = null)
    {
        ArgumentNullException.ThrowIfNull(_frame, $"Frame is not registered in RegisterFrameEvents.");

        var pageType = _pageService.GetPageType(pageTag);
        if (_frame.Content?.GetType() == pageType && _frame.Content is Page page &&
            GetPageViewModel(page) is INavigationAware navigationAware)
        {
            navigationAware.OnNavigatedTo(parameter);
        }

        return false;
    }

    /// <summary>
    /// Set the next navigation after the frame is registered.
    /// </summary>
    /// <remarks>
    /// This is useful for navigating to a specific page after the frame is registered.
    /// </remarks>
    /// <param name="tag"></param>
    /// <param name="parameter"></param>
    public void SetNextNavigation(SettingPageTag pageTag, object? parameter)
    {
        _nextNavigation = new Tuple<SettingPageTag, object?>(pageTag, parameter);
    }

    #region Events

    private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var selectedItem = args.InvokedItemContainer;
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
        _navigationView!.IsBackEnabled = _frame!.CanGoBack;
        if (sender is not Frame frame) return;
        if (frame.Content is not Page page) return;
        if (GetPageViewModel(page) is INavigationAware navigationAware)
        {
            navigationAware.OnNavigatedFrom();
        }
    }

    private void Frame_OnNavigated(object sender, NavigationEventArgs e)
    {
        _navigationView!.IsBackEnabled = _frame!.CanGoBack;
        if (sender is not Frame frame) return;
        if (frame.Content is not Page page) return;
        if (GetPageViewModel(page) is INavigationAware navigationAware)
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

        // Update the contained NavigationViewItem based on the page type
        var containedTag = _pageService.GetContainedPageTag(currentTag);
        if (containedTag.HasValue)
        {
            foreach (var item in _navigationView.MenuItems)
            {
                if (item is NavigationViewItem newItem &&
                    newItem.Tag is SettingPageTag tag &&
                    tag == containedTag.Value &&
                    _navigationView.SelectedItem != newItem)
                {
                    // Temporarily remove and reattach the event handler to prevent navigation
                    _navigationView.ItemInvoked -= NavigationView_ItemInvoked;
                    _navigationView.SelectedItem = newItem;
                    _navigationView.ItemInvoked += NavigationView_ItemInvoked;
                }
            }
        }

        // TODO: Use DynamicResource for Binding
        // Update the header of the NavigationView
        _navigationView.Header = page.Title;
    }

    private static object? GetPageViewModel(Page page) => page.DataContext as INavigationAware;

    #endregion
}
