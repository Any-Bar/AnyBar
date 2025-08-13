using System.Windows;
using System.Windows.Controls;
using Flow.Bar.Helpers.DragDrop;

namespace Flow.Bar.Controls;

public class ListViewEx : ListViewExBase
{
    static ListViewEx()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ListViewEx), new FrameworkPropertyMetadata(typeof(ListViewEx)));
    }

    public ListViewEx()
    {
    }

    #region ItemIsTabStop

    public static readonly DependencyProperty ItemIsTabStopProperty =
        DependencyProperty.Register(
            nameof(ItemIsTabStop),
            typeof(bool),
            typeof(ListViewEx),
            new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Gets or sets a value indicating whether the items in the ListViewEx can receive focus via tab navigation.
    /// </summary>
    /// <remarks>
    /// If the item contains focusable root elements like clickable SettingsCard,
    /// set this property to false to prevent the ListViewEx from duplicating tab stop.
    /// </remarks>
    public bool ItemIsTabStop
    {
        get => (bool)GetValue(ItemIsTabStopProperty);
        set => SetValue(ItemIsTabStopProperty, value);
    }

    #endregion

    #region CanReorderItems

    public static readonly DependencyProperty CanReorderItemsProperty =
        DependencyProperty.Register(
            nameof(CanReorderItems),
            typeof(bool),
            typeof(ListViewEx),
            new FrameworkPropertyMetadata(false, OnCanReorderItemsPropertyChanged));

    /// <summary>
    /// Gets or sets a value that indicates whether items in the view can be reordered through user interaction.
    /// </summary>
    public bool CanReorderItems
    {
        get => (bool)GetValue(CanReorderItemsProperty);
        set => SetValue(CanReorderItemsProperty, value);
    }

    private static void OnCanReorderItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.OldValue)
        {
            d.DisableDragDrop();
        }
        if ((bool)e.NewValue)
        {
            d.EnableDragDrop();
        }
    }

    #endregion

    #region DropScrollViewer

    public static readonly DependencyProperty DropScrollViewerProperty =
        DependencyProperty.Register(
            nameof(DropScrollViewer),
            typeof(ScrollViewer),
            typeof(ListViewEx),
            new FrameworkPropertyMetadata(null, OnDropScrollViewerPropertyChanged));

    private static void OnDropScrollViewerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is ScrollViewer scrollViewer)
        {
            d.SetDropScrollViewer(scrollViewer);
        }
    }

    /// <summary>
    /// Gets or sets the ScrollViewer that will be used for scrolling when items are dropped into the ListViewEx.
    /// </summary>
    public ScrollViewer DropScrollViewer
    {
        get => (ScrollViewer)GetValue(DropScrollViewerProperty);
        set => SetValue(DropScrollViewerProperty, value);
    }

    #endregion

    #region IsHoverEnabled

    public static readonly DependencyProperty IsHoverEnabledProperty =
        DependencyProperty.Register(
            nameof(IsHoverEnabled),
            typeof(bool),
            typeof(ListViewEx),
            new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Gets or sets a value indicating whether the items in the ListViewEx can have hover effects.
    /// </summary>
    /// <remarks>
    /// If the item contains elements like clickable SettingsCard with hover effects,
    /// set this property to false to prevent the ListViewEx from duplicating hover effects.
    /// </remarks>
    public bool IsHoverEnabled
    {
        get => (bool)GetValue(IsHoverEnabledProperty);
        set => SetValue(IsHoverEnabledProperty, value);
    }

    #endregion

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is ListViewExItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ListViewExItem();
    }
}
