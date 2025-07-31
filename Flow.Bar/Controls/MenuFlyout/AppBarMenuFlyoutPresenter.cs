using iNKORE.UI.WPF.Modern.Controls.Helpers;
using iNKORE.UI.WPF.Modern.Controls.Primitives;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Flow.Bar.Controls.MenuFlyout;

public class AppBarMenuFlyoutPresenter : ContextMenu
{
    static AppBarMenuFlyoutPresenter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBarMenuFlyoutPresenter), new FrameworkPropertyMetadata(typeof(AppBarMenuFlyoutPresenter)));

        IsOpenProperty.OverrideMetadata(typeof(AppBarMenuFlyoutPresenter), new FrameworkPropertyMetadata(OnIsOpenChanged));
    }

    public AppBarMenuFlyoutPresenter()
    {
    }

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty =
        ControlHelper.CornerRadiusProperty.AddOwner(typeof(AppBarMenuFlyoutPresenter));

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
            typeof(AppBarMenuFlyoutPresenter),
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

    internal void SetOwningFlyout(AppBarMenuFlyout owningFlyout)
    {
        m_owningFlyout = new WeakReference<AppBarMenuFlyout>(owningFlyout);
    }

    internal void UpdatePopupAnimation()
    {
        if (_parentPopup != null && m_owningFlyout!.TryGetTarget(out var _))
        {
            _parentPopup.Resources.Remove(SystemParameters.MenuPopupAnimationKey);
        }
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((AppBarMenuFlyoutPresenter)d).OnIsOpenChanged(e);
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
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, ApplyOpenAnimation);
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
        if (Template?.FindName("Shdw", this) is ThemeShadowChrome chorme)
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
            DependencyProperty dp = TranslateTransform.YProperty;
            double timeDuration = 0;
            if (m_owningFlyout != null && m_owningFlyout.TryGetTarget(out var flyout))
            {
                from = flyout.Placement switch
                {
                    AppBarPlacementMode.Left or AppBarPlacementMode.Top => s_offset,
                    AppBarPlacementMode.Right or AppBarPlacementMode.Bottom => -s_offset,
                    _ => null
                };
                dp = flyout.Placement switch
                {
                    AppBarPlacementMode.Top or AppBarPlacementMode.Bottom => TranslateTransform.YProperty,
                    AppBarPlacementMode.Left or AppBarPlacementMode.Right => TranslateTransform.XProperty,
                    _ => dp
                };
                timeDuration = flyout.Placement switch
                {
                    AppBarPlacementMode.Top or AppBarPlacementMode.Bottom => RenderSize.Height * vtd_factor,
                    AppBarPlacementMode.Left or AppBarPlacementMode.Right => RenderSize.Width * htd_factor,
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
    }

    private void HandlePopupMouseButtonEvent(object sender, MouseButtonEventArgs e)
    {
        if (!_parentPopup!.IsOpen)
        {
            e.Handled = true;
        }
    }

    private Popup? _parentPopup;
    private WeakReference<AppBarMenuFlyout>? m_owningFlyout;

    private const double s_offset = 30;
    private const double vtd_factor = 0.002734375;
    private const double htd_factor = 0.000735294;
}
