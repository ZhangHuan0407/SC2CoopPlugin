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

        protected int m_Multiple;
        public int Multiple
        {
            get => m_Multiple;
            private set => m_Multiple = value;
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
        protected float m_Range;
        public float Range
        {
            get => m_Range;
            private set => m_Range = value;
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