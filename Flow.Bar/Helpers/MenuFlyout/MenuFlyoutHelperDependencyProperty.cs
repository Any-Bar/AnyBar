using System;
using System.Windows;

namespace Flow.Bar.Helpers.MenuFlyout;

public class MenuFlyoutHelperDependencyProperty
{
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
