using System.Windows.Input;

namespace Flow.Bar.Controls;

public delegate void StackViewItemMouseButtonEventHandler(object sender, StackViewItemMouseButtonEventArgs e);

public sealed class StackViewItemMouseButtonEventArgs(object item, MouseButtonEventArgs e) : MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton, e.StylusDevice)
{
    public new int ClickCount { get; internal set; } = e.ClickCount;

    public object ClickedItem { get; internal set; } = item;
}
