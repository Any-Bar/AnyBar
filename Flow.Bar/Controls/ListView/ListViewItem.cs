using System.Windows;
using iNKORE.UI.WPF.Modern.Controls;

namespace Flow.Bar.Controls.ListView;

public class ListViewItemEx : ListViewBaseItem
{
    static ListViewItemEx()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ListViewItemEx), new FrameworkPropertyMetadata(typeof(ListViewItemEx)));
    }
}
