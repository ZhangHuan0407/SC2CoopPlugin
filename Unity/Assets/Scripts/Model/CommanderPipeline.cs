using System;
using System.Collections.Generic;
using Table;
using UnityEngine;

namespace Game.Model
{
    [Serializable]
    public class CommanderPipeline
    {
        public CommanderName Commander;
        public SystemLanguage Language;
        public int Level;
        public int[] Masteries;
        public int Prestige;
        public string Title;
        public string Desc;
        public List<PlayerOperatorEventModel> EventModels;
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