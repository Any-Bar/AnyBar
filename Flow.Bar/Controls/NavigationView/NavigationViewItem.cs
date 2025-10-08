// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using iNKORE.UI.WPF.Modern.Controls;
using static Flow.Bar.Controls.CppWinRTHelpers;
using PointerRoutedEventArgs = System.Windows.Input.MouseEventArgs;

namespace Flow.Bar.Controls;

#pragma warning disable IDE0060 // Remove unused parameter

public partial class NavigationViewItem : NavigationViewItemBase
{
    private const string C_navigationViewItemPresenterName = "NavigationViewItemPresenter";

    // Visual States
    private const string C_pressedSelected = "PressedSelected";
    private const string C_pointerOverSelected = "PointerOverSelected";
    private const string C_selected = "Selected";
    private const string C_pressed = "Pressed";
    private const string C_pointerOver = "PointerOver";
    private const string C_disabled = "Disabled";
    private const string C_enabled = "Enabled";
    private const string C_normal = "Normal";

    static NavigationViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NavigationViewItem),
            new FrameworkPropertyMetadata(typeof(NavigationViewItem)));
    }

    internal void UpdateVisualStateNoTransition()
    {
        UpdateVisualState(false);
    }

    private protected override void OnNavigationViewItemBaseDepthChanged()
    {
        UpdateItemIndentation();
    }

    private protected override void OnNavigationViewItemBaseIsSelectedChanged()
    {
        UpdateVisualStateForPointer();
    }

    private protected override void OnNavigationViewItemBasePositionChanged()
    {
        UpdateVisualStateNoTransition();
    }

    public override void OnApplyTemplate()
    {
        // Stop UpdateVisualState before template is applied. Otherwise the visuals may be unexpected
        m_appliedTemplate = false;

        UnhookEventsAndClearFields();

        base.OnApplyTemplate();

        // Find selection indicator
        // Retrieve pointers to stable controls 
        IControlProtected controlProtected = this;
        m_helper.Init(controlProtected);

        HookInputEvents(controlProtected);

        IsEnabledChanged += OnIsEnabledChanged;

        m_toolTip = GetTemplateChildT<ToolTip>("ToolTip", controlProtected);

        if (GetSplitView() is { } splitView)
        {
            m_splitViewIsPaneOpenChangedRevoker = new(splitView, OnSplitViewIsPaneOpenChanged);
            m_splitViewDisplayModeChangedRevoker = new(splitView, OnSplitViewDisplayModeChanged);
            m_splitViewCompactPaneLengthChangedRevoker = new(splitView, OnSplitViewCompactPaneLengthChanged);

            UpdateCompactPaneLength();
            UpdateIsClosedCompact();
        }

        m_appliedTemplate = true;

        UpdateItemIndentation();
        UpdateVisualStateNoTransition();
    }

    internal UIElement? GetSelectionIndicator()
    {
        var selectIndicator = m_helper.GetSelectionIndicator();
        if (GetPresenter() is { } presenter)
        {
            selectIndicator = presenter.GetSelectionIndicator();
        }
        return selectIndicator;
    }

    private void OnSplitViewIsPaneOpenChanged(object? sender, EventArgs e)
    {
        UpdateCompactPaneLength();
    }

    private void OnSplitViewDisplayModeChanged(object? sender, EventArgs e)
    {
        UpdateIsClosedCompact();
    }

    private void OnSplitViewCompactPaneLengthChanged(object? sender, EventArgs e)
    {
        UpdateCompactPaneLength();
    }

    private void UpdateCompactPaneLength()
    {
        if (GetSplitView() is { } splitView)
        {
            SetValue(s_compactPaneLengthPropertyKey, splitView.CompactPaneLength);

            // Only update when on left
            if (GetPresenter() is { } presenter)
            {
                presenter.UpdateCompactPaneLength(splitView.CompactPaneLength, true);
            }
        }
    }

    internal void UpdateIsClosedCompact()
    {
        if (GetSplitView() is { } splitView)
        {
            // Check if the pane is closed and if the splitview is in either compact mode.
            m_isClosedCompact = !splitView.IsPaneOpen
                && (splitView.DisplayMode == SplitViewDisplayMode.CompactOverlay || splitView.DisplayMode == SplitViewDisplayMode.CompactInline);

            UpdateVisualState();
        }
    }

    private void UpdateNavigationViewItemToolTip()
    {
        var toolTipContent = ToolTipService.GetToolTip(this);

        // no custom tooltip, then use suggested tooltip
        if (toolTipContent == null || toolTipContent == m_suggestedToolTipContent)
        {
            if (ShouldEnableToolTip())
            {
                ToolTipService.SetToolTip(this, m_suggestedToolTipContent);
            }
            else
            {
                ToolTipService.SetToolTip(this, null);
            }
        }
    }

    private void SuggestedToolTipChanged(object newContent)
    {
        var potentialString = newContent;
        var stringableToolTip = (potentialString != null && potentialString is string);

        object? newToolTipContent = null;
        if (stringableToolTip)
        {
            newToolTipContent = newContent;
        }

        // Both customer and NavigationViewItem can update ToolTipContent by ToolTipService.SetToolTip or XAML
        // If the ToolTipContent is not the same as m_suggestedToolTipContent, then it's set by customer.
        // Customer's ToolTip take high priority, and we never override Customer's ToolTip.
        var toolTipContent = ToolTipService.GetToolTip(this);
        if (m_suggestedToolTipContent is { } oldToolTipContent)
        {
            if (oldToolTipContent == toolTipContent)
            {
                ToolTipService.SetToolTip(this, null);
            }
        }

        m_suggestedToolTipContent = newToolTipContent;
    }

    private void OnIconPropertyChanged(DependencyPropertyChangedEventArgs args)
    {
        UpdateVisualStateNoTransition();
    }

    private void UpdateVisualStateForIconAndContent(bool showIcon, bool showContent)
    {
        if (m_navigationViewItemPresenter is { } presenter)
        {
            var stateName = showIcon ? (showContent ? "IconOnLeft" : "IconOnly") : "ContentOnly";
            VisualStateManager.GoToState(presenter, stateName, false);
        }
    }

    private void UpdateVisualStateForKeyboardFocusedState()
    {
        var focusState = "KeyboardNormal";
        if (m_hasKeyboardFocus)
        {
            focusState = "KeyboardFocused";
        }

        VisualStateManager.GoToState(this, focusState, false);
    }

    private void UpdateVisualStateForToolTip()
    {
        // Since RS5, ToolTip apply to NavigationViewItem directly to make Keyboard focus has tooltip too.
        // If ToolTip TemplatePart is detected, fallback to old logic and apply ToolTip on TemplatePart.
        if (m_toolTip is { } toolTip)
        {
            var shouldEnableToolTip = ShouldEnableToolTip();
            var toolTipContent = m_suggestedToolTipContent;
            if (shouldEnableToolTip && toolTipContent != null)
            {
                toolTip.Content = toolTipContent;
                toolTip.IsEnabled = true;
            }
            else
            {
                toolTip.Content = null;
                toolTip.IsEnabled = false;
            }
        }
        else
        {
            UpdateNavigationViewItemToolTip();
        }
    }

    private void UpdateVisualStateForPointer()
    {
        // DisabledStates and CommonStates
        var enabledStateValue = C_enabled;
        var isSelected = IsSelected;
        var selectedStateValue = C_normal;
        if (IsEnabled)
        {
            if (isSelected)
            {
                if (m_isPressed)
                {
                    selectedStateValue = C_pressedSelected;
                }
                else if (m_isPointerOver)
                {
                    selectedStateValue = C_pointerOverSelected;
                }
                else
                {
                    selectedStateValue = C_selected;
                }
            }
            else if (m_isPointerOver)
            {
                if (m_isPressed)
                {
                    selectedStateValue = C_pressed;
                }
                else
                {
                    selectedStateValue = C_pointerOver;
                }
            }
            else if (m_isPressed)
            {
                selectedStateValue = C_pressed;
            }
        }
        else
        {
            enabledStateValue = C_disabled;
            if (isSelected)
            {
                selectedStateValue = C_selected;
            }
        }

        // There are scenarios where the presenter may not exist.
        // For example, the top nav settings item. In that case,
        // update the states for the item itself.
        if (m_navigationViewItemPresenter is { } presenter)
        {
            VisualStateManager.GoToState(presenter, enabledStateValue, true);
            VisualStateManager.GoToState(presenter, selectedStateValue, true);
        }
        else
        {
            VisualStateManager.GoToState(this, enabledStateValue, true);
            VisualStateManager.GoToState(this, selectedStateValue, true);
        }
    }

    private void UpdateVisualState(bool useTransitions = true)
    {
        if (!m_appliedTemplate)
            return;

        UpdateVisualStateForPointer();

        var shouldShowIcon = ShouldShowIcon();
        var shouldShowContent = ShouldShowContent();

        if (m_navigationViewItemPresenter is { } presenter)
        {
            // Backward Compatibility with RS4-, new implementation prefer IconOnLeft/IconOnly/ContentOnly
            VisualStateManager.GoToState(presenter, shouldShowIcon ? "IconVisible" : "IconCollapsed", useTransitions);
        }

        UpdateVisualStateForToolTip();

        UpdateVisualStateForIconAndContent(shouldShowIcon, shouldShowContent);

        // visual state for focus state. top navigation use it to provide different visual for selected and selected+focused
        UpdateVisualStateForKeyboardFocusedState();
    }

    private bool ShouldShowIcon()
    {
        return Icon != null;
    }

    private bool ShouldEnableToolTip()
    {
        // We may enable Tooltip for IconOnly in the future, but not now
        return m_isClosedCompact;
    }

    private bool ShouldShowContent()
    {
        return Content != null;
    }

    private UIElement GetPresenterOrItem()
    {
        if (m_navigationViewItemPresenter is { } presenter)
        {
            return presenter;
        }
        else
        {
            return this;
        }
    }

    private NavigationViewItemPresenter? GetPresenter()
    {
        NavigationViewItemPresenter? presenter = null;
        if (m_navigationViewItemPresenter != null)
        {
            presenter = m_navigationViewItemPresenter;
        }
        return presenter;
    }

    private void UpdateItemIndentation()
    {
        // Update item indentation based on its depth
        if (m_navigationViewItemPresenter is { } presenter)
        {
            var newLeftMargin = Depth * C_itemIndentation;
            presenter.UpdateContentLeftIndentation(newLeftMargin);
        }
    }

    // IUIElement / IUIElementOverridesHelper
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new NavigationViewItemAutomationPeer(this);
    }

    // IContentControlOverrides / IContentControlOverridesHelper
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        SuggestedToolTipChanged(newContent);
        UpdateVisualStateNoTransition();
    }

    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);
        if (e.OriginalSource is Control originalSource)
        {
            // It's used to support bluebar have difference appearance between focused and focused+selection. 
            // For example, we can move the SelectionIndicator 3px up when focused and selected to make sure focus rectange doesn't override SelectionIndicator. 
            // If it's a pointer or programatic, no focus rectangle, so no action
            if (originalSource.IsKeyboardFocused && InputManager.Current.MostRecentInputDevice is KeyboardDevice)
            {
                m_hasKeyboardFocus = true;
                UpdateVisualStateNoTransition();
            }
        }
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        if (m_hasKeyboardFocus)
        {
            m_hasKeyboardFocus = false;
            UpdateVisualStateNoTransition();
        }
    }

    private void OnPresenterPointerPressed(object sender, PointerRoutedEventArgs args)
    {
        m_isPressed = args.LeftButton == MouseButtonState.Pressed || args.RightButton == MouseButtonState.Pressed;

        var presenter = GetPresenterOrItem();

        Debug.Assert(presenter != null);

        if (presenter.CaptureMouse())
        {
            m_isMouseCaptured = true;
        }

        UpdateVisualState();
    }

    private void OnPresenterPointerReleased(object sender, PointerRoutedEventArgs args)
    {
        if (m_isPressed)
        {
            m_isPressed = false;

            if (m_isMouseCaptured)
            {
                var presenter = GetPresenterOrItem();

                Debug.Assert(presenter != null);

                presenter.ReleaseMouseCapture();
            }
        }

        UpdateVisualState();
    }

    private void OnPresenterPointerEntered(object sender, PointerRoutedEventArgs args)
    {
        ProcessPointerOver(args);
    }

    private void OnPresenterPointerMoved(object sender, PointerRoutedEventArgs args)
    {
        ProcessPointerOver(args);
    }

    private void OnPresenterPointerExited(object sender, PointerRoutedEventArgs args)
    {
        m_isPointerOver = false;
        UpdateVisualState();
    }

    private void OnPresenterPointerCanceled(object sender, PointerRoutedEventArgs args)
    {
        ProcessPointerCanceled(args);
    }

    private void OnPresenterPointerCaptureLost(object sender, PointerRoutedEventArgs args)
    {
        ProcessPointerCanceled(args);
    }

    private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs args)
    {
        if (!IsEnabled)
        {
            m_isPressed = false;
            m_isPointerOver = false;

            if (m_isMouseCaptured)
            {
                var presenter = GetPresenterOrItem();

                Debug.Assert(presenter != null);

                presenter.ReleaseMouseCapture();
                m_isMouseCaptured = false;
            }
        }

        UpdateVisualState();
    }

    private void ProcessPointerCanceled(PointerRoutedEventArgs args)
    {
        m_isPressed = false;
        m_isPointerOver = false;
        m_isMouseCaptured = false;
        UpdateVisualState();
    }

    private void ProcessPointerOver(PointerRoutedEventArgs args)
    {
        if (!m_isPointerOver)
        {
            m_isPointerOver = true;
            UpdateVisualState();
        }
    }

    private void HookInputEvents(IControlProtected controlProtected)
    {
        UIElement presenter;
        {
            presenter = init();
            UIElement init()
            {
                if (GetTemplateChildT<NavigationViewItemPresenter>(C_navigationViewItemPresenterName, controlProtected) is { } presenter)
                {
                    m_navigationViewItemPresenter = presenter;
                    return presenter;
                }
                // We don't have a presenter, so we are our own presenter.
                return this;
            }
        }

        Debug.Assert(presenter != null);

        // Handlers that set flags are skipped when args.Handled is already True.
        presenter.MouseDown += OnPresenterPointerPressed;
        presenter.MouseEnter += OnPresenterPointerEntered;
        presenter.MouseMove += OnPresenterPointerMoved;

        // Handlers that reset flags are not skipped when args.Handled is already True to avoid broken states.
        presenter.AddHandler(MouseUpEvent, new MouseButtonEventHandler(OnPresenterPointerReleased), true /*handledEventsToo*/);
        presenter.AddHandler(MouseLeaveEvent, new MouseEventHandler(OnPresenterPointerExited), true /*handledEventsToo*/);
        presenter.AddHandler(LostMouseCaptureEvent, new MouseEventHandler(OnPresenterPointerCaptureLost), true /*handledEventsToo*/);
    }

    private void UnhookInputEvents()
    {
        var presenter = m_navigationViewItemPresenter as UIElement ?? this;
        presenter.MouseDown -= OnPresenterPointerPressed;
        presenter.MouseEnter -= OnPresenterPointerEntered;
        presenter.MouseMove -= OnPresenterPointerMoved;
        presenter.RemoveHandler(MouseUpEvent, new MouseButtonEventHandler(OnPresenterPointerReleased));
        presenter.RemoveHandler(MouseLeaveEvent, new MouseEventHandler(OnPresenterPointerExited));
        presenter.RemoveHandler(LostMouseCaptureEvent, new MouseEventHandler(OnPresenterPointerCaptureLost));
    }

    private void UnhookEventsAndClearFields()
    {
        UnhookInputEvents();

        m_splitViewIsPaneOpenChangedRevoker?.Revoke();
        m_splitViewDisplayModeChangedRevoker?.Revoke();
        m_splitViewCompactPaneLengthChangedRevoker?.Revoke();
        IsEnabledChanged -= OnIsEnabledChanged;

        m_navigationViewItemPresenter = null;
        m_toolTip = null;
    }

    private SplitViewIsPaneOpenChangedRevoker? m_splitViewIsPaneOpenChangedRevoker;
    private SplitViewDisplayModeChangedRevoker? m_splitViewDisplayModeChangedRevoker;
    private SplitViewCompactPaneLengthChangedRevoker? m_splitViewCompactPaneLengthChangedRevoker;

    private ToolTip? m_toolTip;
    private readonly NavigationViewItemHelper<NavigationViewItem> m_helper = new();
    private NavigationViewItemPresenter? m_navigationViewItemPresenter;
    private object? m_suggestedToolTipContent;

    private bool m_isClosedCompact = false;

    private bool m_appliedTemplate = false;
    private bool m_hasKeyboardFocus = false;

    // Visual state tracking
    private bool m_isMouseCaptured = false;
    private bool m_isPressed = false;
    private bool m_isPointerOver = false;
}
