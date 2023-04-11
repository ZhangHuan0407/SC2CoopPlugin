using System;
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
            public AmonAI m_AI;
            public AmonAI AI
            {
                get => m_AI;
                private set => m_AI = value;
            }

            [SerializeField]
            public int m_Technology;
            public int Technology
            {
                get => m_Technology;
                private set => m_Technology = value;
            }

            [SerializeField]
            public StringID m_Name;
            public StringID Name
            {
                get => m_Name;
                private set => m_Name = value;
            }

            [SerializeField]
            public int[] m_UnitID;
            public int[] UnitID
            {
                get => m_UnitID;
                private set => m_UnitID = value;
            }

            [SerializeField]
            public string m_TotalCost;
            public string TotalCost
            {
                get => m_TotalCost;
                private set => m_TotalCost = value;
            }
        }

        public AttackWaveTable()
        {
        }
    }
}