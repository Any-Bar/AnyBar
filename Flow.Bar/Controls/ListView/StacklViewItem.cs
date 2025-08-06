using System.Windows;

namespace Flow.Bar.Controls;

public class StackViewItem : StackViewBaseItem
{
    static StackViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StackViewItem), new FrameworkPropertyMetadata(typeof(StackViewItem)));
    }

    public StackViewItem()
    {
    }
}
