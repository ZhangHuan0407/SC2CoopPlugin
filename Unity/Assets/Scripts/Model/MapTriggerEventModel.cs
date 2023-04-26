using System;
using Table;
using UnityEngine;

namespace Game.Model
{
    [Serializable]
    public class MapTriggerEventModel : IEventModel
    {
        [SerializeField]
        private Guid m_Guid;
        public Guid Guid
        {
            get => m_Guid;
            set => m_Guid = value;
        }

        [SerializeField]
        private float m_StartTime;
        public float StartTime
        {
            get => m_StartTime;
            set => m_StartTime = value;
        }

        [SerializeField]
        private float m_TriggerTime;
        public float TriggerTime
        {
            get => m_TriggerTime;
            set => m_TriggerTime = value;
        }

        [SerializeField]
        private float m_EndTime;
        public float EndTime
        {
            get => m_EndTime;
            set => m_EndTime = value;
        }

        [SerializeField]
        private MapSubType m_MapSubType;
        public MapSubType MapSubType
        {
            get => m_MapSubType;
            set => m_MapSubType = value;
        }

        [SerializeField]
        private string m_Texture;
        public string Texture
        {
            get => m_Texture;
            set => m_Texture = value;
        }

        [SerializeField]
        private string m_Desc;
        public string Desc
        {
            get => m_Desc;
            set => m_Desc = value;
        }

        public MapTriggerEventModel()
        {
        }

        public bool SkipEvent(CoopTimeline timeline) => (timeline.Map.MapSubType & MapSubType) == 0;
    }
}