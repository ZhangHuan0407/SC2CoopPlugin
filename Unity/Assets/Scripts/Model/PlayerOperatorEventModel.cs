using System;
using UnityEngine;

namespace Game.Model
{
    [Serializable]
    public class PlayerOperatorEventModel : IEventModel
    {
        [SerializeField]
        private int m_UnitID;
        public int UnitID
        {
            get => m_UnitID;
            private set => m_UnitID = value;
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

        public PlayerOperatorEventModel()
        {
        }

        public bool NeedShowView(float time)
        {
            throw new NotImplementedException();
        }
    }
}