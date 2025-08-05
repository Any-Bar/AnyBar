using System.Windows;

namespace Flow.Bar.Controls;

public class ListViewEx : ListViewExBase
{
    static ListViewEx()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ListViewEx), new FrameworkPropertyMetadata(typeof(ListViewEx)));
    }

    public ListViewEx()
    {

    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is ListViewExItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ListViewExItem();
    }
}
