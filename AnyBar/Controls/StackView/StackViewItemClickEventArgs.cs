using System.Windows;

namespace AnyBar.Controls;

public delegate void StackViewItemClickEventHandler(object sender, StackViewItemClickEventArgs e);

public sealed class StackViewItemClickEventArgs : RoutedEventArgs
{
    public StackViewItemClickEventArgs()
    {
    }

    public object ClickedItem { get; internal set; } = null!;
}
