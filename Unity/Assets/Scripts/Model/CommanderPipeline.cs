using System;
using System.Collections.Generic;
using Table;
using UnityEngine;

namespace Game.Model
{
    [Serializable]
    public class CommanderPipeline
    {
        [SerializeField]
        private CommanderName m_Commander;
        public CommanderName Commander
        {
            get => m_Commander;
            set => m_Commander = value;
        }

        [SerializeField]
        private SystemLanguage m_Language;
        public SystemLanguage Language
        {
            get => m_Language;
            set => m_Language = value;
        }

        [SerializeField]
        private int m_Level;
        public int Level
        {
            get => m_Level;
            set => m_Level = value;
        }

        [SerializeField]
        private int[] m_Masteries;
        public int[] Masteries
        {
            get => m_Masteries;
            set => m_Masteries = value;
        }

        [SerializeField]
        private int m_Prestige;
        public int Prestige
        {
            get => m_Prestige;
            set => m_Prestige = value;
        }

        [SerializeField]
        private string m_Title;
        public string Title
        {
            get => m_Title;
            set => m_Title = value;
        }

        [SerializeField]
        private string m_Desc;
        public string Desc
        {
            get => m_Desc;
            set => m_Desc = value;
        }

        [SerializeField]
        private List<IEventModel> m_EventModels;
        public List<IEventModel> EventModels
        {
            get => m_EventModels;
            set => m_EventModels = value;
        }

        [SerializeField]
        private Guid m_Guid;
        public Guid Guid
        {
            get => m_Guid;
            set => m_Guid = value;
        }

        [SerializeField]
        private string m_DemoURL;
        public string DemoURL
        {
            get => m_DemoURL;
            set => m_DemoURL = value;
        }
        //public Dictionary<StringID, string> LocalizationTable;

        protected CommanderPipeline()
        {
        }

        #region Serialized
        public static JSONObject ToJSON(object instance)
        {
            if (instance is CommanderPipeline commanderPipeline)
            {
                JSONObject @object = JSONMap.FieldsToJSON(commanderPipeline, null);
                JSONObject @descField = @object.GetField(nameof(m_Desc));
                @descField.str = @descField.str.Replace("\n", "\\n");
                return @object;
            }
            return new JSONObject(JSONObject.Type.NULL);
        }
        public static object ParseJSON(JSONObject @object)
        {
            if (@object == null || @object.IsNull)
                return null;
            CommanderPipeline commanderPipeline = new CommanderPipeline();
            JSONMap.FieldsParseJSON<CommanderPipeline>(commanderPipeline, @object);
            commanderPipeline.Desc = commanderPipeline.Desc.Replace("\\n", "\n");
            return commanderPipeline;
        }
        #endregion
    }
}