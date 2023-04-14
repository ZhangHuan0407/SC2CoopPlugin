using System;
using UnityEngine;

namespace Table
{
    [Serializable]
    public class Guard
    {
        [SerializeField]
        protected int m_Defence;
        public int Defence
        {
            get => m_Defence;
            private set => m_Defence = value;
        }

        [SerializeField]
        protected int m_UpgradePreLevel;
        public int UpgradePreLevel
        {
            get => m_UpgradePreLevel;
            private set => m_UpgradePreLevel = value;
        }

        [SerializeField]
        protected string m_Technology;
        public string Technology
        {
            get => m_Technology;
            private set => m_Technology = value;
        }
    }
}