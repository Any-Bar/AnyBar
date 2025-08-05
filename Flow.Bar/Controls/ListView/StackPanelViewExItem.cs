using System.Windows;

namespace Flow.Bar.Controls;

public class StackPanelViewExItem : StackPanelViewExBaseItem
{
    static StackPanelViewExItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StackPanelViewExItem), new FrameworkPropertyMetadata(typeof(StackPanelViewExItem)));
    }

    public StackPanelViewExItem()
    {
    }
}
