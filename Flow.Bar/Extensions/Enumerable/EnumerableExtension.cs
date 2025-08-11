using Flow.Bar.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Flow.Bar.Extensions.Enumerable;

public static class EnumerableExtension
{
    public static int GetMax(this ICollection<int> list)
    {
        return list.Count != 0 ? list.Max() + 1 : 0;
    }

    public static int GetMax<T>(this IList<T> list, Func<T, int> selector)
    {
        return list.Count != 0 ? list.Max(selector) + 1 : 0;
    }

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

    public static bool Move<T>(this Dictionary<int, T> dictionary, int oldOrder, int newOrder, int itemsCount)
        where T : class, IOrder
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        // Validate necessity
        if (oldOrder == newOrder || itemsCount == 0)
        {
            return false;
        }

        var itemsToMove = new List<T>();

        // Change item order
        for (var i = 0; i < itemsCount; i++)
        {
            var key = oldOrder + i;
            if (dictionary.Remove(key, out var model))
            {
                itemsToMove.Add(model);
            }
        }

        // Shift affected range
        if (oldOrder < newOrder)
        {
            // Shift down in reverse order to avoid overwriting
            for (var i = newOrder; i >= oldOrder + itemsCount; i--)
            {
                if (dictionary.Remove(i, out var model))
                {
                    model.Order -= itemsCount;
                    dictionary[i - itemsCount] = model;
                }
            }

            // Insert moved items
            for (var i = 0; i < itemsToMove.Count; i++)
            {
                var insertedKey = newOrder + 1 - itemsToMove.Count + i;
                itemsToMove[i].Order = insertedKey;
                dictionary[insertedKey] = itemsToMove[i];
            }
        }
        else
        {
            // Shift up in ascending order
            for (var i = oldOrder - 1; i >= newOrder; i--)
            {
                if (dictionary.Remove(i, out var model))
                {
                    model.Order += itemsCount;
                    dictionary[i + itemsCount] = model;
                }
            }

            // Insert moved items
            for (var i = 0; i < itemsToMove.Count; i++)
            {
                var insertedKey = newOrder + i;
                itemsToMove[i].Order = insertedKey;
                dictionary[insertedKey] = itemsToMove[i];
            }
        }

        return true;
    }

    public static bool Move<T>(this List<T> list, int oldOrder, int newOrder, int itemsCount)
        where T : class, IOrder
    {
        ArgumentNullException.ThrowIfNull(list);

        // Validate necessity
        if (oldOrder == newOrder || itemsCount == 0)
        {
            return false;
        }

        // Extract items to move
        var itemsToMove = list.GetRange(oldOrder, itemsCount);
        list.RemoveRange(oldOrder, itemsCount);

        // Adjust newOrder if items are removed before it
        int insertedOrder;
        int toIndex;
        if (oldOrder < newOrder)
        {
            insertedOrder = newOrder - itemsCount + 1;
            toIndex = newOrder + 1;
        }
        else
        {
            insertedOrder = newOrder;
            toIndex = oldOrder + itemsCount;
        }

        // Insert items at new position
        list.InsertRange(insertedOrder, itemsToMove);

        // Update Order property
        var fromIndex = Math.Min(oldOrder, newOrder);
        for (var i = fromIndex; i < toIndex; i++)
        {
            list[i].Order = i;
        }

        return true;
    }

    public static bool Move<T>(this ObservableCollection<T> collection, int oldOrder, int newOrder, int itemsCount)
        where T : class, IOrder
    {
        ArgumentNullException.ThrowIfNull(collection);

        // Validate necessity
        if (oldOrder == newOrder || itemsCount == 0)
        {
            return false;
        }

        // Extract items to move
        var itemsToMove = new List<T>();
        for (var i = 0; i < itemsCount; i++)
        {
            itemsToMove.Add(collection[oldOrder]);
            collection.RemoveAt(oldOrder);
        }

        // Adjust newOrder if items are removed before it
        int insertedOrder;
        int toIndex;
        if (oldOrder < newOrder)
        {
            insertedOrder = newOrder - itemsCount + 1;
            toIndex = newOrder + 1;
        }
        else
        {
            insertedOrder = newOrder;
            toIndex = oldOrder + itemsCount;
        }

        // Insert at new position
        for (var i = 0; i < itemsToMove.Count; i++)
        {
            collection.Insert(insertedOrder + i, itemsToMove[i]);
        }

        // Update Order property
        var fromIndex = Math.Min(oldOrder, newOrder);
        for (var i = fromIndex; i < toIndex; i++)
        {
            collection[i].Order = i;
        }

        return true;
    }
}
