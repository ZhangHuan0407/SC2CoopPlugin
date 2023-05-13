using System;
using System.Collections;
using System.Collections.Generic;
using Table;
using UnityEngine;

namespace Game.Editor
{
    public class TechnologyTableEntryWrapper : TechnologyTable.Entry
    {
        public new int ID
        {
            get => m_ID;
            set => m_ID = value;
        }

        public new StringID Name
        {
            get => m_Name;
            set => m_Name = value;
        }

        public new string Annotation
        {
            get => m_Annotation;
            set => m_Annotation = value;
        }

        public new CommanderName Commander
        {
            get => m_Commander;
            set => m_Commander = value;
        }

        public new int Unit0
        {
            get => m_Unit0;
            set => m_Unit0 = value;
        }

        public new int Unit1
        {
            get => m_Unit1;
            set => m_Unit1 = value;
        }

        public new int Duration
        {
            get => m_Duration;
            set => m_Duration = value;
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

        public new string Texture
        {
            get => m_Texture;
            set => m_Texture = value;
        }

        public TechnologyTableEntryWrapper() : base()
        {
        }

        public override bool Equals(object obj)
        {
            TechnologyTableEntryWrapper wrapper = obj as TechnologyTableEntryWrapper;
            bool isEqual = wrapper is TechnologyTableEntryWrapper &&
                           m_ID == wrapper.m_ID &&
                           EqualityComparer<StringID>.Default.Equals(m_Name, wrapper.m_Name) &&
                           m_Annotation == wrapper.m_Annotation &&
                           m_Commander == wrapper.m_Commander &&
                           m_Unit0 == wrapper.m_Unit0 &&
                           m_Unit1 == wrapper.m_Unit1 &&
                           m_Duration == wrapper.m_Duration &&
                           m_UnlockLevel == wrapper.m_UnlockLevel &&
                           m_CrystalCost == wrapper.m_CrystalCost &&
                           m_GasCost == wrapper.m_GasCost &&
                           m_Texture == wrapper.m_Texture;
            return isEqual;
        }

        public override int GetHashCode()
        {
            int hashCode = -1834660212;
            hashCode = hashCode * -1521134295 + m_ID.GetHashCode();
            hashCode = hashCode * -1521134295 + m_Name.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(m_Annotation);
            hashCode = hashCode * -1521134295 + m_Commander.GetHashCode();
            hashCode = hashCode * -1521134295 + m_Unit0;
            hashCode = hashCode * -1521134295 + m_Unit1;
            hashCode = hashCode * -1521134295 + m_Duration;
            hashCode = hashCode * -1521134295 + m_UnlockLevel;
            hashCode = hashCode * -1521134295 + m_CrystalCost;
            hashCode = hashCode * -1521134295 + m_GasCost;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(m_Texture);
            return hashCode;
        }

        #region Serialized
        public static JSONObject ToJSON(object instance) => JSONMap.FieldsToJSON(instance, null);
        public static object ParseJSON(JSONObject @object)
        {
            TechnologyTableEntryWrapper entry = new TechnologyTableEntryWrapper();
            JSONMap.FieldsParseJSON(entry, @object);
            return entry;
        }
        #endregion
    }
}