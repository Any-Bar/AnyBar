using System.Windows;

namespace Flow.Bar.Controls;

public delegate void ItemClickEventHandler(object sender, StackPanelViewExItemClickEventArgs e);

public sealed class StackPanelViewExItemClickEventArgs : RoutedEventArgs
{
    public StackPanelViewExItemClickEventArgs()
    {
    }

    public object ClickedItem { get; internal set; } = null!;
}
