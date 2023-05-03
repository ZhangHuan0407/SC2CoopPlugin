using System;
using System.Collections.Generic;
using Table;
using UnityEngine;

namespace Game.Model
{
    [Serializable]
    public class MapModel
    {
        [SerializeField]
        private MapName m_MapName;
        public MapName MapName
        {
            get => m_MapName;
            set => m_MapName = value;
        }

        [SerializeField]
        private IEventModel[] m_EventModels;
        public IEventModel[] EventModels
        {
            get => m_EventModels;
            set => m_EventModels = value;
        }

        [NonSerializedAttribute]
        private MapSubType m_MapSubType;
        public MapSubType MapSubType
        {
            get => m_MapSubType;
            set => m_MapSubType = value;
        }

        public MapModel()
        {
            MapSubType = MapSubType.AorB;
        }

        public IList<IEventModel> BuildEventModels(CoopTimeline coopTimeline)
        {
            List<IEventModel> eventModels = new List<IEventModel>();
            eventModels.AddRange(EventModels);
            return eventModels;
        }
    }
}