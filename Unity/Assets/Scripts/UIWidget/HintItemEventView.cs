using System;
using Game.Model;
using Table;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class HintItemView : MonoBehaviour, IEventView
    {
        [SerializeField]
        private Image m_Icon;
        [SerializeField]
        private Text m_Countdown;
        [SerializeField]
        private Image m_Progress;

        private PlayerOperatorEventModel m_Model;
        public Guid Guid => m_Model.Guid;

        public void Update(float time)
        {
            m_Progress.fillAmount = Mathf.Clamp01((time - m_Model.StartTime) / (m_Model.TriggerTime - m_Model.StartTime));
            int countdown = Mathf.FloorToInt((m_Model.TriggerTime - time) * 10f);
            m_Progress.color = time >= m_Model.TriggerTime ? Color.red : Color.green;
        }

        public void SetModel(IEventModel eventModel)
        {
            m_Model = eventModel as PlayerOperatorEventModel;
            int unitID = m_Model.UnitID;

        }
        private void SetIcon(string iconPath)
        {
            m_Icon.sprite = Resources.Load<Sprite>(iconPath);
        }
    }
}