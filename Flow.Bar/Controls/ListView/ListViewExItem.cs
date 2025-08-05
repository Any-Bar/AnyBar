using System.Windows;

namespace Flow.Bar.Controls;

public class ListViewExItem : ListViewExBaseItem
{
    static ListViewExItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ListViewExItem), new FrameworkPropertyMetadata(typeof(ListViewExItem)));
    }

    public ListViewExItem()
    {

    }
}
