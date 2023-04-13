using System.Collections.Generic;
using Table;

namespace Game.Editor
{
    public class UnitTableEntryWrapper : UnitTable.Entry
    {
        public new int ID
        {
            get => m_ID;
            set => m_ID = value;
        }

        public new string Annotation
        {
            get => m_Annotation;
            set => m_Annotation = value;
        }

        public new StringID Name
        {
            get => m_Name;
            set => m_Name = value;
        }

        public new Commander Commander
        {
            get => m_Commander;
            set => m_Commander = value;
        }

        public new int UnlockLevel
        {
            get => m_UnlockLevel;
            set => m_UnlockLevel = value;
        }

        public new int CrystalCost
        {
            get => m_CrystalCost;
            set => m_CrystalCost = value;
        }

        public new int GasCost
        {
            get => m_GasCost;
            set => m_GasCost = value;
        }

        public new float Population
        {
            get => m_Population;
            set => m_Population = value;
        }

        public new int BuildDuration
        {
            get => m_BuildDuration;
            set => m_BuildDuration = value;
        }

        public new UnitLabel Label
        {
            get => m_Label;
            set => m_Label = value;
        }

        public new string StealthTechnology
        {
            get => m_StealthTechnology;
            set => m_StealthTechnology = value;
        }

        public UnitTableEntryWrapper() : base()
        {
            Population = 1;
        }

        public override bool Equals(object obj)
        {
            return obj is UnitTableEntryWrapper wrapper &&
                   m_ID == wrapper.m_ID &&
                   m_Annotation == wrapper.m_Annotation &&
                   EqualityComparer<StringID>.Default.Equals(m_Name, wrapper.m_Name) &&
                   m_Commander == wrapper.m_Commander &&
                   m_UnlockLevel == wrapper.m_UnlockLevel &&
                   m_CrystalCost == wrapper.m_CrystalCost &&
                   m_GasCost == wrapper.m_GasCost &&
                   m_Population == wrapper.m_Population &&
                   m_BuildDuration == wrapper.m_BuildDuration &&
                   m_Label == wrapper.m_Label;
        }

        public override int GetHashCode()
        {
            int hashCode = -743652579;
            hashCode = hashCode * -1521134295 + m_ID.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(m_Annotation);
            hashCode = hashCode * -1521134295 + m_Name.GetHashCode();
            hashCode = hashCode * -1521134295 + m_Commander.GetHashCode();
            hashCode = hashCode * -1521134295 + m_UnlockLevel.GetHashCode();
            hashCode = hashCode * -1521134295 + m_CrystalCost.GetHashCode();
            hashCode = hashCode * -1521134295 + m_GasCost.GetHashCode();
            hashCode = hashCode * -1521134295 + m_Population.GetHashCode();
            hashCode = hashCode * -1521134295 + m_BuildDuration.GetHashCode();
            hashCode = hashCode * -1521134295 + m_Label.GetHashCode();
            return hashCode;
        }
    }
}