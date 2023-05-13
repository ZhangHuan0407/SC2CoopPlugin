using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Game;
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
            protected int m_ID;
            public int ID
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
            protected string m_Annotation;
            public string Annotation
            {
                get => m_Annotation;
                private set => m_Annotation = value;
            }

            [SerializeField]
            protected int m_Unit0;
            public int Unit0
            {
                get => m_Unit0;
                private set => m_Unit0 = value;
            }

            [SerializeField]
            protected int m_Unit1;
            public int Unit1
            {
                get => m_Unit1;
                private set => m_Unit1 = value;
            }

            [SerializeField]
            protected CommanderName m_Commander;
            public CommanderName Commander
            {
                get => m_Commander;
                private set => m_Commander = value;
            }

            [SerializeField]
            protected int m_Duration;
            public int Duration
            {
                get => m_Duration;
                private set => m_Duration = value;
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
            protected string m_Texture;
            public string Texture
            {
                get => m_Texture;
                private set => m_Texture = value;
            }

            public Sprite LoadTexture()
            {
                if (string.IsNullOrWhiteSpace(Texture))
                    return null;
                return ResourcesInterface.Load<Sprite>($"Textures/{Texture}");
            }
        }

        [SerializeField]
        private Dictionary<int, Entry> m_Data;
        public IReadOnlyDictionary<int, Entry> Data => m_Data;

        public Entry this[int id]
        {
            get
            {
                Data.TryGetValue(id, out Entry entry);
                return entry;
            }
        }

        public TechnologyTable()
        {
            m_Data = new Dictionary<int, Entry>();
        }

#if UNITY_EDITOR
        public void OverrideEntry_Editor(Entry entry) => m_Data[entry.ID] = entry;
#endif

        #region Serialized
        public static JSONObject ToJSON(object instance)
        {
            if (instance is TechnologyTable table)
            {
                JSONObject @array = new JSONObject(JSONObject.Type.ARRAY);
                foreach (Entry entry in table.m_Data.Values)
                    @array.list.Add(JSONMap.ToJSON(entry));
                return @array;
            }
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