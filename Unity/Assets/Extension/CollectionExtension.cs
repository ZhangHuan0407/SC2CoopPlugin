using System;
using System.Collections.Generic;

namespace System.Extension
{
    public static class CollectionExtension
    {
        public static int RemoveAll<T>(this LinkedList<T> list, Predicate<T> predicate)
        {
            if (list is null)
                throw new ArgumentNullException(nameof(list));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            int removeCount = 0;
            LinkedListNode<T> node = list.First;
            while (node != null)
            {
                bool removeThisNode = predicate(node.Value);
                LinkedListNode<T> next = node.Next;
                if (removeThisNode)
                {
                    removeCount++;
                    list.Remove(node);
                }
                node = next;
            }
            return removeCount;
        }

        public static void SortByKeys<TKey, TValue>(this IList<TValue> values, IList<TKey> keys) where TKey : IComparable<TKey>
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));
            if (keys is null)
                throw new ArgumentNullException(nameof(keys));

            if (values.Count != keys.Count)
                throw new ArgumentException($"keys.Count: {keys.Count} is not equal values.Count: {values.Count}");

            Sort_Internal(values, keys, 0, values.Count - 1);
        }
        private static void Sort_Internal<TKey, TValue>(IList<TValue> values, IList<TKey> keys, int l, int r) where TKey : IComparable<TKey>
        {
            TKey mid = keys[(l + r) / 2];
            int i = l;
            int j = r;
            do
            {
                while (keys[i].CompareTo(mid) < 0)
                    i++;
                while (keys[j].CompareTo(mid) > 0)
                    j--;
                if (i <= j)
                {
                    TKey tempKey = keys[i];
                    keys[i] = keys[j];
                    keys[j] = tempKey;
                    TValue tempValue = values[i];
                    values[i] = values[j];
                    values[j] = tempValue;
                    i++;
                    j--;
                }
            } while (i <= j);
            if (l < j)
                Sort_Internal(values, keys, l, j);
            if (i < r)
                Sort_Internal(values, keys, i, r);
        }
    }
}