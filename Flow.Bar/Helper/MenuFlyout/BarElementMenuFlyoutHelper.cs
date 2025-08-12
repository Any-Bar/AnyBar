using System;
using System.Windows;

namespace Flow.Bar.Helper.MenuFlyout;

public class BarElementMenuFlyoutHelper
{
    public static readonly DependencyProperty MenuFlyoutHelperProperty =
        DependencyProperty.RegisterAttached(
            "MenuFlyoutHelper",
            typeof(AppBarMenuFlyoutHelper),
            typeof(BarElementMenuFlyoutHelper),
            new PropertyMetadata(null));

    [AttachedPropertyBrowsableForType(typeof(UIElement))]
    public static AppBarMenuFlyoutHelper? GetMenuFlyoutHelper(FrameworkElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        return (AppBarMenuFlyoutHelper?)element.GetValue(MenuFlyoutHelperProperty);
    }

    [AttachedPropertyBrowsableForType(typeof(UIElement))]
    public static void SetMenuFlyoutHelper(FrameworkElement element, AppBarMenuFlyoutHelper value)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentNullException.ThrowIfNull(value);

        element.SetValue(MenuFlyoutHelperProperty, value);
        value.Element = element;
    }
}
