using iNKORE.UI.WPF.Modern.Controls.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Flow.Bar.Controls;

public class StackViewBaseItem : ListBoxItem
{
    protected StackViewBaseItem()
    {
    }

    #region UseSystemFocusVisuals

    public static readonly DependencyProperty UseSystemFocusVisualsProperty =
        FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(StackViewBaseItem));

    public bool UseSystemFocusVisuals
    {
        get => (bool)GetValue(UseSystemFocusVisualsProperty);
        set => SetValue(UseSystemFocusVisualsProperty, value);
    }

    #endregion

    #region FocusVisualMargin

    public static readonly DependencyProperty FocusVisualMarginProperty =
        FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(StackViewBaseItem));

    public Thickness FocusVisualMargin
    {
        get => (Thickness)GetValue(FocusVisualMarginProperty);
        set => SetValue(FocusVisualMarginProperty, value);
    }

    #endregion

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty =
        ControlHelper.CornerRadiusProperty.AddOwner(typeof(StackViewBaseItem));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (!e.Handled)
        {
            m_isPressed = true;
        }
        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!e.Handled)
        {
            HandleItemClick(e);
            m_isPressed = false;
        }
        base.OnMouseLeftButtonUp(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        if (!e.Handled)
        {
            m_isPressed = false;
        }
        base.OnMouseLeave(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Enter)
        {
            OnClick();
            e.Handled = true;
        }
    }

    private void HandleItemClick(MouseButtonEventArgs e)
    {
        if (m_isPressed)
        {
            Rect r = new(new Point(), RenderSize);

            if (r.Contains(e.GetPosition(this)))
            {
                OnClick();
            }
        }
    }

    private void OnClick()
    {
        ParentStackPanelViewBase?.NotifyListItemClicked(this);
    }

    private StackViewBase? ParentStackPanelViewBase => ItemsControl.ItemsControlFromItemContainer(this) as StackViewBase;

    private bool m_isPressed;
}
