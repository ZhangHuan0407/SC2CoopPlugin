using System;
using Table;

namespace Game.Model
{
    [Serializable]
    public class MapModel
    {
        public MapName MapName;
        public AttackWaveEventModel[] EventModels;

        public static MapModel CreateDebug()
        {
            MapModel model = new MapModel();
            model.MapName = MapName.RiftsToKorhal;
            model.EventModels = new AttackWaveEventModel[]
            {
            };
            return model;
        }
    }
}