using System.Collections.Generic;
using Table;
using UnityEngine;

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

        public new float MoveSpeed
        {
            get => m_MoveSpeed;
            set => m_MoveSpeed = value;
        }

        public new string StealthTechnology
        {
            get => m_StealthTechnology;
            set => m_StealthTechnology = value;
        }

        public new AttackWeapon Weapon0
        {
            get => m_Weapon0;
            set => m_Weapon0 = value;
        }

        public new AttackWeapon Weapon1
        {
            get => m_Weapon1;
            set => m_Weapon1 = value;
        }

        public new AttackWeapon Weapon2
        {
            get => m_Weapon2;
            set => m_Weapon2 = value;
        }

        public new Guard Guard
        {
            get => m_Guard;
            set => m_Guard = value;
        }

        public new Guard Shield
        {
            get => m_Shield;
            set => m_Shield = value;
        }

        public UnitTableEntryWrapper() : base()
        {
            Population = 1;
        }

        #region Serialized
        public static JSONObject ToJSON(object instance) => JSONMap.FieldsToJSON(instance, null);
        public static object ParseJSON(JSONObject @object)
        {
            UnitTableEntryWrapper entry = new UnitTableEntryWrapper();
            JSONMap.FieldsParseJSON(entry, @object);
            entry.m_Weapon0 = JSONMap.ParseJSON<AttackWeaponWrapper>(@object.GetField(nameof(m_Weapon0)));
            entry.m_Weapon1 = JSONMap.ParseJSON<AttackWeaponWrapper>(@object.GetField(nameof(m_Weapon1)));
            entry.m_Weapon2 = JSONMap.ParseJSON<AttackWeaponWrapper>(@object.GetField(nameof(m_Weapon2)));
            entry.m_Guard = JSONMap.ParseJSON<Guard>(@object.GetField(nameof(m_Guard)));
            entry.m_Shield = JSONMap.ParseJSON<Guard>(@object.GetField(nameof(m_Shield)));
            return entry;
        }

        public override bool Equals(object obj)
        {
            UnitTableEntryWrapper wrapper = obj as UnitTableEntryWrapper;
            bool isEqual = wrapper is UnitTableEntryWrapper &&
                           m_ID == wrapper.m_ID &&
                           m_Annotation == wrapper.m_Annotation &&
                           EqualityComparer<StringID>.Default.Equals(m_Name, wrapper.m_Name) &&
                           m_Commander == wrapper.m_Commander &&
                           m_UnlockLevel == wrapper.m_UnlockLevel &&
                           m_CrystalCost == wrapper.m_CrystalCost &&
                           m_GasCost == wrapper.m_GasCost &&
                           m_Population == wrapper.m_Population &&
                           m_BuildDuration == wrapper.m_BuildDuration &&
                           m_Label == wrapper.m_Label &&
                           m_StealthTechnology == wrapper.m_StealthTechnology &&
                           m_MoveSpeed == wrapper.m_MoveSpeed;
            if (isEqual)
            {
                isEqual = (m_Weapon0 == null && wrapper.m_Weapon0 == null) ||
                            m_Weapon0 != null && m_Weapon0.Equals(wrapper.m_Weapon0);
            }
            if (isEqual)
            {
                isEqual = (m_Weapon1 == null && wrapper.m_Weapon1 == null) ||
                            m_Weapon1 != null && m_Weapon1.Equals(wrapper.m_Weapon1);
            }
            if (isEqual)
            {
                isEqual = (m_Weapon2 == null && wrapper.m_Weapon2 == null) ||
                            m_Weapon2 != null && m_Weapon2.Equals(wrapper.m_Weapon2);
            }
            if (isEqual)
            {
                isEqual = (m_Guard == null && wrapper.m_Guard == null) ||
                            m_Guard != null && m_Guard.Equals(wrapper.m_Guard);
            }
            if (isEqual)
            {
                isEqual = (m_Shield == null && wrapper.m_Shield == null) ||
                            m_Shield != null && m_Shield.Equals(wrapper.m_Shield);
            }
            return isEqual;
        }

        public override int GetHashCode()
        {
            int hashCode = -1834660212;
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
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(m_StealthTechnology);
            hashCode = hashCode * -1521134295 + m_MoveSpeed.GetHashCode();
            hashCode = hashCode * -1521134295 + m_Weapon0?.GetHashCode() ?? 10203;
            hashCode = hashCode * -1521134295 + m_Weapon1?.GetHashCode() ?? 10203;
            hashCode = hashCode * -1521134295 + m_Weapon2?.GetHashCode() ?? 10203;
            hashCode = hashCode * -1521134295 + m_Guard?.GetHashCode() ?? 10203;
            hashCode = hashCode * -1521134295 + m_Shield?.GetHashCode() ?? 10203;
            return hashCode;
        }
        #endregion
    }
}