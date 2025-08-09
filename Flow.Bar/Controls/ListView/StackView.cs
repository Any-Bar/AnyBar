using Flow.Bar.Helper.DragDrop;
using System.Windows;
using System.Windows.Controls;

namespace Flow.Bar.Controls;

public class StackView : StackViewBase
{
    static StackView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StackView), new FrameworkPropertyMetadata(typeof(StackView)));
    }

    public StackView()
    {
    }

    #region Orientation

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StackView),
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
            typeof(StackView),
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
            typeof(StackView),
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

    #region IsHoverEnabled

    public static readonly DependencyProperty IsHoverEnabledProperty =
        DependencyProperty.Register(
            nameof(IsHoverEnabled),
            typeof(bool),
            typeof(StackView),
            new FrameworkPropertyMetadata(true));

    public bool IsHoverEnabled
    {
        get => (bool)GetValue(IsHoverEnabledProperty);
        set => SetValue(IsHoverEnabledProperty, value);
    }

    #endregion

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is StackViewItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new StackViewItem();
    }
}
