using System;
using System.Collections.Generic;
using Table;

namespace Game.Model
{
    [Serializable]
    public class CommanderModel
    {
        public CommanderName Commander;

        public List<PlayerOperatorEventModel> EventModels;

        protected CommanderModel()
        {
        }

        public static CommanderModel CreateDebug()
        {
            CommanderModel model = new CommanderModel();
            model.Commander = CommanderName.Zagara;
            model.EventModels = new List<PlayerOperatorEventModel>();
            return model;
        }
    }
}