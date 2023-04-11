using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Table
{
    [Serializable]
    public class PrestigeTable
    {
        [Serializable]
        [StructLayout(LayoutKind.Auto)]
        public class Entry
        {
            [SerializeField]
            public Commander m_Commander;
            public Commander Commander
            {
                get => m_Commander;
                private set => m_Commander = value;
            }

            [SerializeField]
            public StringID m_Name;
            public StringID Name
            {
                get => m_Name;
                private set => m_Name = value;
            }

            [SerializeField]
            public StringID m_Describe;
            public StringID Describe
            {
                get => m_Describe;
                private set => m_Describe = value;
            }

            [SerializeField]
            public int m_Index;
            public int Index
            {
                get => m_Index;
                private set => m_Index = value;
            }

            [SerializeField]
            public double m_ThirtyLevelValue;
            public double ThirtyLevelValue
            {
                get => m_ThirtyLevelValue;
                private set => m_ThirtyLevelValue = value;
            }
        }

        public Dictionary<Commander, Entry[]> Data;

        public PrestigeTable()
        {
            Data = new Dictionary<Commander, Entry[]>();
        }

        #region Serialized
        public static JSONObject ToJSON(object instance)
        {
            if (!(instance is PrestigeTable table))
                return new JSONObject(JSONObject.Type.NULL);
            JSONObject @array = new JSONObject(JSONObject.Type.ARRAY);
            foreach (var pair in table.Data)
            {
                for (int i = 0; i < pair.Value.Length; i++)
                    @array.Add(JSONMap.ToJSON(pair.Value[i]));
            }
            return @array;
        }
        public static object ParseJSON(JSONObject @array)
        {
            if (@array.IsNull)
                return null;
            PrestigeTable table = new PrestigeTable();
            for (int i = 0; i < array.list.Count; i++)
            {
                JSONObject @object = array.list[i];
                Entry entry = JSONMap.ParseJSON<Entry>(@object);
                if (!table.Data.TryGetValue(entry.Commander, out Entry[] entries))
                {
                    entries = new Entry[6];
                    table.Data[entry.Commander] = entries;
                }
                entries[entry.Index] = entry;
            }
            return table;
        }
        #endregion
    }
}