using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Table
{
    [Serializable]
    public class UnitTable
    {
        [Serializable]
        [StructLayout(LayoutKind.Auto)]
        public class Entry
        {
            [SerializeField]
            protected int m_ID;
            public int ID
            {
                get => m_ID;
                private set => m_ID = value;
            }

#if UNITY_EDITOR
            [SerializeField]
            protected string m_Annotation;
            public string Annotation => m_Annotation;
#endif

            [SerializeField]
            protected StringID m_Name;
            public StringID Name
            {
                get => m_Name;
                private set => m_Name = value;
            }

            [SerializeField]
            protected Commander m_Commander;
            public Commander Commander
            {
                get => m_Commander;
                private set => m_Commander = value;
            }

            [SerializeField]
            protected int m_UnlockLevel;
            public int UnlockLevel
            {
                get => m_UnlockLevel;
                private set => m_UnlockLevel = value;
            }

            [SerializeField]
            protected int m_CrystalCost;
            public int CrystalCost
            {
                get => m_CrystalCost;
                private set => m_CrystalCost = value;
            }

            [SerializeField]
            protected int m_GasCost;
            public int GasCost
            {
                get => m_GasCost;
                private set => m_GasCost = value;
            }

            [SerializeField]
            protected float m_Population;
            public float Population
            {
                get => m_Population;
                private set => m_Population = value;
            }

            [SerializeField]
            protected int m_BuildDuration;
            public int BuildDuration
            {
                get => m_BuildDuration;
                private set => m_BuildDuration = value;
            }

            [SerializeField]
            protected UnitLabel m_Label;
            public UnitLabel Label
            {
                get => m_Label;
                private set => m_Label = value;
            }

            [SerializeField]
            protected string m_StealthTechnology;
            public string StealthTechnology
            {
                get => m_StealthTechnology;
                private set => m_StealthTechnology = value;
            }

            protected Entry()
            {
            }
        }

        private Dictionary<int, Entry> m_Data;
        public IReadOnlyDictionary<int, Entry> Data => m_Data;

        public UnitTable()
        {
            m_Data = new Dictionary<int, Entry>();
        }

#if UNITY_EDITOR
        public void InsertEntry_Editor(Entry entry) => m_Data[entry.ID] = entry;
#endif

        #region Serialized
        public static JSONObject ToJSON(object instance)
        {
            if (!(instance is UnitTable table))
                return new JSONObject(JSONObject.Type.NULL);
            JSONObject @array = new JSONObject(JSONObject.Type.ARRAY);
            foreach (Entry entry in table.Data.Values)
            {
                @array.Add(JSONMap.ToJSON(entry));
            }
            return @array;
        }
        public static object ParseJSON(JSONObject @array)
        {
            if (@array.IsNull)
                return null;
            UnitTable table = new UnitTable();
            for (int i = 0; i < @array.list.Count; i++)
            {
                JSONObject @object = @array.list[i];
                Entry entry = JSONMap.ParseJSON<Entry>(@object);
                table.m_Data[entry.ID] = entry;
            }
            return table;
        }
        #endregion
    }
}