using System.Windows;

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
