using iNKORE.UI.WPF.Modern.Controls.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Flow.Bar.Controls;

public class StackPanelViewExBaseItem : ListBoxItem
{
    protected StackPanelViewExBaseItem()
    {
    }

    #region UseSystemFocusVisuals

    public static readonly DependencyProperty UseSystemFocusVisualsProperty =
        FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(StackPanelViewExBaseItem));

    public bool UseSystemFocusVisuals
    {
        get => (bool)GetValue(UseSystemFocusVisualsProperty);
        set => SetValue(UseSystemFocusVisualsProperty, value);
    }

    #endregion

    #region FocusVisualMargin

    public static readonly DependencyProperty FocusVisualMarginProperty =
        FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(StackPanelViewExBaseItem));

    public Thickness FocusVisualMargin
    {
        get => (Thickness)GetValue(FocusVisualMarginProperty);
        set => SetValue(FocusVisualMarginProperty, value);
    }

    #endregion

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty =
        ControlHelper.CornerRadiusProperty.AddOwner(typeof(StackPanelViewExBaseItem));

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
            HandleMouseUp(e);
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

    private void HandleMouseUp(MouseButtonEventArgs e)
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

    private StackPanelViewExBase? ParentStackPanelViewBase => ItemsControl.ItemsControlFromItemContainer(this) as StackPanelViewExBase;

    private bool m_isPressed;
}
