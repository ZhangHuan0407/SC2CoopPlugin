using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Table
{
    [Serializable]
    public class AttackWaveTable
    {
        [Serializable]
        [StructLayout(LayoutKind.Auto)]
        public class Entry
        {
            [SerializeField]
            private AmonAIName m_AI;
            public AmonAIName AI
            {
                get => m_AI;
                private set => m_AI = value;
            }

            [SerializeField]
            private int m_Technology;
            public int Technology
            {
                get => m_Technology;
                private set => m_Technology = value;
            }

            public StringID Name => new StringID($"{nameof(AmonAIName)}.{AI}");

            public StringID Describe => new StringID($"{nameof(AmonAIName)}.{AI}.Desc");

            [SerializeField]
            private int[] m_UnitID;
            public int[] UnitID
            {
                get => m_UnitID;
                private set => m_UnitID = value;
            }

            public int TotalCost
            {
                get
                {
                    switch (Technology)
                    {
                        case 0:
                            return 0;
                        default:
                            throw new ArgumentException("TotalCost Technology:" + Technology.ToString());
                    }
                }
            }
        }

        [NonSerialized]
        private Dictionary<AmonAIName, Entry[]> m_Data;
        public IReadOnlyDictionary<AmonAIName, Entry[]> Data => m_Data;

        public Entry this[AmonAIName AIName, int technology]
        {
            get
            {
                Entry[] data = Data[AIName];
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i].Technology == technology)
                        return data[i];
                }
                return null;
            }
        }

        public AttackWaveTable()
        {
            m_Data = new Dictionary<AmonAIName, Entry[]>();
        }

        #region Serialized
        public static JSONObject ToJSON(object instance)
        {
            if (!(instance is AttackWaveTable table))
                return new JSONObject(JSONObject.Type.NULL);
            JSONObject @array = new JSONObject(JSONObject.Type.ARRAY);
            foreach (Entry[] entries in table.Data.Values)
            {
                for (int i = 0; i < entries.Length; i++)
                    @array.Add(JSONMap.ToJSON(entries[i]));
            }
            return @array;
        }
        public static object ParseJSON(JSONObject @array)
        {
            if (@array == null || @array.IsNull)
                return null;
            AttackWaveTable table = new AttackWaveTable();
            for (int i = 0; i < @array.list.Count; i++)
            {
                JSONObject @object = @array.list[i];
                Entry entry = JSONMap.ParseJSON<Entry>(@object);
                if (!table.m_Data.TryGetValue(entry.AI, out Entry[] entries))
                {
                    entries = new Entry[7];
                    table.m_Data.Add(entry.AI, entries);
                }
                entries[entry.Technology - 1] = entry;
            }
            return table;
        }
        #endregion
    }
}