using iNKORE.UI.WPF.Modern.Controls.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace Flow.Bar.Controls;

public class StackPanelViewExBase : ListBox
{
    protected StackPanelViewExBase()
    {
    }

    #region UseSystemFocusVisuals

    /// <summary>
    /// Identifies the UseSystemFocusVisuals dependency property.
    /// </summary>
    public static readonly DependencyProperty UseSystemFocusVisualsProperty =
        FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(StackPanelViewExBase));

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
        FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(StackPanelViewExBase));

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
        ControlHelper.CornerRadiusProperty.AddOwner(typeof(StackPanelViewExBase));

    /// <summary>
    /// Gets or sets the radius for the corners of the control's border.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    public event ItemClickEventHandler? ItemClick;

    internal void NotifyListItemClicked(StackPanelViewExBaseItem item)
    {
        var clickedItem = ItemContainerGenerator.ItemFromContainer(item);
        ItemClick?.Invoke(this, new StackPanelViewExItemClickEventArgs { ClickedItem = clickedItem });
    }
}
