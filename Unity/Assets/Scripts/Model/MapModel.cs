using System;
using System.Collections.Generic;
using Table;

namespace Game.Model
{
    [Serializable]
    public class MapModel
    {
        public MapName MapName;
        public AttackWaveEventModel[] EventModels;
        public MapSubType MapSubType;

        public static MapModel CreateDebug()
        {
            MapModel model = new MapModel();
            model.MapName = MapName.RiftsToKorhal;
            model.EventModels = new AttackWaveEventModel[]
            {
            };
            model.MapSubType = MapSubType.AorB;
            return model;
        }

        public IList<IEventModel> BuildEventModels(CoopTimeline coopTimeline)
        {
            List<IEventModel> eventModels = new List<IEventModel>();
            eventModels.AddRange(EventModels);
            return eventModels;
        }
    }
}