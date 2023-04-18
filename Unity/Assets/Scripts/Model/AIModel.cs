using System;
using Table;

namespace Game.Model
{
    [Serializable]
    public class AIModel
    {
        public AmonAIName AIName;

        public static AIModel CreateDebug()
        {
            throw new NotImplementedException();
        }
    }
}