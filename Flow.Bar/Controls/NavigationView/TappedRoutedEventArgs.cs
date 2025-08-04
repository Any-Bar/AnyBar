using System.Windows;

namespace Flow.Bar.Controls;

internal sealed class TappedRoutedEventArgs : RoutedEventArgs
{
    internal int Timestamp { get; set; }
}
