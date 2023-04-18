using System;

namespace Game.Model
{
    public interface IEventModel
    {
        float StartTime { get; }
        float EndTime { get; }

        bool NeedShowView(float time);
    }
}