using System;
using System.Collections.Generic;
using Table;
using UnityEngine;

namespace Game.Model
{
    [Serializable]
    public class CommanderModel
    {
        public CommanderName Commander;
        public int[] Masteries;
        public int Prestige;
        public string Title;
        public string Desc;
        public List<PlayerOperatorEventModel> EventModels;
        public Dictionary<StringID, string> LocalizationTable;

        protected CommanderModel()
        {
        }

        public static CommanderModel CreateDebug()
        {
            CommanderModel model = new CommanderModel();
            model.Commander = CommanderName.Zagara;
            model.Masteries = new int[6];
            model.Prestige = 0;
            model.EventModels = new List<PlayerOperatorEventModel>();
            model.LocalizationTable = new Dictionary<StringID, string>();
            return model;
        }
    }
}