using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Math
{
    /// <summary>
    /// 稳定随机数
    /// </summary>
    [Serializable]
    public class Random
    {
        /* const */
        public const ulong A = 25214903917, B = 11, C = 1 << 48 - 1;

        /* field */
        [SerializeField]
        private ulong m_Seed;

        /* ctor */
        public Random(int seed)
        {
            m_Seed = (ulong)seed;
        }

        /* func */
        /// <summary>
        /// 产生一个随机值 [min, max)
        /// </summary>
        public int Next(int min, int max)
        {
            if (min >= max)
            {
                throw new ArgumentException();
            }
            m_Seed = (A * m_Seed + 11) % C;
            return (int)((ulong)min + m_Seed % (ulong)(max - min));
        }
        /// <summary>
        /// 产生一个随机值 [0, 1]
        /// </summary>
        public float Next()
        {
            m_Seed = (A * m_Seed + 11) % C;
            uint value = (uint)m_Seed;
            return (float)value / (float)uint.MaxValue;
        }

        /// <summary>
        /// 打乱数组
        /// </summary>
        public void Shuffle<T>(IList<T> list)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 依照圆桌概率学方法，进行一次加权掉落
        /// <para>不会修改传入的权重列表</para>
        /// </summary>
        /// <param name="dropList">权重值的列表，每一项的值最小为0</param>
        /// <param name="itemIndex">如果掉落成功，第X项作为掉落项</param>
        /// <returns>尝试掉落的结果</returns>
        public bool TryDrop(IReadOnlyList<int> dropList, out int itemIndex)
        {
            if (dropList is null)
                throw new ArgumentNullException(nameof(dropList));

            int totalValue = 0;
            itemIndex = 0;
            foreach (int dropItem in dropList)
            {
                if (dropItem < 0)
                    return false;
                totalValue += dropItem;
            }
            if (totalValue <= 0)
                return false;
            int value = Next(0, totalValue + 1);
            totalValue = 0;
            for (int i = 0; i < dropList.Count; i++)
            {
                int dropItem = dropList[i];
                totalValue += dropItem;
                if (totalValue >= value)
                {
                    itemIndex = i;
                    return true;
                }
            }
            throw new ArgumentException("number out of int32 range");
        }
        /// <summary>
        /// 依照圆桌概率学方法，进行一次加权掉落
        /// <para>不会修改传入的权重列表</para>
        /// </summary>
        /// <param name="set">(掉落项, 权重值)的集合，每一项的权重最小为0</param>
        /// <param name="itemKey">如果掉落成功，掉落项</param>
        /// <returns>尝试掉落的结果</returns>
        public bool TryDrop<TKey>(IEnumerable<KeyValuePair<TKey, int>> set, out TKey itemKey)
        {
            if (set is null)
                throw new ArgumentNullException(nameof(set));

            int totalValue = 0;
            itemKey = default;
            foreach (KeyValuePair<TKey, int> dropItem in set)
            {
                if (dropItem.Value < 0)
                    return false;
                totalValue += dropItem.Value;
            }
            if (totalValue <= 0)
                return false;
            int value = Next(0, totalValue + 1);
            totalValue = 0;
            foreach (KeyValuePair<TKey, int> dropItem in set)
            {
                totalValue += dropItem.Value;
                if (totalValue >= value)
                {
                    itemKey = dropItem.Key;
                    return true;
                }
            }
            throw new ArgumentException("number out of int32 range");
        }
    }
}