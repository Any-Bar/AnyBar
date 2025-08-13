using System.Windows;
using System.Windows.Controls;

namespace Flow.Bar.Controls;

internal static class SelectorHelper
{
    internal static bool ItemGetIsSelectable(object item)
    {
        if (item != null)
        {
            return !(item is Separator);
        }

        return false;
    }

    internal static bool UiGetIsSelectable(DependencyObject o)
    {
        if (o != null)
        {
            if (!ItemGetIsSelectable(o))
            {
                return false;
            }
            else
            {
                // Check the data item
                var itemsControl = ItemsControl.ItemsControlFromItemContainer(o);
                if (itemsControl != null)
                {
                    var data = itemsControl.ItemContainerGenerator.ItemFromContainer(o);
                    if (data != o)
                    {
                        return ItemGetIsSelectable(data);
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
