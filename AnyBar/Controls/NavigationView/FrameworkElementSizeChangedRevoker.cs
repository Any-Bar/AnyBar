using System.Windows;

namespace AnyBar.Controls;

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
