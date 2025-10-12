using System.Windows;

namespace AnyBar.Controls;

internal sealed class TappedRoutedEventArgs : RoutedEventArgs
{
    internal int Timestamp { get; set; }
}
