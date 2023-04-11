using System;
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
            public int m_ID;
            public int ID
            {
                get => m_ID;
                private set => m_ID = value;
            }

            [SerializeField]
            public StringID m_Name;
            public StringID Name
            {
                get => m_Name;
                private set => m_Name = value;
            }

            [SerializeField]
            public Commander m_Commander;
            public Commander Commander
            {
                get => m_Commander;
                private set => m_Commander = value;
            }

            [SerializeField]
            public string m_MineralCost;
            public string MineralCost
            {
                get => m_MineralCost;
                private set => m_MineralCost = value;
            }

            [SerializeField]
            public string m_GasCost;
            public string GasCost
            {
                get => m_GasCost;
                private set => m_GasCost = value;
            }

            [SerializeField]
            public int m_BuildDuration;
            public int BuildDuration
            {
                get => m_BuildDuration;
                private set => m_BuildDuration = value;
            }

            [SerializeField]
            public UnitLabel[] m_Label;
            public UnitLabel[] Label
            {
                get => m_Label;
                private set => m_Label = value;
            }
        }

        public UnitTable()
        {
        }
    }
}