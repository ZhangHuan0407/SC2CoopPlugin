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
            private set => m_AIName = value;
        }

        [SerializeField]
        private int m_Technology;
        public int Technology
        {
            get => m_Technology;
            private set => m_Technology = value;
        }

        [SerializeField]
        private float m_StartTime;
        public float StartTime
        {
            get => m_StartTime;
            private set => m_StartTime = value;
        }

        [SerializeField]
        private float m_EndTime;
        public float EndTime
        {
            get => m_EndTime;
            private set => m_EndTime = value;
        }

        public AttackWaveEventModel()
        {
        }

        public bool NeedShowView(float time)
        {
            throw new NotImplementedException();
        }
    }
}