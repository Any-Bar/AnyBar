using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Navigation;
using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Bar.Controls;
using Flow.Bar.Enums;
using Flow.Bar.Interfaces;
using Frame = iNKORE.UI.WPF.Modern.Controls.Frame;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace Flow.Bar.Services;

public class NavigationViewService(PageService pageService)
{
    private readonly PageService _pageService = pageService;

    private NavigationView? _navigationView;
    private Frame? _frame;

    private Tuple<SettingPageTag, object?>? _nextNavigation;

    // Use a stack for the parameters so that we can go back to the previous parameter.
    private readonly Stack<object?> _parameterStack = [];

    public NavigationView? NavigationView => _navigationView;
    public Frame? Frame => _frame;

    /// <summary>
    /// Registers the frame events for navigation.
    /// </summary>
    /// <param name="navigationView"></param>
    /// <param name="frame"></param>
    /// <param name="parameter"></param>
    /// <param name="navigate"></param>
    public void RegisterFrameEvents(NavigationView navigationView, Frame frame, object? parameter = null, bool navigate = true)
    {
        ArgumentNullException.ThrowIfNull(navigationView);
        ArgumentNullException.ThrowIfNull(frame);

        UnregisterFrameEvents(frame);
        _navigationView = navigationView;
        _navigationView.BackRequested += NavigationView_BackRequested;
        _navigationView.ItemInvoked += NavigationView_ItemInvoked;
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
        ArgumentNullException.ThrowIfNull(_frame, $"{nameof(Frame)} is not registered in {nameof(RegisterFrameEvents)}");

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
        ArgumentNullException.ThrowIfNull(_frame, $"{nameof(Frame)} is not registered in {nameof(RegisterFrameEvents)}");

        var pageType = _pageService.GetPageType(pageTag);
        if (_frame.Content?.GetType() != pageType || (parameter != null && parameter != _parameterStack.Peek()))
        {
            var navigated = _frame.Navigate(pageType, parameter: parameter);
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
        if (_frame == null) return false;

        var pageType = _pageService.GetPageType(pageTag);
        if (_frame.Content?.GetType() == pageType && _frame.Content is Page page)
        {
            OnNavigateTo(NavigationView, page, parameter);
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
        OnNavigateTo(NavigationView, page, _parameterStack.Peek());

        // Update the selected NavigationViewItem based on the page type
        var currentTag = _pageService.GetPageTag(frame.SourcePageType);
        foreach (var item in _navigationView.MenuItems.OfType<NavigationViewItem>())
        {
            if (item.Tag is SettingPageTag tag &&
                tag == currentTag &&
                _navigationView.SelectedItem != item)
            {
                _navigationView.SelectedItem = item;
            }
        }

        // Update the contained NavigationViewItem based on the page type
        var containedTag = _pageService.GetContainedPageTag(currentTag);
        if (containedTag.HasValue)
        {
            foreach (var item in _navigationView.MenuItems.OfType<NavigationViewItem>())
            {
                if (item.Tag is SettingPageTag tag &&
                    tag == containedTag.Value &&
                    _navigationView.SelectedItem != item)
                {
                    // Temporarily remove and reattach the event handler to prevent navigation
                    _navigationView.ItemInvoked -= NavigationView_ItemInvoked;
                    _navigationView.SelectedItem = item;
                    _navigationView.ItemInvoked += NavigationView_ItemInvoked;
                }
            }
        }
    }

    private static object? GetPageViewModel(Page page) => page.DataContext as ObservableObject;

    #endregion

    #region Header

    private static void OnNavigateTo(NavigationView? navigationView, Page page, object? parameter)
    {
        var viewModel = GetPageViewModel(page);
        if (viewModel is INavigationAware navigationAware)
        {
            navigationAware.OnNavigatedTo(parameter);
        }

        if (navigationView != null)
        {
            if (viewModel is INavigationHeader navigationHeader)
            {
                var headerKey = navigationHeader.GetHeaderKey();
                if (!string.IsNullOrWhiteSpace(headerKey))
                {
                    SetHeaderReference(navigationView, headerKey);
                }
                else
                {
                    SetHeaderValue(navigationView, navigationHeader.GetHeaderValue());
                }
            }
            else
            {
                SetHeaderValue(navigationView, string.Empty);
            }
        }
    }

    private static void SetHeaderReference(NavigationView navigationView, string headerKey)
    {
        navigationView.SetResourceReference(NavigationView.HeaderProperty, headerKey);
    }

    private static void SetHeaderValue(NavigationView navigationView, string headerValue)
    {
        navigationView.Header = headerValue;
    }

    #endregion
}
