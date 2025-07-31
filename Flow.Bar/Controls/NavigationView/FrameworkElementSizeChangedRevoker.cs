using System.Windows;

namespace Flow.Bar.Controls.NavigationView;

internal class FrameworkElementSizeChangedRevoker(FrameworkElement source, SizeChangedEventHandler handler) : EventRevoker<FrameworkElement, SizeChangedEventHandler>(source, handler)
{
    protected override void AddHandler(FrameworkElement source, SizeChangedEventHandler handler)
    {
        source.SizeChanged += handler;
    }

    protected override void RemoveHandler(FrameworkElement source, SizeChangedEventHandler handler)
    {
        source.SizeChanged -= handler;
    }
}
