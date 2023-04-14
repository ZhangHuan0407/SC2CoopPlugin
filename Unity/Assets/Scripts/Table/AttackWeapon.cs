using System;
using UnityEngine;

namespace Table
{
    [Serializable]
    public class AttackWeapon
    {
        [SerializeField]
        protected int m_Attack;
        public int Attack
        {
            get => m_Attack;
            private set => m_Attack = value;
        }

        [SerializeField]
        protected UnitLabel m_Label;
        public UnitLabel Label
        {
            get => m_Label;
            private set => m_Label = value;
        }

        [SerializeField]
        protected float m_Speed;
        public float Speed
        {
            get => m_Speed;
            private set => m_Speed = value;
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