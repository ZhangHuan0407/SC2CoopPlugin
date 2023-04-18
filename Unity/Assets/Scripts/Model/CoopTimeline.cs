using System;
using Game.UI;

namespace Game.Model
{
    [Serializable]
    public class CoopTimeline
    {
        public AIModel AI;
        public MapModel Map;
        public CommanderModel Commander;
        public float Time;

        public void Update(TestDialog testDialog)
        {

        }
    }
}