using System.Windows;
using System.Windows.Input;

namespace Flow.Bar.Controls;

public delegate void ItemClickEventHandler(object sender, StackViewItemClickEventArgs e);

public sealed class StackViewItemClickEventArgs : RoutedEventArgs
{
    public StackViewItemClickEventArgs()
    {
    }

    public object ClickedItem { get; internal set; } = null!;
}

public delegate void ItemMouseButtonEventHandler(object sender, StackViewItemMouseButtonEventArgs e);

public sealed class StackViewItemMouseButtonEventArgs(object item, MouseButtonEventArgs e) : MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton, e.StylusDevice)
{
    public new int ClickCount { get; internal set; } = e.ClickCount;

    public object ClickedItem { get; internal set; } = item;
}
