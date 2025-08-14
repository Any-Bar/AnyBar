using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
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

    public Action<ContextMenu>? OnApplyTemplateAction { get; set; }

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

    internal void CancelAsyncShow()
    {
        if (m_asyncShow != null)
        {
            m_asyncShow.Abort();
            m_asyncShow = null;
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

            double? from = null;
            var dp = TranslateTransform.YProperty;
            double timeDuration = 0;
            if (m_owningFlyout != null && m_owningFlyout.TryGetTarget(out var flyout) &&
                flyout.CurrentOptions?.Placement is MenuFlyoutExPlacementMode placement)
            {
                from = placement switch
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
                dp = placement switch
                {
                    MenuFlyoutExPlacementMode.Top or MenuFlyoutExPlacementMode.Bottom => TranslateTransform.YProperty,
                    MenuFlyoutExPlacementMode.Left or MenuFlyoutExPlacementMode.Right => TranslateTransform.XProperty,
                    MenuFlyoutExPlacementMode.Full => dp,
                    MenuFlyoutExPlacementMode.TopEdgeAlignedLeft or MenuFlyoutExPlacementMode.TopEdgeAlignedRight => TranslateTransform.YProperty,
                    MenuFlyoutExPlacementMode.BottomEdgeAlignedLeft or MenuFlyoutExPlacementMode.BottomEdgeAlignedRight => TranslateTransform.YProperty,
                    MenuFlyoutExPlacementMode.LeftEdgeAlignedTop or MenuFlyoutExPlacementMode.LeftEdgeAlignedBottom => TranslateTransform.XProperty,
                    MenuFlyoutExPlacementMode.RightEdgeAlignedTop or MenuFlyoutExPlacementMode.RightEdgeAlignedBottom => TranslateTransform.XProperty,
                    MenuFlyoutExPlacementMode.Auto => dp,
                    MenuFlyoutExPlacementMode.AppBarTop or MenuFlyoutExPlacementMode.AppBarBottom => TranslateTransform.YProperty,
                    MenuFlyoutExPlacementMode.AppBarLeft or MenuFlyoutExPlacementMode.AppBarRight => TranslateTransform.XProperty,
                    _ => dp
                };
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
            }

            var animation = new DoubleAnimation
            {
                From = from,
                To = 0,
                Duration = TimeSpan.FromSeconds(timeDuration),
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
            };

            translateTransform.BeginAnimation(dp, animation);
        }

        m_asyncShow = null;
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

    private const double C_offset = 30;
    private const double Vtd_factor = 0.001875;
    private const double Htd_factor = 0.000750;
}
