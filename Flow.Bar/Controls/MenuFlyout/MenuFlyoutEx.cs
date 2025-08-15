using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Threading;
using Flow.Bar.Plugin;

namespace Flow.Bar.Controls;

[ContentProperty(nameof(Items))]
public class MenuFlyoutEx : DependencyObject
{
    private const string C_contextMenuStyleKey = "DefaultContextMenuStyle";
    private static Style? s_contextMenuStyle;

    public MenuFlyoutEx()
    {
        // Sometimes application is exiting but menu is still being created, so we need to check if Application.Current is null
        if (Application.Current == null) return;

        s_contextMenuStyle ??= (Style)Application.Current.Resources[C_contextMenuStyleKey];

        ArgumentNullException.ThrowIfNull(s_contextMenuStyle, $"{C_contextMenuStyleKey} not found in {nameof(Application)} {nameof(ResourceDictionary)}");

        MenuFlyoutPresenterStyle = s_contextMenuStyle;
    }

    public ItemCollection Items => EnsurePresenter().Items;

    public Action<ContextMenu>? OnApplyTemplateAction
    {
        get => EnsurePresenter().OnApplyTemplateAction;
        set => EnsurePresenter().OnApplyTemplateAction = value;
    }

    public ContextMenuPopupMode PopupMode
    {
        get => EnsurePresenter().PopupMode;
        set => EnsurePresenter().PopupMode = value;
    }

    #region Width

    public static readonly DependencyProperty WidthProperty =
        DependencyProperty.Register(
            nameof(Width),
            typeof(double),
            typeof(MenuFlyoutEx),
            new PropertyMetadata(double.NaN, OnWidthChanged));

    public double Width
    {
        get => (double)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((MenuFlyoutEx)d).OnWidthChanged(e);
    }

    private void OnWidthChanged(DependencyPropertyChangedEventArgs e)
    {
        if (m_presenter != null)
        {
            m_presenter.Width = (double)e.NewValue;
        }
    }

    #endregion

    #region Height

    public static readonly DependencyProperty HeightProperty =
        DependencyProperty.Register(
            nameof(Height),
            typeof(double),
            typeof(MenuFlyoutEx),
            new PropertyMetadata(double.NaN, OnHeightChanged));

    public double Height
    {
        get => (double)GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    private static void OnHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((MenuFlyoutEx)d).OnHeightChanged(e);
    }

    private void OnHeightChanged(DependencyPropertyChangedEventArgs e)
    {
        if (m_presenter != null)
        {
            m_presenter.Height = (double)e.NewValue;
        }
    }

    #endregion

    #region MenuFlyoutPresenterStyle

    public static readonly DependencyProperty MenuFlyoutPresenterStyleProperty =
        DependencyProperty.Register(
            nameof(MenuFlyoutPresenterStyle),
            typeof(Style),
            typeof(MenuFlyoutEx),
            new PropertyMetadata(OnMenuFlyoutPresenterStyleChanged));

    public Style MenuFlyoutPresenterStyle
    {
        get => (Style)GetValue(MenuFlyoutPresenterStyleProperty);
        set => SetValue(MenuFlyoutPresenterStyleProperty, value);
    }

    private static void OnMenuFlyoutPresenterStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((MenuFlyoutEx)d).OnMenuFlyoutPresenterStyleChanged(e);
    }

    private void OnMenuFlyoutPresenterStyleChanged(DependencyPropertyChangedEventArgs e)
    {
        if (m_presenter != null)
        {
            m_presenter.Style = (Style)e.NewValue;
        }
    }

    #endregion

    #region StaysOpen

    private static readonly DependencyPropertyKey StaysOpenPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(StaysOpen),
            typeof(bool),
            typeof(MenuFlyoutEx),
            new PropertyMetadata(false, OnStaysOpenChanged));

    public bool StaysOpen
    {
        get => (bool)GetValue(StaysOpenPropertyKey.DependencyProperty);
        internal set => SetValue(StaysOpenPropertyKey, value);
    }

    public static readonly DependencyProperty StaysOpenProperty =
        StaysOpenPropertyKey.DependencyProperty;

    private static void OnStaysOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((MenuFlyoutEx)d).OnStaysOpenChanged();
    }

    #endregion

    #region IsOpen

    private static readonly DependencyPropertyKey IsOpenPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsOpen),
            typeof(bool),
            typeof(MenuFlyoutEx),
            new PropertyMetadata(false, OnIsOpenChanged));

    public static readonly DependencyProperty IsOpenProperty =
        IsOpenPropertyKey.DependencyProperty;

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        internal set => SetValue(IsOpenPropertyKey, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((MenuFlyoutEx)d).OnIsOpenChanged();
    }

    #endregion

    #region Placement

    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.Register(
            nameof(Placement),
            typeof(MenuFlyoutExPlacementMode),
            typeof(MenuFlyoutEx),
            new PropertyMetadata(MenuFlyoutExPlacementMode.Top));

    public MenuFlyoutExPlacementMode Placement
    {
        get => (MenuFlyoutExPlacementMode)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    #endregion

    public void ShowAt(FrameworkElement placementTarget)
    {
        ArgumentNullException.ThrowIfNull(placementTarget);

        var showOptions = new MenuFlyoutExOptions
        {
            Placement = Placement
        };
        ShowAt(placementTarget, showOptions);
    }

    public void ShowAt(FrameworkElement placementTarget, MenuFlyoutExOptions showOptions)
    {
        ArgumentNullException.ThrowIfNull(placementTarget);
        ArgumentNullException.ThrowIfNull(showOptions);

        ShowAtCore(placementTarget, showOptions);
    }

    public void Hide()
    {
        CancelAsyncShow();
        HideCore();
    }

    internal void ShowAtCore(FrameworkElement placementTarget, MenuFlyoutExOptions showOptions)
    {
        CancelAsyncShow();

        if (m_presenter != null &&
            m_presenter.IsOpen &&
            m_presenter.PlacementTarget == placementTarget &&
            m_presenter.Placement == PlacementMode.Custom &&
            m_currentOptions == showOptions)
        {
            return;
        }

        if (m_opened)
        {
            m_pendingShow = () => ShowAtCore(placementTarget, showOptions);
            return;
        }

        m_target = placementTarget;
        EnsurePresenter();

        if (m_presenter!.IsOpen)
        {
            m_presenter.IsOpen = false;
        }

        m_currentOptions = showOptions;

        m_presenter.Placement = PlacementMode.Custom;
        m_presenter.PlacementTarget = placementTarget;
        m_presenter.PlacementRectangle = GetPlacementRectangle(placementTarget);

        OnOpening();
        m_presenter.IsOpen = true;
    }

    internal void HideCore()
    {
        if (m_presenter != null && m_presenter.IsOpen)
        {
            m_presenter.IsOpen = false;
        }
    }

    internal void OnIsOpenChanged()
    {
        if (m_presenter != null)
        {
            m_presenter.IsOpen = IsOpen;
        }
    }

    internal void OnStaysOpenChanged()
    {
        if (m_presenter != null)
        {
            m_presenter.StaysOpen = true;
        }
    }

    internal void UpdateIsOpen()
    {
        IsOpen = m_presenter != null && m_presenter.IsOpen;
    }

    private MenuFlyoutExPresenter EnsurePresenter()
    {
        if (m_presenter == null)
        {
            var presenter = new MenuFlyoutExPresenter
            {
                Width = Width,
                Height = Height,
                Style = MenuFlyoutPresenterStyle,
                Placement = PlacementMode.Custom,
                CustomPopupPlacementCallback = PositionPopup,
                StaysOpen = StaysOpen
            };
            presenter.SetOwningFlyout(this);
            presenter.UpdatePopupAnimation();
            presenter.Opened += OnPresenterOpened;
            presenter.Closed += OnPresenterClosed;
            presenter.IsOpenChanged += OnPresenterIsOpenChanged;

            m_presenter = presenter;
        }

        return m_presenter;
    }

    private CustomPopupPlacement[] PositionPopup(Size popupSize, Size targetSize, Point offset)
    {
        return MenuFlyoutExPlacementHelper.PositionPopup(m_currentOptions!, popupSize, targetSize, offset, m_target!, m_presenter!);
    }

    private void OnPresenterOpened(object? sender, RoutedEventArgs e)
    {
        OnOpened();
    }

    private void OnPresenterClosed(object? sender, RoutedEventArgs e)
    {
        if (!m_presenter!.IsOpen)
        {
            m_presenter.ClearValue(ContextMenu.PlacementProperty);
            m_presenter.ClearValue(ContextMenu.PlacementTargetProperty);
            m_presenter.ClearValue(ContextMenu.PlacementRectangleProperty);
            m_currentOptions = null;
        }

        OnClosed();
    }

    private void OnPresenterIsOpenChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        UpdateIsOpen();
    }

    internal Rect GetPlacementRectangle(UIElement target)
    {
        var value = Rect.Empty;

        if (target != null)
        {
            var targetSize = target.RenderSize;

            switch (m_currentOptions!.Placement)
            {
                case MenuFlyoutExPlacementMode.Top:
                case MenuFlyoutExPlacementMode.Bottom:
                case MenuFlyoutExPlacementMode.TopEdgeAlignedLeft:
                case MenuFlyoutExPlacementMode.TopEdgeAlignedRight:
                case MenuFlyoutExPlacementMode.BottomEdgeAlignedLeft:
                case MenuFlyoutExPlacementMode.BottomEdgeAlignedRight:
                    value = new Rect(
                        new Point(0, -C_offset),
                        new Point(targetSize.Width, targetSize.Height + C_offset));
                    break;
                case MenuFlyoutExPlacementMode.Left:
                case MenuFlyoutExPlacementMode.Right:
                case MenuFlyoutExPlacementMode.LeftEdgeAlignedTop:
                case MenuFlyoutExPlacementMode.LeftEdgeAlignedBottom:
                case MenuFlyoutExPlacementMode.RightEdgeAlignedTop:
                case MenuFlyoutExPlacementMode.RightEdgeAlignedBottom:
                    value = new Rect(
                        new Point(-C_offset, 0),
                        new Point(targetSize.Width + C_offset, targetSize.Height));
                    break;
                case MenuFlyoutExPlacementMode.Auto:
                    throw new NotImplementedException($"{MenuFlyoutExPlacementMode.Auto} is not supported in {nameof(MenuFlyoutEx)}");
                case MenuFlyoutExPlacementMode.AppBarTop:
                case MenuFlyoutExPlacementMode.AppBarBottom:
                    value = new Rect(
                        new Point(0, -C_offset),
                        new Point(targetSize.Width, targetSize.Height + C_offset));
                    break;
                case MenuFlyoutExPlacementMode.AppBarLeft:
                case MenuFlyoutExPlacementMode.AppBarRight:
                    value = new Rect(
                        new Point(-C_offset, 0),
                        new Point(targetSize.Width + C_offset, targetSize.Height));
                    break;
            }
        }

        return value;
    }

    internal virtual void OnOpening()
    {
        Opening?.Invoke(this, null);
    }

    internal virtual void OnOpened()
    {
        Opened?.Invoke(this, null);
        m_opened = true;
    }

    internal virtual void OnClosed()
    {
        Closed?.Invoke(this, null);
        m_opened = false;

        var pendingShow = m_pendingShow;
        CancelAsyncShow();
        if (pendingShow != null)
        {
            m_asyncShow = Dispatcher.BeginInvoke(pendingShow);
        }
    }

    private void CancelAsyncShow()
    {
        m_presenter?.CancelAsyncShowOrHide();
        m_pendingShow = null;

        if (m_asyncShow != null)
        {
            m_asyncShow.Abort();
            m_asyncShow = null;
        }
    }

    public event EventHandler<object?>? Opening;
    public event EventHandler<object?>? Opened;
    public event EventHandler<object?>? Closed;

    internal MenuFlyoutExOptions? CurrentOptions => m_currentOptions;

    private MenuFlyoutExPresenter? m_presenter;
    private MenuFlyoutExOptions? m_currentOptions;

    private const double C_offset = 4;

    private FrameworkElement? m_target;
    private bool m_opened;
    private Action? m_pendingShow;
    private DispatcherOperation? m_asyncShow;
}
