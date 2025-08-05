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

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is StackPanelViewExItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new StackPanelViewExItem();
    }
}
