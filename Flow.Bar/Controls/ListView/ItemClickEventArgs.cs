using System.Windows;

namespace Flow.Bar.Controls.ListView;

public delegate void ItemClickEventHandler(object sender, ItemClickEventArgs e);

public sealed class ItemClickEventArgs : RoutedEventArgs
{
    public ItemClickEventArgs()
    {
    }

    public object ClickedItem { get; internal set; } = null!;
}
