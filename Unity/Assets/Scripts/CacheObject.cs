using System;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    public class CacheObject<T> : IEnumerable where T : UnityEngine.Object
    {
        /* field */
        public Dictionary<T, List<T>> Caches;
        public Action<T> OnPushItem;
        public Action<T> OnPopItem;

        /* ctor */
        public CacheObject()
        {
            Caches = new Dictionary<T, List<T>>();
        }

        /* func */
        public void Add(T template)
        {
            Caches.Add(template, new List<T>());
        }
        public void PushItem(T template, T copy)
        {
            List<T> list = Caches[template];
            list.Add(copy);
            OnPushItem?.Invoke(copy);
        }
        public T PopItem(T template)
        {
            List<T> list = Caches[template];
            T item;
            if (list.Count > 0)
            {
                item = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
            }
            else
                item = UnityEngine.Object.Instantiate(template);
            OnPopItem?.Invoke(item);
            return item;
        }

        /// <summary>
        /// 尝试基于目标模板，将缓冲拷贝数量提升至count
        /// <para>如果当前的缓冲拷贝数量已经大于<paramref name="count"/>，将会费电</para>
        /// </summary>
        public void Preload(T template, int count)
        {
            if (count <= 0)
            {
                LogService.Error(nameof(Preload), $"{template}, count: {count}");
                return;
            }
            int length = count / 16 * 16;
            if (count % 16 != 0)
                length += 16;
            T[] tempArray = new T[length];
            for (int i = 0; i < count; i++)
                tempArray[i] = PopItem(template);
            for (int i = 0; i < count; i++)
                PushItem(template, tempArray[i]);
        }

        public IEnumerator GetEnumerator() => Caches.GetEnumerator();
    }
}