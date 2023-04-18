using System;
using Table;

namespace Game.Model
{
    [Serializable]
    public class CommanderModel
    {
        public CommanderName Commander;

        public PlayerOperatorEventModel[] EventModels;

        public static CommanderModel CreateDebug()
        {
            CommanderModel model = new CommanderModel();
            model.Commander = CommanderName.Zagara;
            model.EventModels = new PlayerOperatorEventModel[]
            {

            };
            return model;
        }
    }
}