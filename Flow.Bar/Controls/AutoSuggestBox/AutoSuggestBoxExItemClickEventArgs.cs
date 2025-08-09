using System.Windows;

namespace Flow.Bar.Controls;

public delegate void AutoSuggestBoxExItemClickEventHandler(object sender, AutoSuggestBoxExItemClickEventArgs e);

public sealed class AutoSuggestBoxExItemClickEventArgs : RoutedEventArgs
{
    public AutoSuggestBoxExItemClickEventArgs()
    {
    }

    public object ClickedItem { get; internal set; } = null!;
}
