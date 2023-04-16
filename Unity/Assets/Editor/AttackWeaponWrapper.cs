using System;
using System.Collections.Generic;
using Table;
using UnityEngine;

namespace Game.Editor
{
    public class AttackWeaponWrapper : AttackWeapon
    {
        public new int Attack
        {
            get => m_Attack;
            set => m_Attack = value;
        }

        public new int Multiple
        {
            get => m_Multiple;
            set => m_Multiple = value;
        }

        public new UnitLabel Label
        {
            get => m_Label;
            set => m_Label = value;
        }

        public new float Speed
        {
            get => m_Speed;
            set => m_Speed = value;
        }

        public new float Range
        {
            get => m_Range;
            set => m_Range = value;
        }

        public new int UpgradePreLevel
        {
            get => m_UpgradePreLevel;
            set => m_UpgradePreLevel = value;
        }

        public new string Technology
        {
            get => m_Technology;
            set => m_Technology = value;
        }

        public AttackWeaponWrapper() : base()
        {
            m_Multiple = 1;
            m_Technology = string.Empty;
        }

        public override bool Equals(object obj)
        {
            return obj is AttackWeaponWrapper wrapper &&
                   m_Attack == wrapper.m_Attack &&
                   m_Label == wrapper.m_Label &&
                   m_Speed == wrapper.m_Speed &&
                   m_Range == wrapper.m_Range &&
                   m_UpgradePreLevel == wrapper.m_UpgradePreLevel &&
                   m_Technology == wrapper.m_Technology;
        }

        public override int GetHashCode()
        {
            int hashCode = 1843636647;
            hashCode = hashCode * -1521134295 + m_Attack.GetHashCode();
            hashCode = hashCode * -1521134295 + m_Label.GetHashCode();
            hashCode = hashCode * -1521134295 + m_Speed.GetHashCode();
            hashCode = hashCode * -1521134295 + m_Range.GetHashCode();
            hashCode = hashCode * -1521134295 + m_UpgradePreLevel.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(m_Technology);
            return hashCode;
        }
    }
}