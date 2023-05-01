using System;

namespace Game.Model
{
    public interface IEventModel
    {
        Guid Guid { get; set; }
        float StartTime { get; set; }
        float TriggerTime { get; set; }
        float EndTime { get; set; }

        bool SkipEvent(CoopTimeline timeline);
    }
}