using System;
using UnityEngine;

namespace Game.Model
{
    [Serializable]
    public class PlayerTechnologyEventModel : IEventModel
    {
        [SerializeField]
        private int m_TechnologyID;
        public int TechnologyID
        {
            get => m_TechnologyID;
            set => m_TechnologyID = value;
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

        public PlayerTechnologyEventModel()
        {
        }

        public bool SkipEvent(CoopTimeline timeline) => false;
    }
}