using System;
using System.Collections.Generic;
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
            List<IEventModel> allEventModels = new List<IEventModel>();
            allEventModels.AddRange(AI.EventModels);
            allEventModels.AddRange(Map.EventModels);
            allEventModels.AddRange(Commander.EventModels);
            List<IEventModel> eventModels = new List<IEventModel>();
            for (int i = 0; i < allEventModels.Count; i++)
            {
                IEventModel eventModel = allEventModels[i];
                if (eventModel.EndTime < Time ||
                    eventModel.StartTime > Time)
                    continue;
                if (eventModel.SkipEvent(this))
                    continue;
                eventModels.Add(eventModel);
            }
            eventModels.Sort((l, r) =>
            {
                int compare = l.StartTime.CompareTo(r.StartTime);
                // if compare == 0, compare with guid
                return compare;
            });
            testDialog.ShowModelView(eventModels.ToArray());
        }
    }
}