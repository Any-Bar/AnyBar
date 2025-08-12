using Flow.Bar.Helper.MenuFlyout;
using Flow.Bar.Helper.Plugins;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Plugin;
using Flow.Bar.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Flow.Bar.Controls;

public class StackViewItem : StackViewBaseItem
{
    private readonly string ClassName = nameof(StackViewItem);

    private AppBarMenuFlyoutHelper? _rightMenuFlyoutHelper;
    private bool _rightMenuFlyoutHelperInitialized = false;

    static StackViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StackViewItem), new FrameworkPropertyMetadata(typeof(StackViewItem)));
    }

    public StackViewItem()
    {
    }

    private AppBarMenuFlyoutHelper? GetRightMenuFlyoutHelper()
    {
        if (_rightMenuFlyoutHelperInitialized) return _rightMenuFlyoutHelper;

        ArgumentNullException.ThrowIfNull(Model);
        ArgumentNullException.ThrowIfNull(AppBarViewModel);

        if (PluginManager.GetRightClickMenu(Model.ID) is IRightClickMenu rightClickMenu)
        {
            _rightMenuFlyoutHelper = new();
            foreach (var menuItem in rightClickMenu.GetRightClickMenuItems())
            {
                _rightMenuFlyoutHelper.Items.Add(menuItem);
            }
            _rightMenuFlyoutHelper.ViewModel = AppBarViewModel;
            _rightMenuFlyoutHelper.Element = this;
            _rightMenuFlyoutHelper.Handled = true;
        }
        _rightMenuFlyoutHelperInitialized = true;
        return _rightMenuFlyoutHelper;
    }

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

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (!e.Handled)
        {
            IsPressed = true;
            App.API.LogVerbose(ClassName, $"Prepare {nameof(StackViewItem)} left click context menu");
            // TODO
            e.Handled = true;
        }

        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!e.Handled)
        {
            IsPressed = false;
            App.API.LogVerbose(ClassName, $"Show {nameof(StackViewItem)} left click context menu");
            // TODO
            e.Handled = true;
        }

        base.OnMouseLeftButtonUp(e);
    }

    protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
    {
        if (!e.Handled)
        {
            App.API.LogVerbose(ClassName, $"Prepare {nameof(StackViewItem)} right click context menu");
            GetRightMenuFlyoutHelper()?.MouseRightButtonDown(this, e);
        }

        base.OnPreviewMouseRightButtonDown(e);
    }

    protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
    {
        if (!e.Handled)
        {
            App.API.LogVerbose(ClassName, $"Show {nameof(StackViewItem)} right click context menu");
            GetRightMenuFlyoutHelper()?.MouseRightButtonUp(this, e);
        }

        base.OnPreviewMouseRightButtonUp(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        if (!e.Handled)
        {
            IsPressed = false;
        }

        base.OnMouseLeave(e);
    }

    private StackViewBase? ParentStackPanelViewBase => ItemsControl.ItemsControlFromItemContainer(this) as StackViewBase;

    private AppBarViewModel? AppBarViewModel => ParentStackPanelViewBase?.DataContext as AppBarViewModel;

    private BarElementModel? Model => DataContext as BarElementModel;
}
