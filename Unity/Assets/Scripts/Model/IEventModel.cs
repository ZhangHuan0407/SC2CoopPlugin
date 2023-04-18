using System;

namespace Game.Model
{
    public interface IEventModel
    {
        Guid Guid { get; }
        float StartTime { get; }
        float TriggerTime { get; }
        float EndTime { get; }

        bool SkipEvent(CoopTimeline timeline);
    }
}