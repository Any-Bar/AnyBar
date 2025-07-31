using System.Windows;

namespace Flow.Bar.Controls.NavigationView;

internal sealed class TappedRoutedEventArgs : RoutedEventArgs
{
    internal int Timestamp { get; set; }
}
