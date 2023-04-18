using System;
using Table;

namespace Game.Model
{
    [Serializable]
    public class AIModel
    {
        public AmonAIName AIName;
        public AttackWaveEventModel[] EventModels;

        public static AIModel CreateDebug()
        {
            AIModel model = new AIModel();
            model.AIName = AmonAIName.RaidingParty;
            model.EventModels = new AttackWaveEventModel[]
            {

            };
            return model;
        }
    }
}