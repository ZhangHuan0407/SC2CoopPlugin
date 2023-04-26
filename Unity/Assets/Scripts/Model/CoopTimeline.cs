using System;
using System.Collections.Generic;
using Game.OCR;
using Game.UI;

namespace Game.Model
{
    public class CoopTimeline
    {
        public AIModel AI;
        public MapModel Map;
        public CommanderPipeline Commander;
        public float MapTime;
        public List<IEventModel> allEventModels;
        public bool RebuildEventModels;

        public CoopTimeline()
        {
            MapTime = 0f;
            allEventModels = new List<IEventModel>(100);
            RebuildEventModels = true;
        }

        public void Update(TestDialog testDialog)
        {
            if (RebuildEventModels)
            {
                RebuildEventModels = false;
                allEventModels.Clear();
                allEventModels.AddRange(AI.EventModels);
                allEventModels.AddRange(Map.BuildEventModels(this));
                allEventModels.AddRange(Commander.EventModels);
                allEventModels.Sort((l, r) =>
                {
                    int compare = l.StartTime.CompareTo(r.StartTime);
                    if (compare == 0)
                        compare = l.Guid.CompareTo(r.Guid);
                    return compare;
                });
            }
            List<IEventModel> eventModels = new List<IEventModel>();
            for (int i = 0; i < allEventModels.Count; i++)
            {
                IEventModel eventModel = allEventModels[i];
                if (eventModel.EndTime < MapTime ||
                    eventModel.StartTime > MapTime)
                    continue;
                if (eventModel.SkipEvent(this))
                    continue;
                eventModels.Add(eventModel);
            }
            testDialog.UpdateModelView(eventModels.ToArray(), MapTime);
        }
    }
}