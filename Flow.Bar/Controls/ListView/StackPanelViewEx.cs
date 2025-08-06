using Flow.Bar.Helper.DragDrop;
using System.Windows;
using System.Windows.Controls;

namespace Flow.Bar.Controls;

public class StackPanelViewEx : StackPanelViewExBase
{
    static StackPanelViewEx()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StackPanelViewEx), new FrameworkPropertyMetadata(typeof(StackPanelViewEx)));
    }

    public StackPanelViewEx()
    {
    }

    #region Orientation

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StackPanelViewEx),
            new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    #endregion

    #region CanReorderItems

    public static readonly DependencyProperty CanReorderItemsProperty =
        DependencyProperty.Register(
            nameof(CanReorderItems),
            typeof(bool),
            typeof(StackPanelViewEx),
            new FrameworkPropertyMetadata(false, OnCanReorderItemsPropertyChanged));

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

    public bool CanReorderItems
    {
        get => (bool)GetValue(CanReorderItemsProperty);
        set => SetValue(CanReorderItemsProperty, value);
    }

    #endregion

    #region DropScrollViewer

    public static readonly DependencyProperty DropScrollViewerProperty =
        DependencyProperty.Register(
            nameof(DropScrollViewer),
            typeof(ScrollViewer),
            typeof(StackPanelViewEx),
            new FrameworkPropertyMetadata(null, OnDropScrollViewerPropertyChanged));

    private static void OnDropScrollViewerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is ScrollViewer scrollViewer)
        {
            d.SetDropScrollViewer(scrollViewer);
        }
    }

    public ScrollViewer DropScrollViewer
    {
        get => (ScrollViewer)GetValue(DropScrollViewerProperty);
        set => SetValue(DropScrollViewerProperty, value);
    }

    #endregion

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is StackPanelViewExItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new StackPanelViewExItem();
    }
}
