using System;
using System.Collections.Generic;
using Table;
using UnityEngine;

namespace Game.Editor
{
    public class GuardWrapper : Guard
    {
        public new int Defence
        {
            get => m_Defence;
            set => m_Defence = value;
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

        public GuardWrapper() : base()
        {
            Technology = string.Empty;
        }

        public override bool Equals(object obj)
        {
            return obj is GuardWrapper wrapper &&
                   m_Defence == wrapper.m_Defence &&
                   m_UpgradePreLevel == wrapper.m_UpgradePreLevel &&
                   m_Technology == wrapper.m_Technology;
        }

        public override int GetHashCode()
        {
            int hashCode = 1760374792;
            hashCode = hashCode * -1521134295 + m_Defence.GetHashCode();
            hashCode = hashCode * -1521134295 + m_UpgradePreLevel.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(m_Technology);
            return hashCode;
        }
    }
}