using System;
using System.Collections.ObjectModel;

namespace Flow.Bar.Extensions.Enumerable;

public static class EnumerableExtension
{
    public static int RemoveAll<T>(this ObservableCollection<T> collection, Predicate<T> match)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(match);

        var removedCount = 0;
        for (var i = collection.Count - 1; i >= 0; i--)
        {
            if (match(collection[i]))
            {
                collection.RemoveAt(i);
                removedCount++;
            }
        }
        return removedCount;
    }
}
