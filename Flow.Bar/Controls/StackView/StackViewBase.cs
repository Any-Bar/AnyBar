using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using iNKORE.UI.WPF.Modern.Controls.Helpers;

namespace Flow.Bar.Controls;

public class StackViewBase : ListBox
{
    protected StackViewBase()
    {
    }

    #region UseSystemFocusVisuals

    /// <summary>
    /// Identifies the UseSystemFocusVisuals dependency property.
    /// </summary>
    public static readonly DependencyProperty UseSystemFocusVisualsProperty =
        FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(StackViewBase));

    /// <summary>
    /// Gets or sets a value that indicates whether the control uses focus visuals that
    /// are drawn by the system or those defined in the control template.
    /// </summary>
    public bool UseSystemFocusVisuals
    {
        get => (bool)GetValue(UseSystemFocusVisualsProperty);
        set => SetValue(UseSystemFocusVisualsProperty, value);
    }

    #endregion

    #region FocusVisualMargin

    /// <summary>
    /// Identifies the FocusVisualMargin dependency property.
    /// </summary>
    public static readonly DependencyProperty FocusVisualMarginProperty =
        FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(StackViewBase));

    /// <summary>
    /// Gets or sets the outer margin of the focus visual for a FrameworkElement.
    /// </summary>
    public Thickness FocusVisualMargin
    {
        get => (Thickness)GetValue(FocusVisualMarginProperty);
        set => SetValue(FocusVisualMarginProperty, value);
    }

    #endregion

    #region CornerRadius

    /// <summary>
    /// Identifies the CornerRadius dependency property.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        ControlHelper.CornerRadiusProperty.AddOwner(typeof(StackViewBase));

    /// <summary>
    /// Gets or sets the radius for the corners of the control's border.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    public event StackViewItemClickEventHandler? ItemClick;

    internal void NotifyListItemClicked(StackViewBaseItem item)
    {
        var clickedItem = ItemContainerGenerator.ItemFromContainer(item);
        ItemClick?.Invoke(this, new StackViewItemClickEventArgs { ClickedItem = clickedItem });
    }

    public event StackViewItemMouseButtonEventHandler? ItemPreviewMouseLeftButtonDown;
    public event StackViewItemMouseButtonEventHandler? ItemPreviewMouseLeftButtonUp;
    public event StackViewItemMouseButtonEventHandler? ItemMouseLeftButtonDown;
    public event StackViewItemMouseButtonEventHandler? ItemMouseLeftButtonUp;

    internal void NotifyListItemPreviewMouseLeftButtonDown(StackViewBaseItem item, MouseButtonEventArgs e)
    {
        var clickedItem = ItemContainerGenerator.ItemFromContainer(item);
        ItemPreviewMouseLeftButtonDown?.Invoke(this, new StackViewItemMouseButtonEventArgs(item, e));
    }

    internal void NotifyListItemPreviewMouseLeftButtonUp(StackViewBaseItem item, MouseButtonEventArgs e)
    {
        var clickedItem = ItemContainerGenerator.ItemFromContainer(item);
        ItemPreviewMouseLeftButtonUp?.Invoke(this, new StackViewItemMouseButtonEventArgs(item, e));
    }

    internal void NotifyListItemMouseLeftButtonDown(StackViewBaseItem item, MouseButtonEventArgs e)
    {
        var clickedItem = ItemContainerGenerator.ItemFromContainer(item);
        ItemMouseLeftButtonDown?.Invoke(this, new StackViewItemMouseButtonEventArgs(item, e));
    }

    internal void NotifyListItemMouseLeftButtonUp(StackViewBaseItem item, MouseButtonEventArgs e)
    {
        var clickedItem = ItemContainerGenerator.ItemFromContainer(item);
        ItemMouseLeftButtonUp?.Invoke(this, new StackViewItemMouseButtonEventArgs(item, e));
    }

    public event StackViewItemMouseButtonEventHandler? ItemPreviewMouseRightButtonDown;
    public event StackViewItemMouseButtonEventHandler? ItemPreviewMouseRightButtonUp;
    public event StackViewItemMouseButtonEventHandler? ItemMouseRightButtonDown;
    public event StackViewItemMouseButtonEventHandler? ItemMouseRightButtonUp;

    internal void NotifyListItemPreviewMouseRightButtonDown(StackViewBaseItem item, MouseButtonEventArgs e)
    {
        var clickedItem = ItemContainerGenerator.ItemFromContainer(item);
        ItemPreviewMouseRightButtonDown?.Invoke(this, new StackViewItemMouseButtonEventArgs(item, e));
    }

    internal void NotifyListItemPreviewMouseRightButtonUp(StackViewBaseItem item, MouseButtonEventArgs e)
    {
        var clickedItem = ItemContainerGenerator.ItemFromContainer(item);
        ItemPreviewMouseRightButtonUp?.Invoke(this, new StackViewItemMouseButtonEventArgs(item, e));
    }

    internal void NotifyListItemMouseRightButtonDown(StackViewBaseItem item, MouseButtonEventArgs e)
    {
        var clickedItem = ItemContainerGenerator.ItemFromContainer(item);
        ItemMouseRightButtonDown?.Invoke(this, new StackViewItemMouseButtonEventArgs(item, e));
    }

    internal void NotifyListItemMouseRightButtonUp(StackViewBaseItem item, MouseButtonEventArgs e)
    {
        var clickedItem = ItemContainerGenerator.ItemFromContainer(item);
        ItemMouseRightButtonUp?.Invoke(this, new StackViewItemMouseButtonEventArgs(item, e));
    }
}
