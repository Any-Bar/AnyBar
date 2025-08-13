using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using iNKORE.UI.WPF.Modern.Controls.Helpers;

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

    #region IsPressed

    public static readonly DependencyProperty IsPressedProperty =
        DependencyProperty.Register(
            nameof(IsPressed),
            typeof(bool),
            typeof(StackViewBaseItem),
            new FrameworkPropertyMetadata(false));

    public bool IsPressed
    {
        get => (bool)GetValue(IsPressedProperty);
        set => SetValue(IsPressedProperty, value);
    }

    #endregion

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (!e.Handled)
        {
            ParentStackPanelViewBase?.NotifyListItemPreviewMouseLeftButtonDown(this, e);
        }

        base.OnPreviewMouseLeftButtonDown(e);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (!e.Handled)
        {
            IsPressed = true;
            m_isPressed = true;
            ParentStackPanelViewBase?.NotifyListItemMouseLeftButtonDown(this, e);
        }

        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!e.Handled)
        {
            ParentStackPanelViewBase?.NotifyListItemPreviewMouseLeftButtonUp(this, e);
        }

        base.OnPreviewMouseLeftButtonUp(e);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!e.Handled)
        {
            IsPressed = false;
            HandleItemClick(e);
            m_isPressed = false;
            ParentStackPanelViewBase?.NotifyListItemMouseLeftButtonUp(this, e);
        }

        base.OnMouseLeftButtonUp(e);
    }

    protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
    {
        if (!e.Handled)
        {
            ParentStackPanelViewBase?.NotifyListItemMouseRightButtonDown(this, e);
        }

        base.OnMouseRightButtonDown(e);
    }

    protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
    {
        if (!e.Handled)
        {
            ParentStackPanelViewBase?.NotifyListItemMouseRightButtonUp(this, e);
        }

        base.OnMouseRightButtonUp(e);
    }

    protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
    {
        if (!e.Handled)
        {
            ParentStackPanelViewBase?.NotifyListItemPreviewMouseRightButtonDown(this, e);
        }

        base.OnPreviewMouseRightButtonDown(e);
    }

    protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
    {
        if (!e.Handled)
        {
            ParentStackPanelViewBase?.NotifyListItemPreviewMouseRightButtonUp(this, e);
        }

        base.OnPreviewMouseRightButtonUp(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        if (!e.Handled)
        {
            IsPressed = false;
            m_isPressed = false;
        }

        base.OnMouseLeave(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Enter)
        {
            ParentStackPanelViewBase?.NotifyListItemClicked(this);
            e.Handled = true;
        }
    }

    private void HandleItemClick(MouseButtonEventArgs e)
    {
        if (m_isPressed)
        {
            var r = new Rect(new Point(), RenderSize);

            if (r.Contains(e.GetPosition(this)))
            {
                ParentStackPanelViewBase?.NotifyListItemClicked(this);
            }
        }
    }

    private StackViewBase? ParentStackPanelViewBase => ItemsControl.ItemsControlFromItemContainer(this) as StackViewBase;

    private bool m_isPressed;
}
