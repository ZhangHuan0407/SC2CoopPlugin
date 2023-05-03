using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Model
{
    public class CoopTimeline
    {
        private AIModel m_AI;
        public AIModel AI
        {
            get => m_AI;
            set => m_AI = value;
        }
        private MapModel m_Map;
        public MapModel Map
        {
            get => m_Map;
            set => m_Map = value;
        }
        private CommanderPipeline m_Commander;
        public CommanderPipeline Commander
        {
            get => m_Commander;
            set => m_Commander = value;
        }
        public float MapTime { get; private set; }
        public IReadOnlyList<IEventModel> EventModels { get; private set; }
        private List<IEventModel> m_AllEventModels;
        public bool RebuildEventModels { get; set; }

        public CoopTimeline()
        {
            MapTime = 0f;
            EventModels = Array.Empty<IEventModel>();
            m_AllEventModels = new List<IEventModel>(100);
            RebuildEventModels = true;
        }

        public void Update(float time)
        {
            if (RebuildEventModels)
            {
                RebuildEventModels = false;
                m_AllEventModels.Clear();
                m_AllEventModels.AddRange(AI.BuildEventModels(this));
                m_AllEventModels.AddRange(Map.BuildEventModels(this));
                m_AllEventModels.AddRange(Commander.EventModels);
                m_AllEventModels.Sort((l, r) =>
                {
                    int compare = l.StartTime.CompareTo(r.StartTime);
                    if (compare == 0)
                        compare = l.Guid.CompareTo(r.Guid);
                    return compare;
                });
                Debug.Log($"time: {time}, AllEventModels: {m_AllEventModels.Count}");
            }
            List<IEventModel> eventModels = new List<IEventModel>();
            MapTime = time;
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
            EventModels = eventModels.ToArray();
        }
    }
}