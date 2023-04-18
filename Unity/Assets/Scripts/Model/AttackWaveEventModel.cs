using System;
using Table;
using UnityEngine;

namespace Game.Model
{
    [Serializable]
    public class AttackWaveEventModel : IEventModel
    {
        [SerializeField]
        private AmonAIName m_AIName;
        public AmonAIName AIName
        {
            get => m_AIName;
            set => m_AIName = value;
        }

        [SerializeField]
        private int m_Technology;
        public int Technology
        {
            get => m_Technology;
            set => m_Technology = value;
        }

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
        private bool m_Hide;
        public bool Hide
        {
            get => m_Hide;
            set => m_Hide = value;
        }

        public AttackWaveEventModel()
        {
        }

        public bool SkipEvent(CoopTimeline timeline) => false;
    }
}