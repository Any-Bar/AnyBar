using System;
using System.Windows;

namespace AnyBar.Helpers.MenuFlyout;

public class MenuFlyoutHelperDependencyProperty
{
    public static readonly DependencyProperty LeftClickMenuFlyoutHelperProperty =
        DependencyProperty.RegisterAttached(
            "LeftClickMenuFlyoutHelper",
            typeof(AppBarMenuFlyoutHelper),
            typeof(MenuFlyoutHelperDependencyProperty),
            new PropertyMetadata(null));

    [AttachedPropertyBrowsableForType(typeof(UIElement))]
    public static AppBarMenuFlyoutHelper? GetLeftClickMenuFlyoutHelper(FrameworkElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        return (AppBarMenuFlyoutHelper?)element.GetValue(LeftClickMenuFlyoutHelperProperty);
    }

    [AttachedPropertyBrowsableForType(typeof(UIElement))]
    public static void SetLeftClickMenuFlyoutHelper(FrameworkElement element, AppBarMenuFlyoutHelper value)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentNullException.ThrowIfNull(value);

        element.SetValue(LeftClickMenuFlyoutHelperProperty, value);
    }

    public static readonly DependencyProperty RightClickMenuFlyoutHelperProperty =
        DependencyProperty.RegisterAttached(
            "RightClickMenuFlyoutHelper",
            typeof(AppBarMenuFlyoutHelper),
            typeof(MenuFlyoutHelperDependencyProperty),
            new PropertyMetadata(null));

    [AttachedPropertyBrowsableForType(typeof(UIElement))]
    public static AppBarMenuFlyoutHelper? GetRightClickMenuFlyoutHelper(FrameworkElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        return (AppBarMenuFlyoutHelper?)element.GetValue(RightClickMenuFlyoutHelperProperty);
    }

    [AttachedPropertyBrowsableForType(typeof(UIElement))]
    public static void SetRightClickMenuFlyoutHelper(FrameworkElement element, AppBarMenuFlyoutHelper value)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentNullException.ThrowIfNull(value);

        element.SetValue(RightClickMenuFlyoutHelperProperty, value);
    }
}
