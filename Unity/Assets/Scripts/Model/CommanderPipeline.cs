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
        private List<PlayerOperatorEventModel> m_EventModels;
        public List<PlayerOperatorEventModel> EventModels
        {
            get => m_EventModels;
            set => m_EventModels = value;
        }

        //public Dictionary<StringID, string> LocalizationTable;

        protected CommanderPipeline()
        {
        }

        public static CommanderPipeline CreateDebug()
        {
            CommanderPipeline pipeline = new CommanderPipeline();
            pipeline.Commander = CommanderName.Zagara;
            pipeline.Language = SystemLanguage.ChineseSimplified;
            pipeline.Level = 15;
            pipeline.Masteries = new int[6];
            pipeline.Prestige = 0;
            pipeline.Title = "Title";
            pipeline.Desc = "Desc";
            pipeline.EventModels = new List<PlayerOperatorEventModel>();
            //model.LocalizationTable = new Dictionary<StringID, string>();
            return pipeline;
        }
    }
}