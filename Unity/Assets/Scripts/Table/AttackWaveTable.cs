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

            [SerializeField]
            private StringID m_Name;
            public StringID Name
            {
                get => m_Name;
                private set => m_Name = value;
            }

            [SerializeField]
            private int[] m_UnitID;
            public int[] UnitID
            {
                get => m_UnitID;
                private set => m_UnitID = value;
            }

            [SerializeField]
            private string m_TotalCost;
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