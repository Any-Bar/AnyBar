using System.Windows;

namespace Flow.Bar.Controls;

public delegate void ItemClickEventHandler(object sender, StackViewItemClickEventArgs e);

public sealed class StackViewItemClickEventArgs : RoutedEventArgs
{
    public StackViewItemClickEventArgs()
    {
    }

    public object ClickedItem { get; internal set; } = null!;
}
