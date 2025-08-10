using Flow.Bar.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
            if (dictionary.Remove(oldOrder + i, out var model))
            {
                model.Order = newOrder + i;
                itemsToMove.Add(model);
            }
        }

        if (oldOrder < newOrder)
        {
            // Shift down
            for (var i = oldOrder + itemsCount; i <= newOrder; i++)
            {
                if (dictionary.Remove(i, out var model))
                {
                    model.Order -= itemsCount;
                    itemsToMove.Add(model);
                }
            }
        }
        else
        {
            // Shift up
            for (var i = newOrder; i < oldOrder; i++)
            {
                if (dictionary.Remove(i, out var model))
                {
                    model.Order += itemsCount;
                    itemsToMove.Add(model);
                }
            }
        }

        foreach (var model in itemsToMove.OrderBy(m => m.Order))
        {
            dictionary.TryAdd(model.Order, model);
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
        if (oldOrder < newOrder)
        {
            newOrder -= itemsCount;
        }

        // Insert items at new position
        list.InsertRange(newOrder, itemsToMove);

        // Update Order property
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] is IOrder orderItem)
            {
                orderItem.Order = i;
            }
        }

        return true;
    }

    public static bool Move<T>(this ObservableCollection<T> collection, int oldOrder, int newOrder, int itemsCount)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(collection);

        // Validate necessity
        if (oldOrder == newOrder || itemsCount == 0)
        {
            return false;
        }

        // If moving forward, adjust newOrder to account for removal
        if (newOrder > oldOrder)
        {
            newOrder -= itemsCount;
        }

        // Extract items to move
        var itemsToMove = new List<T>();
        for (int i = 0; i < itemsCount; i++)
        {
            itemsToMove.Add(collection[oldOrder]);
            collection.RemoveAt(oldOrder);
        }

        // Insert at new position
        for (int i = 0; i < itemsToMove.Count; i++)
        {
            collection.Insert(newOrder + i, itemsToMove[i]);
        }

        return true;
    }
}
