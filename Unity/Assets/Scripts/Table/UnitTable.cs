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
            protected CommanderName m_Commander;
            public CommanderName Commander
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
            protected int m_HP;
            public int HP
            {
                get => m_HP;
                private set => m_HP = value;
            }

            [SerializeField]
            protected int m_HP2;
            public int HP2
            {
                get => m_HP2;
                private set => m_HP2 = value;
            }

            [SerializeField]
            protected int m_Energy;
            public int Energy
            {
                get => m_Energy;
                private set => m_Energy = value;
            }

            [SerializeField]
            protected UnitLabel m_Label;
            public UnitLabel Label
            {
                get => m_Label;
                private set => m_Label = value;
            }
            
            [SerializeField]
            protected float m_MoveSpeed;
            public float MoveSpeed
            {
                get => m_MoveSpeed;
                private set => m_MoveSpeed = value;
            }

            [SerializeField]
            protected string m_StealthTechnology;
            public string StealthTechnology
            {
                get => m_StealthTechnology;
                private set => m_StealthTechnology = value;
            }

            [SerializeField]
            protected string m_Texture;
            public string Texture
            {
                get => m_Texture;
                private set => m_Texture = value;
            }

            [SerializeField]
            protected AttackWeapon m_Weapon0;
            public AttackWeapon Weapon0
            {
                get => m_Weapon0;
                private set => m_Weapon0 = value;
            }

            [SerializeField]
            protected AttackWeapon m_Weapon1;
            public AttackWeapon Weapon1
            {
                get => m_Weapon1;
                private set => m_Weapon1 = value;
            }

            [SerializeField]
            protected AttackWeapon m_Weapon2;
            public AttackWeapon Weapon2
            {
                get => m_Weapon2;
                private set => m_Weapon2 = value;
            }

            [SerializeField]
            protected Guard m_Guard;
            public Guard Guard
            {
                get => m_Guard;
                private set => m_Guard = value;
            }

            [SerializeField]
            protected Guard m_Shield;
            public Guard Shield
            {
                get => m_Shield;
                private set => m_Shield = value;
            }

            protected Entry()
            {
#if UNITY_EDITOR
                m_Annotation = string.Empty;
#endif
                m_Name.Key = string.Empty;
                m_StealthTechnology = string.Empty;
                m_Texture = string.Empty;
            }
        }

        [NonSerialized]
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
            if (@array == null || @array.IsNull)
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