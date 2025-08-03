using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;

namespace Flow.Bar.Controls.ListView;

public class ListViewEx : ListViewBase
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
        return item is ListViewItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ListViewItem();
    }
}
