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

    /// <summary>
    /// Remove an item with a specific Order from a dictionary and update the Order property of remaining items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="dictionary1"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public static bool RemoveOrder<T, T1>(this Dictionary<int, T> dictionary, int order, Dictionary<int, T1> dictionary1, Action<T1> action)
        where T : class, IOrder
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        if (!dictionary.Remove(order, out var _))
        {
            return false;
        }

        // Invoke action on the corresponding item in dictionary1
        dictionary1.Remove(order, out var _);

        // Find the maximum index which is less than the order
        var smallerItems = dictionary.Keys.Where(k => k < order).Order().ToList();
        var index = smallerItems.Count > 0 ? smallerItems.Last() + 1 : 0;

        // Update Order property for remaining items
        foreach (var key in dictionary.Keys.Where(k => k > order).Order().ToList())
        {
            var model = dictionary[key];
            model.Order = index;
            dictionary[index] = model;
            dictionary.Remove(key);

            // Invoke action on the corresponding item in dictionary1
            if (dictionary1.Remove(key, out var value))
            {
                dictionary1[index] = value;
                action?.Invoke(value);
            }

            index++;
        }

        return true;
    }

    /// <summary>
    /// Remove an item with a specific Order from a dictionary and update the Order property of remaining items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public static bool RemoveOrder<T>(this List<T> list, int order)
        where T : class, IOrder
    {
        ArgumentNullException.ThrowIfNull(list);

        if (list.RemoveAll(x => x.Order == order) == 0)
        {
            return false;
        }

        // Update Order property for remaining items
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].Order > order)
            {
                list[i].Order--;
            }
        }

        return true;
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

    /// <summary>
    /// Move a range of items in a dictionary to a new position and update their Order property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="oldIndex"></param>
    /// <param name="newIndex"></param>
    /// <param name="itemsCount"></param>
    /// <returns></returns>
    public static bool Move<T>(this Dictionary<int, T> dictionary, int oldIndex, int newIndex, int itemsCount)
        where T : class, IOrder
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        // Validate necessity
        if (oldIndex == newIndex || itemsCount == 0)
        {
            return false;
        }

        // Change item order
        var itemsToMove = new List<T>();
        for (var i = 0; i < itemsCount; i++)
        {
            var key = oldIndex + i;
            if (dictionary.Remove(key, out var model))
            {
                itemsToMove.Add(model);
            }
        }

        // Shift affected range
        if (oldIndex < newIndex)
        {
            // Shift down in reverse order to avoid overwriting
            for (var i = newIndex; i >= oldIndex + itemsCount; i--)
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
                var insertedKey = newIndex + 1 - itemsToMove.Count + i;
                itemsToMove[i].Order = insertedKey;
                dictionary[insertedKey] = itemsToMove[i];
            }
        }
        else
        {
            // Shift up in ascending order
            for (var i = oldIndex - 1; i >= newIndex; i--)
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
                var insertedKey = newIndex + i;
                itemsToMove[i].Order = insertedKey;
                dictionary[insertedKey] = itemsToMove[i];
            }
        }

        return true;
    }

    /// <summary>
    /// Move a range of items in a list to a new position and update their Order property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="oldIndex"></param>
    /// <param name="newIndex"></param>
    /// <param name="itemsCount"></param>
    /// <returns></returns>
    public static bool Move<T>(this List<T> list, int oldIndex, int newIndex, int itemsCount)
        where T : class, IOrder
    {
        ArgumentNullException.ThrowIfNull(list);

        // Validate necessity
        if (oldIndex == newIndex || itemsCount == 0)
        {
            return false;
        }

        // Extract items to move
        var itemsToMove = list.GetRange(oldIndex, itemsCount);
        list.RemoveRange(oldIndex, itemsCount);

        // Adjust newOrder if items are removed before it
        int insertedOrder;
        int toIndex;
        if (oldIndex < newIndex)
        {
            insertedOrder = newIndex - itemsCount + 1;
            toIndex = newIndex + 1;
        }
        else
        {
            insertedOrder = newIndex;
            toIndex = oldIndex + itemsCount;
        }

        // Insert items at new position
        list.InsertRange(insertedOrder, itemsToMove);

        // Update Order property
        var fromIndex = Math.Min(oldIndex, newIndex);
        for (var i = fromIndex; i < toIndex; i++)
        {
            list[i].Order = i;
        }

        return true;
    }

    /// <summary>
    /// Move a range of items in an ObservableCollection to a new position with their updated Order property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <param name="oldIndex"></param>
    /// <param name="newIndex"></param>
    /// <param name="itemsCount"></param>
    /// <returns></returns>
    public static bool Move<T>(this ObservableCollection<T> collection, int oldIndex, int newIndex, int itemsCount)
        where T : class, IOrder
    {
        ArgumentNullException.ThrowIfNull(collection);

        // Validate necessity
        if (oldIndex == newIndex || itemsCount == 0)
        {
            return false;
        }

        // Find items to move with updated Order property
        var itemsToMove = new List<T>();
        for (var i = 0; i < collection.Count; i++)
        {
            var item = collection[i];
            if (item.Order >= newIndex && item.Order < newIndex + itemsCount)
            {
                itemsToMove.Add(item);
            }
        }
        foreach (var item in itemsToMove)
        {
            collection.Remove(item);
        }

        // Find the new insertion point
        var insertedIndex = 0;
        if (collection.Count == 0)
        {
            insertedIndex = 0;
        }
        else
        {
            var firstItemOrder = collection[0].Order;
            var lastItemOrder = collection[^1].Order;
            if (newIndex < firstItemOrder)
            {
                insertedIndex = 0;
            }
            else if (newIndex >= lastItemOrder)
            {
                insertedIndex = collection.Count;
            }
            else
            {
                for (var i = 0; i < collection.Count; i++)
                {
                    var item = collection[i];
                    if (item.Order >= newIndex)
                    {
                        insertedIndex = i;
                        break;
                    }
                }
            }
        }

        // Insert at new position
        for (var i = 0; i < itemsToMove.Count; i++)
        {
            collection.Insert(insertedIndex + i, itemsToMove[i]);
        }

        return true;
    }
}
