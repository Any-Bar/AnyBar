using System.Windows;
using System.Windows.Input;

namespace Flow.Bar.Controls.NavigationView;

internal static class InputHelper
{
    public static readonly DependencyProperty IsTapEnabledProperty = DependencyProperty.RegisterAttached("IsTapEnabled", typeof(bool), typeof(InputHelper), new PropertyMetadata(false, OnIsTapEnabledChanged));

    public static readonly DependencyProperty IsPressedProperty = DependencyProperty.RegisterAttached("IsPressed", typeof(bool), typeof(InputHelper), new PropertyMetadata(false));

    public static readonly RoutedEvent TappedEvent = EventManager.RegisterRoutedEvent("Tapped", RoutingStrategy.Bubble, typeof(TappedEventHandler), typeof(InputHelper));

    private static TappedRoutedEventArgs? _lastTappedArgs;

    public static bool GetIsTapEnabled(UIElement element)
    {
        return (bool)element.GetValue(IsTapEnabledProperty);
    }

    public static void SetIsTapEnabled(UIElement element, bool value)
    {
        element.SetValue(IsTapEnabledProperty, value);
    }

    private static void OnIsTapEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UIElement uIElement = (UIElement)d;
        _ = (bool)e.OldValue;
        if ((bool)e.NewValue)
        {
            uIElement.MouseLeftButtonDown += OnMouseLeftButtonDown;
            uIElement.MouseLeftButtonUp += OnMouseLeftButtonUp;
            uIElement.LostMouseCapture += OnLostMouseCapture;
            uIElement.MouseLeave += OnMouseLeave;
        }
        else
        {
            uIElement.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            uIElement.MouseLeftButtonUp -= OnMouseLeftButtonUp;
            uIElement.LostMouseCapture -= OnLostMouseCapture;
            uIElement.MouseLeave -= OnMouseLeave;
        }
    }

    private static bool GetIsPressed(UIElement element)
    {
        return (bool)element.GetValue(IsPressedProperty);
    }

    private static void SetIsPressed(UIElement element, bool value)
    {
        if (value)
        {
            element.SetValue(IsPressedProperty, value);
        }
        else
        {
            element.ClearValue(IsPressedProperty);
        }
    }

    public static void AddTappedHandler(UIElement element, TappedEventHandler handler)
    {
        element.AddHandler(TappedEvent, handler);
    }

    public static void RemoveTappedHandler(UIElement element, TappedEventHandler handler)
    {
        element.RemoveHandler(TappedEvent, handler);
    }

    private static void RaiseTapped(UIElement element, int timestamp)
    {
        element.RaiseEvent(_lastTappedArgs = new TappedRoutedEventArgs
        {
            RoutedEvent = TappedEvent,
            Source = element,
            Timestamp = timestamp
        });
    }

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        UIElement element = (UIElement)sender;
        if (!GetIsPressed(element))
        {
            SetIsPressed(element, value: true);
        }
    }

    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        UIElement uIElement = (UIElement)sender;
        if (!GetIsPressed(uIElement))
        {
            return;
        }

        SetIsPressed((UIElement)sender, value: false);
        TappedRoutedEventArgs? lastTappedArgs = _lastTappedArgs;
        if (lastTappedArgs == null || !lastTappedArgs.Handled || lastTappedArgs.Timestamp != e.Timestamp)
        {
            Rect rect = new(default, uIElement.RenderSize);
            if (rect.Contains(e.GetPosition(uIElement)))
            {
                RaiseTapped(uIElement, e.Timestamp);
            }
        }
    }

    private static void OnLostMouseCapture(object sender, MouseEventArgs e)
    {
        SetIsPressed((UIElement)sender, value: false);
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        SetIsPressed((UIElement)sender, value: false);
    }
}
