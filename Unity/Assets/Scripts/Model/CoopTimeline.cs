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
        private List<IEventModel> m_AllEventModels;
        public bool RebuildEventModels { get; set; }

        public CoopTimeline()
        {
            MapTime = 0f;
            m_AllEventModels = new List<IEventModel>(100);
            RebuildEventModels = true;
        }

        public void Update(TestDialog testDialog)
        {
            if (RebuildEventModels)
            {
                RebuildEventModels = false;
                m_AllEventModels.Clear();
                m_AllEventModels.AddRange(AI.EventModels);
                m_AllEventModels.AddRange(Map.BuildEventModels(this));
                m_AllEventModels.AddRange(Commander.EventModels);
                m_AllEventModels.Sort((l, r) =>
                {
                    int compare = l.StartTime.CompareTo(r.StartTime);
                    if (compare == 0)
                        compare = l.Guid.CompareTo(r.Guid);
                    return compare;
                });
            }
            List<IEventModel> eventModels = new List<IEventModel>();
            for (int i = 0; i < m_AllEventModels.Count; i++)
            {
                IEventModel eventModel = m_AllEventModels[i];
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