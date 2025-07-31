using System.Collections.Generic;

namespace Flow.Bar.Controls.NavigationView;

internal static class ListExtensions
{
    public static T Last<T>(this List<T> list)
    {
        return list[^1];
    }

    public static void RemoveLast<T>(this List<T> list)
    {
        list.RemoveAt(list.Count - 1);
    }

    public static bool Empty<T>(this List<T> list)
    {
        return list.Count == 0;
    }
}
