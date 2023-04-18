using System;
using Table;

namespace Game.Model
{
    [Serializable]
    public class MapModel
    {
        public MapName MapName;

        public static MapModel CreateDebug()
        {
            throw new NotImplementedException();
        }
    }
}