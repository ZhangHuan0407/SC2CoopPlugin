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

        [SerializeField]
        private int m_BigHybrid;
        public int BigHybrid
        {
            get => m_BigHybrid;
            set => m_BigHybrid = value;
        }

        [SerializeField]
        private int m_SmallHybrid;
        public int SmallHybrid
        {
            get => m_SmallHybrid;
            set => m_SmallHybrid = value;
        }

        [SerializeField]
        private int m_Technology;
        public int Technology
        {
            get => m_Technology;
            set => m_Technology = value;
        }

#if UNITY_EDITOR
        [SerializeField]
        private string m_Annotation;
        public string Annotation
        {
            get => m_Annotation;
            set => m_Annotation = value;
        }
#endif

        public MapTriggerEventModel()
        {
            m_Technology = -1;
        }

        public bool SkipEvent(CoopTimeline timeline) => (timeline.Map.MapSubType & MapSubType) == 0;
    }
}