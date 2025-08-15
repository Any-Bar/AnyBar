using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Flow.Bar.Plugin;
using iNKORE.UI.WPF.Helpers;
using ControlHelper = iNKORE.UI.WPF.Modern.Controls.Helpers.ControlHelper;

namespace Flow.Bar.Controls;

public class MenuFlyoutExPresenter : ContextMenu
{
    static MenuFlyoutExPresenter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuFlyoutExPresenter), new FrameworkPropertyMetadata(typeof(MenuFlyoutExPresenter)));

        IsOpenProperty.OverrideMetadata(typeof(MenuFlyoutExPresenter), new FrameworkPropertyMetadata(OnIsOpenChanged));
    }

    public MenuFlyoutExPresenter()
    {
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        OnApplyTemplateAction?.Invoke(this);
    }

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty =
        ControlHelper.CornerRadiusProperty.AddOwner(typeof(MenuFlyoutExPresenter));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    #region IsDefaultShadowEnabled

    public static readonly DependencyProperty IsDefaultShadowEnabledProperty =
        DependencyProperty.Register(
            nameof(IsDefaultShadowEnabled),
            typeof(bool),
            typeof(MenuFlyoutExPresenter),
            new PropertyMetadata(true));

    public bool IsDefaultShadowEnabled
    {
        get => (bool)GetValue(IsDefaultShadowEnabledProperty);
        set => SetValue(IsDefaultShadowEnabledProperty, value);
    }

    #endregion

    internal Action<ContextMenu>? OnApplyTemplateAction { get; set; } = null;

    internal ContextMenuPopupMode PopupMode { get; set; } = ContextMenuPopupMode.AlwaysPopup;

    internal event EventHandler<DependencyPropertyChangedEventArgs>? IsOpenChanged;

    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);

        if (_parentPopup == null)
        {
            HookupParentPopup();
        }
    }

    internal void SetOwningFlyout(MenuFlyoutEx owningFlyout)
    {
        m_owningFlyout = new WeakReference<MenuFlyoutEx>(owningFlyout);
    }

    internal void UpdatePopupAnimation()
    {
        if (_parentPopup != null && m_owningFlyout!.TryGetTarget(out var _))
        {
            _parentPopup.Resources.Remove(SystemParameters.MenuPopupAnimationKey);
        }
    }

    internal void CancelAsyncShowOrHide()
    {
        if (m_asyncShow != null)
        {
            m_asyncShow.Abort();
            m_asyncShow = null;
        }
        if (m_asyncHide != null)
        {
            m_asyncHide.Abort();
            m_asyncHide = null;
        }
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((MenuFlyoutExPresenter)d).OnIsOpenChanged(e);
    }

    private void OnIsOpenChanged(DependencyPropertyChangedEventArgs e)
    {
        IsOpenChanged?.Invoke(this, e);

        if ((bool)e.NewValue)
        {
            if (_parentPopup == null)
            {
                HookupParentPopup();
            }

            if (App.Settings.EnableAnimationEffects)
            {
                m_asyncShow = Dispatcher.BeginInvoke(DispatcherPriority.Loaded, ApplyOpenAnimation);
            }
        }
        else
        {
            if (PopupMode == ContextMenuPopupMode.PopupAndFadeAway && App.Settings.EnableAnimationEffects)
            {
                m_asyncHide = Dispatcher.BeginInvoke(DispatcherPriority.Loaded, ApplyCloseAnimation);
            }
        }
    }

    private void HookupParentPopup()
    {
        _parentPopup = Parent as Popup;

        if (_parentPopup != null)
        {
            _parentPopup.PreviewMouseLeftButtonDown += HandlePopupMouseButtonEvent;
            _parentPopup.PreviewMouseRightButtonDown += HandlePopupMouseButtonEvent;
            _parentPopup.PreviewMouseLeftButtonUp += HandlePopupMouseButtonEvent;
            _parentPopup.PreviewMouseRightButtonUp += HandlePopupMouseButtonEvent;

            UpdatePopupAnimation();
        }
    }

    private void ApplyOpenAnimation()
    {
        if (this.GetTemplateChild<Decorator>("Shdw") is { } chorme)
        {
            // Ensure RenderTransform is a TranslateTransform
            if (chorme.RenderTransform is TranslateTransform translateTransform)
            {
                translateTransform.X = translateTransform.Y = 0;
            }
            else
            {
                translateTransform = new TranslateTransform();
                chorme.RenderTransform = translateTransform;
            }

            double timeDuration = 0;
            if (m_owningFlyout != null && m_owningFlyout.TryGetTarget(out var flyout) &&
                flyout.CurrentOptions?.Placement is MenuFlyoutExPlacementMode placement)
            {
                var animationProperty = GetAnimationProperty(placement);
                timeDuration = placement switch
                {
                    MenuFlyoutExPlacementMode.Top or MenuFlyoutExPlacementMode.Bottom => RenderSize.Height * Vtd_factor,
                    MenuFlyoutExPlacementMode.Left or MenuFlyoutExPlacementMode.Right => RenderSize.Width * Htd_factor,
                    MenuFlyoutExPlacementMode.Full => 0,
                    MenuFlyoutExPlacementMode.TopEdgeAlignedLeft or MenuFlyoutExPlacementMode.TopEdgeAlignedRight => RenderSize.Height * Vtd_factor,
                    MenuFlyoutExPlacementMode.BottomEdgeAlignedLeft or MenuFlyoutExPlacementMode.BottomEdgeAlignedRight => RenderSize.Height * Vtd_factor,
                    MenuFlyoutExPlacementMode.LeftEdgeAlignedTop or MenuFlyoutExPlacementMode.LeftEdgeAlignedBottom => RenderSize.Width * Htd_factor,
                    MenuFlyoutExPlacementMode.RightEdgeAlignedTop or MenuFlyoutExPlacementMode.RightEdgeAlignedBottom => RenderSize.Width * Htd_factor,
                    MenuFlyoutExPlacementMode.AppBarTop or MenuFlyoutExPlacementMode.AppBarBottom => RenderSize.Height * Vtd_factor,
                    MenuFlyoutExPlacementMode.AppBarLeft or MenuFlyoutExPlacementMode.AppBarRight => RenderSize.Width * Htd_factor,
                    _ => timeDuration
                };


                double? from;
                if (PopupMode == ContextMenuPopupMode.AlwaysPopup)
                {
                    from = GetTargetValue(placement, true);

                    var animation = new DoubleAnimation
                    {
                        From = from,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(timeDuration),
                        EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
                    };

                    translateTransform.BeginAnimation(animationProperty, animation);
                }
                else
                {
                    from = GetTargetValue(placement, false);

                    // This makes sure that the initial position is set correctly
                    // And it will only work before any animation is applied
                    if (!translateTransform.IsSealed)
                    {
                        translateTransform.SetValue(animationProperty, from);
                    }

                    var animationDuration = TimeSpan.FromSeconds(timeDuration);
                    var animation = new DoubleAnimationUsingKeyFrames
                    {
                        KeyFrames =
                        [
                            new SplineDoubleKeyFrame
                            {
                                KeySpline = new KeySpline(0, 0, 0, 1),
                                KeyTime = animationDuration,
                                Value = 0
                            },
                        ],
                        Duration = animationDuration
                    };

                    translateTransform.BeginAnimation(animationProperty, animation);
                }
            }
        }

        m_asyncShow = null;
    }

    private void ApplyCloseAnimation()
    {
        if (this.GetTemplateChild<Decorator>("Shdw") is { } chorme)
        {
            // Ensure RenderTransform is a TranslateTransform
            if (chorme.RenderTransform is TranslateTransform translateTransform)
            {
                translateTransform.X = translateTransform.Y = 0;
            }
            else
            {
                translateTransform = new TranslateTransform();
                chorme.RenderTransform = translateTransform;
            }

            double? to = null;
            var animationProperty = TranslateTransform.YProperty;
            double timeDuration = 0;
            if (m_owningFlyout != null && m_owningFlyout.TryGetTarget(out var flyout) &&
                flyout.CurrentOptions?.Placement is MenuFlyoutExPlacementMode placement)
            {
                to = GetTargetValue(placement, false);
                animationProperty = GetAnimationProperty(placement);
                timeDuration = placement switch
                {
                    MenuFlyoutExPlacementMode.Top or MenuFlyoutExPlacementMode.Bottom => RenderSize.Height * Cvtd_factor,
                    MenuFlyoutExPlacementMode.Left or MenuFlyoutExPlacementMode.Right => RenderSize.Width * Chtd_factor,
                    MenuFlyoutExPlacementMode.Full => 0,
                    MenuFlyoutExPlacementMode.TopEdgeAlignedLeft or MenuFlyoutExPlacementMode.TopEdgeAlignedRight => RenderSize.Height * Cvtd_factor,
                    MenuFlyoutExPlacementMode.BottomEdgeAlignedLeft or MenuFlyoutExPlacementMode.BottomEdgeAlignedRight => RenderSize.Height * Cvtd_factor,
                    MenuFlyoutExPlacementMode.LeftEdgeAlignedTop or MenuFlyoutExPlacementMode.LeftEdgeAlignedBottom => RenderSize.Width * Chtd_factor,
                    MenuFlyoutExPlacementMode.RightEdgeAlignedTop or MenuFlyoutExPlacementMode.RightEdgeAlignedBottom => RenderSize.Width * Chtd_factor,
                    MenuFlyoutExPlacementMode.AppBarTop or MenuFlyoutExPlacementMode.AppBarBottom => RenderSize.Height * Cvtd_factor,
                    MenuFlyoutExPlacementMode.AppBarLeft or MenuFlyoutExPlacementMode.AppBarRight => RenderSize.Width * Chtd_factor,
                    _ => timeDuration
                };
            }

            var animationDuration = TimeSpan.FromSeconds(timeDuration);
            var animation = new DoubleAnimation
            {
                From = 0,
                To = to,
                Duration = animationDuration,
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
            };

            translateTransform.BeginAnimation(animationProperty, animation);
        }

        m_asyncHide = null;
    }

    private static double? GetTargetValue(MenuFlyoutExPlacementMode placement, bool open)
    {
        if (open)
        {
            return placement switch
            {
                MenuFlyoutExPlacementMode.Left or MenuFlyoutExPlacementMode.Top => C_offset,
                MenuFlyoutExPlacementMode.Right or MenuFlyoutExPlacementMode.Bottom => -C_offset,
                MenuFlyoutExPlacementMode.Full => null,
                MenuFlyoutExPlacementMode.TopEdgeAlignedLeft or MenuFlyoutExPlacementMode.TopEdgeAlignedRight => C_offset,
                MenuFlyoutExPlacementMode.BottomEdgeAlignedLeft or MenuFlyoutExPlacementMode.BottomEdgeAlignedRight => -C_offset,
                MenuFlyoutExPlacementMode.LeftEdgeAlignedTop or MenuFlyoutExPlacementMode.LeftEdgeAlignedBottom => C_offset,
                MenuFlyoutExPlacementMode.RightEdgeAlignedTop or MenuFlyoutExPlacementMode.RightEdgeAlignedBottom => -C_offset,
                MenuFlyoutExPlacementMode.Auto => throw new NotImplementedException($"{MenuFlyoutExPlacementMode.Auto} is not supported in {nameof(MenuFlyoutEx)}"),
                MenuFlyoutExPlacementMode.AppBarLeft or MenuFlyoutExPlacementMode.AppBarTop => C_offset,
                MenuFlyoutExPlacementMode.AppBarRight or MenuFlyoutExPlacementMode.AppBarBottom => -C_offset,
                _ => null
            };
        }
        else
        {
            return placement switch
            {
                MenuFlyoutExPlacementMode.Left or MenuFlyoutExPlacementMode.Top => C_closeOffset,
                MenuFlyoutExPlacementMode.Right or MenuFlyoutExPlacementMode.Bottom => -C_closeOffset,
                MenuFlyoutExPlacementMode.Full => null,
                MenuFlyoutExPlacementMode.TopEdgeAlignedLeft or MenuFlyoutExPlacementMode.TopEdgeAlignedRight => C_closeOffset,
                MenuFlyoutExPlacementMode.BottomEdgeAlignedLeft or MenuFlyoutExPlacementMode.BottomEdgeAlignedRight => -C_closeOffset,
                MenuFlyoutExPlacementMode.LeftEdgeAlignedTop or MenuFlyoutExPlacementMode.LeftEdgeAlignedBottom => C_closeOffset,
                MenuFlyoutExPlacementMode.RightEdgeAlignedTop or MenuFlyoutExPlacementMode.RightEdgeAlignedBottom => -C_closeOffset,
                MenuFlyoutExPlacementMode.Auto => throw new NotImplementedException($"{MenuFlyoutExPlacementMode.Auto} is not supported in {nameof(MenuFlyoutEx)}"),
                MenuFlyoutExPlacementMode.AppBarLeft or MenuFlyoutExPlacementMode.AppBarTop => C_closeOffset,
                MenuFlyoutExPlacementMode.AppBarRight or MenuFlyoutExPlacementMode.AppBarBottom => -C_closeOffset,
                _ => null
            };
        }
    }

    private static DependencyProperty GetAnimationProperty(MenuFlyoutExPlacementMode placement)
    {
        return placement switch
        {
            MenuFlyoutExPlacementMode.Top or MenuFlyoutExPlacementMode.Bottom => TranslateTransform.YProperty,
            MenuFlyoutExPlacementMode.Left or MenuFlyoutExPlacementMode.Right => TranslateTransform.XProperty,
            MenuFlyoutExPlacementMode.Full => TranslateTransform.YProperty,
            MenuFlyoutExPlacementMode.TopEdgeAlignedLeft or MenuFlyoutExPlacementMode.TopEdgeAlignedRight => TranslateTransform.YProperty,
            MenuFlyoutExPlacementMode.BottomEdgeAlignedLeft or MenuFlyoutExPlacementMode.BottomEdgeAlignedRight => TranslateTransform.YProperty,
            MenuFlyoutExPlacementMode.LeftEdgeAlignedTop or MenuFlyoutExPlacementMode.LeftEdgeAlignedBottom => TranslateTransform.XProperty,
            MenuFlyoutExPlacementMode.RightEdgeAlignedTop or MenuFlyoutExPlacementMode.RightEdgeAlignedBottom => TranslateTransform.XProperty,
            MenuFlyoutExPlacementMode.Auto => TranslateTransform.YProperty,
            MenuFlyoutExPlacementMode.AppBarTop or MenuFlyoutExPlacementMode.AppBarBottom => TranslateTransform.YProperty,
            MenuFlyoutExPlacementMode.AppBarLeft or MenuFlyoutExPlacementMode.AppBarRight => TranslateTransform.XProperty,
            _ => TranslateTransform.YProperty
        };
    }

    private void HandlePopupMouseButtonEvent(object sender, MouseButtonEventArgs e)
    {
        if (!_parentPopup!.IsOpen)
        {
            e.Handled = true;
        }
    }

    private Popup? _parentPopup;
    private WeakReference<MenuFlyoutEx>? m_owningFlyout;
    private DispatcherOperation? m_asyncShow;
    private DispatcherOperation? m_asyncHide;

    private const double C_offset = 25;
    private const double Vtd_factor = 0.001875;
    private const double Htd_factor = 0.000750;

    private const double C_closeOffset = 10;
    private const double Cvtd_factor = 0.000875;
    private const double Chtd_factor = 0.000375;
}
