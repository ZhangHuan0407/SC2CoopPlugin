using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Table
{
    [Serializable]
    public class MasteriesTable
    {
        [Serializable]
        [StructLayout(LayoutKind.Auto)]
        public class Entry
        {
            [SerializeField]
            private Commander m_Commander;
            public Commander Commander
            {
                get => m_Commander;
                private set => m_Commander = value;
            }

            [SerializeField]
            private StringID m_Name;
            public StringID Name
            {
                get => m_Name;
                private set => m_Name = value;
            }

            [SerializeField]
            private StringID m_Describe;
            public StringID Describe
            {
                get => m_Describe;
                private set => m_Describe = value;
            }

            [SerializeField]
            private int m_Index;
            public int Index
            {
                get => m_Index;
                private set => m_Index = value;
            }

            [SerializeField]
            private double m_ThirtyLevelValue;
            public double ThirtyLevelValue
            {
                get => m_ThirtyLevelValue;
                private set => m_ThirtyLevelValue = value;
            }
        }

        [NonSerialized]
        private Dictionary<Commander, Entry[]> m_Data;
        public IReadOnlyDictionary<Commander, Entry[]> Data => m_Data;

        public MasteriesTable()
        {
            m_Data = new Dictionary<Commander, Entry[]>();
        }

        #region Serialized
        public static JSONObject ToJSON(object instance)
        {
            if (!(instance is MasteriesTable table))
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
            if (@array is null || @array.IsNull)
                return null;
            MasteriesTable table = new MasteriesTable();
            for (int i = 0; i < @array.list.Count; i++)
            {
                JSONObject @object = @array.list[i];
                Entry entry = JSONMap.ParseJSON<Entry>(@object);
                if (!table.m_Data.TryGetValue(entry.Commander, out Entry[] entries))
                {
                    entries = new Entry[6];
                    table.m_Data[entry.Commander] = entries;
                }
                entries[entry.Index] = entry;
            }
            return table;
        }
        #endregion
    }
}