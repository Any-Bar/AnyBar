using System.Windows;

namespace Flow.Bar.Controls.ListView;

public class ListViewExItem : ListViewExBaseItem
{
    static ListViewExItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ListViewExItem), new FrameworkPropertyMetadata(typeof(ListViewExItem)));
    }
}
