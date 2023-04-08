using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine
{
    [Serializable]
    public abstract class FuckingDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
        where TKey : struct where TValue : class
    {
        /* field */
        [SerializeField]
        private TKey[] m_Keys;
        [SerializeField]
        private TValue[] m_Values;

        /* inter */

        public void OnAfterDeserialize()
        {
            Clear();
            for (int i = 0; i < m_Keys.Length; i++)
                this[m_Keys[i]] = m_Values[i];
            m_Keys = null;
            m_Values = null;
        }
        public void OnBeforeSerialize()
        {
            m_Keys = Keys.ToArray();
            m_Values = Values.ToArray();
        }
    }
}