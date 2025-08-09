using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Threading;

namespace Flow.Bar.Controls;

[ContentProperty(nameof(Items))]
public class MenuFlyoutEx : DependencyObject
{
    private const string c_contextMenuStyleKey = "DefaultContextMenuStyle";
    private static Style? s_contextMenuStyle;

    public MenuFlyoutEx()
    {
        // TODO: Use <Style BasedOn="{StaticResource DefaultContextMenuStyle}" TargetType="local:MenuFlyoutExPresenter" /> instead of this
        s_contextMenuStyle ??= (Style)Application.Current.Resources[c_contextMenuStyleKey];

        ArgumentNullException.ThrowIfNull(s_contextMenuStyle, $"{c_contextMenuStyleKey} not found in Application resources.");

        MenuFlyoutPresenterStyle = s_contextMenuStyle;
    }

    public ItemCollection Items
    {
        get
        {
            return EnsurePresenter().Items;
        }
    }

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

    #region Placement

    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.Register(
            nameof(Placement),
            typeof(MenuFlyoutExPlacementMode),
            typeof(MenuFlyoutEx),
            new PropertyMetadata(MenuFlyoutExPlacementMode.AppBarBottom));

    public MenuFlyoutExPlacementMode Placement
    {
        get => (MenuFlyoutExPlacementMode)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
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

    #region ShowOptions

    public static readonly DependencyProperty ShowOptionsProperty =
        DependencyProperty.Register(
            nameof(ShowOptions),
            typeof(MenuFlyoutExOptions),
            typeof(MenuFlyoutEx),
            new PropertyMetadata(null));

    public MenuFlyoutExOptions ShowOptions
    {
        get => (MenuFlyoutExOptions)GetValue(ShowOptionsProperty);
        set => SetValue(ShowOptionsProperty, value);
    }

    #endregion

    public void ShowAt(FrameworkElement placementTarget, MenuFlyoutExOptions showOptions)
    {
        ArgumentNullException.ThrowIfNull(placementTarget);
        ArgumentNullException.ThrowIfNull(showOptions);

        ShowOptions = showOptions;
        Placement = showOptions.Placement;
        ShowAtCore(placementTarget);
    }

    public void Hide()
    {
        CancelAsyncShow();
        HideCore();
    }

    internal void ShowAtCore(FrameworkElement placementTarget)
    {
        CancelAsyncShow();

        if (m_presenter != null &&
            m_presenter.IsOpen &&
            m_presenter.PlacementTarget == placementTarget &&
            m_presenter.Placement == PlacementMode.Custom &&
            m_currentPlacement == Placement)
        {
            return;
        }

        if (m_opened)
        {
            m_pendingShow = () => ShowAtCore(placementTarget);
            return;
        }

        m_target = placementTarget;
        EnsurePresenter();

        if (m_presenter!.IsOpen)
        {
            m_presenter.IsOpen = false;
        }

        m_presenter.Placement = PlacementMode.Custom;
        m_presenter.PlacementTarget = placementTarget;

        m_presenter.PlacementRectangle = GetPlacementRectangle(placementTarget);

        m_currentPlacement = Placement;
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

    private CustomPopupPlacement[] PositionPopup(Size popupSize, Size targetSize, Point offset)
    {
        return MenuFlyoutExPlacementHelper.PositionPopup(Placement, popupSize, targetSize, ShowOptions.Monitor, ShowOptions.Position, offset, m_target!, m_presenter!);
    }   

    private MenuFlyoutExPresenter EnsurePresenter()
    {
        if (m_presenter == null)
        {
            var presenter = new MenuFlyoutExPresenter
            {
                Style = MenuFlyoutPresenterStyle,
                Placement = PlacementMode.Custom,
                CustomPopupPlacementCallback = PositionPopup,
                StaysOpen = false
            };
            presenter.SetOwningFlyout(this);
            BindPlacement(presenter);
            presenter.UpdatePopupAnimation();
            presenter.StaysOpen = StaysOpen;
            presenter.Opened += OnPresenterOpened;
            presenter.Closed += OnPresenterClosed;
            presenter.IsOpenChanged += OnPresenterIsOpenChanged;

            m_presenter = presenter;
        }

        return m_presenter;
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
            m_currentPlacement = null;
        }

        OnClosed();
    }

    private void OnPresenterIsOpenChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        UpdateIsOpen();
    }

    internal Rect GetPlacementRectangle(UIElement target)
    {
        Rect value = Rect.Empty;

        if (target != null)
        {
            Size targetSize = target.RenderSize;

            switch (Placement)
            {
                case MenuFlyoutExPlacementMode.Top:
                case MenuFlyoutExPlacementMode.Bottom:
                case MenuFlyoutExPlacementMode.TopEdgeAlignedLeft:
                case MenuFlyoutExPlacementMode.TopEdgeAlignedRight:
                case MenuFlyoutExPlacementMode.BottomEdgeAlignedLeft:
                case MenuFlyoutExPlacementMode.BottomEdgeAlignedRight:
                    value = new Rect(
                        new Point(0, -Offset),
                        new Point(targetSize.Width, targetSize.Height + Offset));
                    break;
                case MenuFlyoutExPlacementMode.Left:
                case MenuFlyoutExPlacementMode.Right:
                case MenuFlyoutExPlacementMode.LeftEdgeAlignedTop:
                case MenuFlyoutExPlacementMode.LeftEdgeAlignedBottom:
                case MenuFlyoutExPlacementMode.RightEdgeAlignedTop:
                case MenuFlyoutExPlacementMode.RightEdgeAlignedBottom:
                    value = new Rect(
                        new Point(-Offset, 0),
                        new Point(targetSize.Width + Offset, targetSize.Height));
                    break;
                case MenuFlyoutExPlacementMode.Auto:
                    throw new NotImplementedException("Auto placement mode is not supported in MenuFlyoutEx.");
                case MenuFlyoutExPlacementMode.AppBarTop:
                case MenuFlyoutExPlacementMode.AppBarBottom:
                    value = new Rect(
                        new Point(0, -Offset),
                        new Point(targetSize.Width, targetSize.Height + Offset));
                    break;
                case MenuFlyoutExPlacementMode.AppBarLeft:
                case MenuFlyoutExPlacementMode.AppBarRight:
                    value = new Rect(
                        new Point(-Offset, 0),
                        new Point(targetSize.Width + Offset, targetSize.Height));
                    break;
            }
        }

        return value;
    }

    internal void BindPlacement(Control presenter)
    {
        presenter.SetBinding(
            MenuFlyoutExPlacementHelper.PlacementProperty,
            new Binding
            {
                Path = new PropertyPath(PlacementProperty),
                Source = this,
                Converter = s_placementConverter
            });
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
        m_presenter?.CancelAsyncShow();
        m_pendingShow = null;

        if (m_asyncShow != null)
        {
            m_asyncShow.Abort();
            m_asyncShow = null;
        }
    }

    private MenuFlyoutExPresenter? m_presenter;
    private MenuFlyoutExPlacementMode? m_currentPlacement;

    private double Offset { get; set; } = s_offset;

    public event EventHandler<object?>? Opening;
    public event EventHandler<object?>? Opened;
    public event EventHandler<object?>? Closed;

    private static readonly IValueConverter s_placementConverter = new PlacementConverter();

    private const double s_offset = 4;

    private FrameworkElement? m_target;
    private bool m_opened;
    private Action? m_pendingShow;
    private DispatcherOperation? m_asyncShow;

    private class PlacementConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (MenuFlyoutExPlacementMode)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
