using System.Windows;
using System.Windows.Controls;

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

    #region Orientation

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(ListViewEx),
            new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
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
