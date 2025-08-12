using System.Windows;

namespace Flow.Bar.Controls;

public delegate void ListViewExItemClickEventHandler(object sender, ListViewExItemClickEventArgs e);

public sealed class ListViewExItemClickEventArgs : RoutedEventArgs
{
    public ListViewExItemClickEventArgs()
    {
    }

    public object ClickedItem { get; internal set; } = null!;
}
