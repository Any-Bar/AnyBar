using System.Windows;
using System.Windows.Input;

namespace AnyBar.Controls;

public delegate void StackViewItemMouseButtonEventHandler(object sender, StackViewItemMouseButtonEventArgs e);

public sealed class StackViewItemMouseButtonEventArgs(object item, MouseButtonEventArgs e) : RoutedEventArgs
{
    public int ClickCount { get; internal set; } = e.ClickCount;

    public object ClickedItem { get; internal set; } = item;

    public MouseButtonEventArgs OriginalEventArgs { get; } = e;
}
