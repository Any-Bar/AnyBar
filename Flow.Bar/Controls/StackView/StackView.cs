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
        Loaded += StackView_Loaded;
    }

    private void StackView_Loaded(object sender, RoutedEventArgs e)
    {
        SelectedIndex = -1; // Do not select any item on load
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

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is StackViewItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new StackViewItem();
    }
}
