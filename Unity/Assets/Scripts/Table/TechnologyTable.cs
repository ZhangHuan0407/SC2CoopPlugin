using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Table
{
    [Serializable]
    public class TechnologyTable
    {
        [Serializable]
        [StructLayout(LayoutKind.Auto)]
        public class Entry
        {
            [SerializeField]
            private string m_ID;
            public string ID
            {
                get => m_ID;
                private set => m_ID = value;
            }

            [SerializeField]
            protected StringID m_Name;
            public StringID Name
            {
                get => m_Name;
                private set => m_Name = value;
            }

            [SerializeField]
            protected StringID m_Describe;
            public StringID Describe
            {
                get => m_Describe;
                private set => m_Describe = value;
            }

            [SerializeField]
            private int m_Unit0;
            public int Unit0
            {
                get => m_Unit0;
                private set => m_Unit0 = value;
            }

            [SerializeField]
            private int m_Unit1;
            public int Unit1
            {
                get => m_Unit1;
                private set => m_Unit1 = value;
            }

            [SerializeField]
            private CommanderName m_Commander;
            public CommanderName Commander
            {
                get => m_Commander;
                private set => m_Commander = value;
            }

            [SerializeField]
            private int m_Duration;
            public int Duration
            {
                get => m_Duration;
                private set => m_Duration = value;
            }

            [SerializeField]
            private int m_UnlockLevel;
            public int UnlockLevel
            {
                get => m_UnlockLevel;
                private set => m_UnlockLevel = value;
            }

            [SerializeField]
            private int m_CrystalCost;
            public int CrystalCost
            {
                get => m_CrystalCost;
                private set => m_CrystalCost = value;
            }

            [SerializeField]
            private int m_GasCost;
            public int GasCost
            {
                get => m_GasCost;
                private set => m_GasCost = value;
            }
        }

        [SerializeField]
        private Dictionary<string, Entry> m_Data;
        public IReadOnlyDictionary<string, Entry> Data => m_Data;

        public TechnologyTable()
        {
            m_Data = new Dictionary<string, Entry>();
        }

        #region Serialized
        public static JSONObject ToJSON(object instance)
        {
            if (instance is TechnologyTable table)
                return JSONMap.FieldsToJSON(table.m_Data.Values.ToArray(), null);
            else
                return new JSONObject(JSONObject.Type.NULL);
        }
        public static object ParseJSON(JSONObject @array)
        {
            if (@array == null || @array.IsNull)
                return null;
            TechnologyTable table = new TechnologyTable();
            for (int i = 0; i < @array.Count; i++)
            {
                Entry entry = JSONMap.ParseJSON<Entry>(@array.list[i]);
                table.m_Data[entry.ID] = entry;
            }
            return table;
        }
        #endregion
    }
}