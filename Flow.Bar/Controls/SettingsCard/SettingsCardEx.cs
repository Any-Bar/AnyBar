// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using iNKORE.UI.WPF.Modern.Controls.Helpers;

namespace Flow.Bar.Controls;

/// <summary>
/// This is the base control to create consistent settings experiences, inline with the Windows 11 design language.
/// A SettingsCardEx can also be hosted within a SettingsExpanderEx.
/// </summary>
public partial class SettingsCardEx : ButtonBase
{
    internal const string CommonStates = "CommonStates";
    internal const string NormalState = "Normal";
    internal const string MouseOverState = "PointerOver";
    internal const string PressedState = "Pressed";
    internal const string DisabledState = "Disabled";

    internal const string ContentAlignmentStates = "ContentAlignmentStates";
    internal const string RightState = "Right";
    internal const string RightWrappedState = "RightWrapped";
    internal const string RightWrappedNoIconState = "RightWrappedNoIcon";
    internal const string LeftState = "Left";
    internal const string VerticalState = "Vertical";

    internal const string ContentSpacingStates = "ContentSpacingStates";
    internal const string NoContentSpacingState = "NoContentSpacing";
    internal const string ContentSpacingState = "ContentSpacing";

    internal const string ActionIconPresenterHolder = "PART_ActionIconPresenterHolder";
    internal const string HeaderPresenter = "PART_HeaderPresenter";
    internal const string DescriptionPresenter = "PART_DescriptionPresenter";
    internal const string HeaderIconPresenterHolder = "PART_HeaderIconPresenterHolder";

    static SettingsCardEx()
    {
        ContentProperty.OverrideMetadata(typeof(SettingsCardEx), new FrameworkPropertyMetadata(null, ContentProperty_ValueChanged));
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SettingsCardEx), new FrameworkPropertyMetadata(typeof(SettingsCardEx)));
    }

    internal static readonly DependencyPropertyDescriptor IsPressedPropertyDescriptior = DependencyPropertyDescriptor.FromProperty(IsPressedProperty, typeof(SettingsCardEx));
    internal static readonly DependencyPropertyDescriptor IsMouseOverPropertyDescriptior = DependencyPropertyDescriptor.FromProperty(IsMouseOverProperty, typeof(SettingsCardEx));

    /// <summary>
    /// Creates a new instance of the <see cref="SettingsCardEx"/> class.
    /// </summary>
    public SettingsCardEx()
    {
        IsPressedPropertyDescriptior.AddValueChanged(this, PointerStateProperties_ValueChanged);
        IsMouseOverPropertyDescriptior.AddValueChanged(this, PointerStateProperties_ValueChanged);
    }

    private void PointerStateProperties_ValueChanged(object? sender, EventArgs e)
    {
        UpdatePointerState();
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        IsEnabledChanged -= OnIsEnabledChanged;
        OnActionIconChanged();
        OnHeaderChanged();
        OnHeaderIconChanged();
        OnDescriptionChanged();
        OnIsClickEnabledChanged();
        CheckInitialVisualState(false);
        SetAccessibleContentName();

        IsEnabledChanged += OnIsEnabledChanged;
        SizeChanged += SettingsCard_SizeChanged;
    }

    private void SettingsCard_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateContentAlignmentState();
    }

    private static void ContentProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SettingsCardEx control)
        {
            control.OnContentChanged(e.OldValue, e.NewValue);
            control.UpdateContentVisibilityStates();
        }
    }

    private void CheckInitialVisualState(bool useTransitions = true)
    {
        VisualStateManager.GoToState(this, IsEnabled ? NormalState : DisabledState, App.Settings.EnableAnimationEffects && useTransitions);

        if (GetTemplateChild("ContentAlignmentStates") is VisualStateGroup contentAlignmentStatesGroup)
        {
            contentAlignmentStatesGroup.CurrentStateChanged -= ContentAlignmentStates_Changed;
            CheckVerticalSpacingState(contentAlignmentStatesGroup.CurrentState);
            contentAlignmentStatesGroup.CurrentStateChanged += ContentAlignmentStates_Changed;
        }
    }

    // We automatically set the AutomationProperties.Name of the Content if not configured.
    private void SetAccessibleContentName()
    {
        if (Header is string headerString && headerString != string.Empty)
        {
            // We don't want to override an AutomationProperties.Name that is manually set, or if the Content basetype is of type ButtonBase (the ButtonBase.Content will be used then)
            if (Content is UIElement element && string.IsNullOrEmpty(AutomationProperties.GetName(element)) && element.GetType().BaseType != typeof(ButtonBase) && element.GetType() != typeof(TextBlock))
            {
                AutomationProperties.SetName(element, headerString);
            }
        }
    }

    private void EnableButtonInteraction()
    {
        DisableButtonInteraction();

        IsTabStop = true;
    }

    private void DisableButtonInteraction()
    {
        IsTabStop = false;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (!IsClickEnabled)
            e.Handled = false;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (!IsClickEnabled)
            e.Handled = false;
    }

    private void UpdatePointerState()
    {
        var state = NormalState;

        if (IsEnabled == false)
        {
            state = DisabledState;
        }
        else if (IsClickEnabled == false)
        {
            state = NormalState;
        }    
        else
        {
            if (IsPressed)
            {
                state = PressedState;
            }
            else if (IsMouseOver)
            {
                state = MouseOverState;
            }
        }

        VisualStateManager.GoToState(this, state, App.Settings.EnableAnimationEffects);
    }

    private void UpdateContentAlignmentState()
    {
        // Manually go to states, adapted from:
        // https://github.com/CommunityToolkit/Windows/blob/main/components/SettingsControls/src/SettingsCard/SettingsCard.xaml#L304-353

        string? state = null;

        if (ContentAlignment == ContentAlignment.Left)
        {
            state = LeftState;
        }
        else if (ContentAlignment == ContentAlignment.Vertical)
        {
            state = VerticalState;
        }
        else
        {
            var SettingsCardWrapNoIconThreshold = FindResource("SettingsCardWrapNoIconThreshold") as double?;
            var SettingsCardWrapThreshold = FindResource("SettingsCardWrapThreshold") as double?;

            if (SettingsCardWrapThreshold != null && SettingsCardWrapThreshold != null)
            {
                if (ActualWidth < SettingsCardWrapNoIconThreshold)
                {
                    state = RightWrappedNoIconState;
                }
                else if (ActualWidth < SettingsCardWrapThreshold)
                {
                    state = RightWrappedState;
                }
                else
                {
                    state = RightState;
                }
            }
        }

        if (state != null)
        {
            VisualStateManager.GoToState(this, state, App.Settings.EnableAnimationEffects);
        }
    }

    public void UpdateContentVisibilityStates()
    {
        // Manually go to states, adapted from:
        // https://github.com/CommunityToolkit/Windows/blob/main/components/SettingsControls/src/SettingsCard/SettingsCard.xaml#L369
        
        var state = Content == null || Content as string == ""
            ? nameof(Visibility.Collapsed)
            : nameof(Visibility.Visible);

        VisualStateManager.GoToState(this, state, App.Settings.EnableAnimationEffects);
    }

    /// <summary>
    /// Creates AutomationPeer
    /// </summary>
    /// <returns>An automation peer for <see cref="SettingsCardEx"/>.</returns>
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new SettingsCardAutomationPeer(this);
    }

    private void OnIsClickEnabledChanged()
    {
        OnActionIconChanged();
        if (IsClickEnabled)
        {
            EnableButtonInteraction();
        }
        else
        {
            DisableButtonInteraction();
        }
    }

    private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        VisualStateManager.GoToState(this, IsEnabled ? NormalState : DisabledState, App.Settings.EnableAnimationEffects);
    }

    private void OnActionIconChanged()
    {
        if (GetTemplateChild(ActionIconPresenterHolder) is FrameworkElement actionIconPresenter)
        {
            if (IsClickEnabled && IsActionIconVisible)
            {
                actionIconPresenter.Visibility = Visibility.Visible;
            }
            else
            {
                actionIconPresenter.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void OnHeaderIconChanged()
    {
        if (GetTemplateChild(HeaderIconPresenterHolder) is FrameworkElement headerIconPresenter)
        {
            headerIconPresenter.Visibility = HeaderIcon != null
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }

    private void OnDescriptionChanged()
    {
        if (GetTemplateChild(DescriptionPresenter) is FrameworkElement descriptionPresenter)
        {
            descriptionPresenter.Visibility = IsNullOrEmptyString(Description)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }

    private void OnHeaderChanged()
    {
        if (GetTemplateChild(HeaderPresenter) is FrameworkElement headerPresenter)
        {
            headerPresenter.Visibility = IsNullOrEmptyString(Header)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }

    private void ContentAlignmentStates_Changed(object? sender, VisualStateChangedEventArgs e)
    {
        CheckVerticalSpacingState(e.NewState);
    }

    private void CheckVerticalSpacingState(VisualState s)
    {
        // On state change, checking if the Content should be wrapped (e.g. when the card is made smaller or the ContentAlignment is set to Vertical). If the Content and the Header or Description are not null, we add spacing between the Content and the Header/Description.

        if (s != null && (s.Name == RightWrappedState || s.Name == RightWrappedNoIconState || s.Name == VerticalState) && (Content != null) && (!IsNullOrEmptyString(Header) || !IsNullOrEmptyString(Description)))
        {
            VisualStateManager.GoToState(this, ContentSpacingState, App.Settings.EnableAnimationEffects);
        }
        else
        {
            VisualStateManager.GoToState(this, NoContentSpacingState, App.Settings.EnableAnimationEffects);
        }
    }

    private static bool IsNullOrEmptyString(object obj)
    {
        if (obj == null)
        {
            return true;
        }

        if (obj is string objString && objString == string.Empty)
        {
            return true;
        }

        return false;
    }

    public static readonly DependencyProperty UseSystemFocusVisualsProperty =
        FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(SettingsCardEx));

    public bool UseSystemFocusVisuals
    {
        get => (bool)GetValue(UseSystemFocusVisualsProperty);
        set => SetValue(UseSystemFocusVisualsProperty, value);
    }

    public static readonly DependencyProperty FocusVisualMarginProperty =
        FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(SettingsCardEx));

    public Thickness FocusVisualMargin
    {
        get => (Thickness)GetValue(FocusVisualMarginProperty);
        set => SetValue(FocusVisualMarginProperty, value);
    }
}
