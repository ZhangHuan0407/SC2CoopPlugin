using System;
using Game.Model;
using Table;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class PlayerOperatorEventView : MonoBehaviour, IEventView
    {
        [SerializeField]
        private Image[] m_Icon;
        [SerializeField]
        private Text m_Countdown;
        [SerializeField]
        private Image m_Progress;

        private PlayerOperatorEventModel m_Model;
        public Guid Guid => m_Model.Guid;

        public void UpdateView(float time)
        {
            m_Progress.fillAmount = Mathf.Clamp01((time - m_Model.StartTime) / (m_Model.TriggerTime - m_Model.StartTime));
            int countdown = Mathf.RoundToInt((m_Model.TriggerTime - time) * 10f);
            m_Countdown.text = $"{countdown / 10f:0.0}";
            m_Progress.color = time >= m_Model.TriggerTime ? Color.red : Color.green;
        }

        public void SetModel(IEventModel eventModel, CoopTimeline coopTimeline)
        {
            m_Model = eventModel as PlayerOperatorEventModel;
            int[] unitID = m_Model.UnitIDList;
            for (int i = 0; i < m_Icon.Length; i++)
            {
                if (i < unitID.Length)
                {
                    m_Icon[i].gameObject.SetActive(true);
                    UnitTable.Entry entry = TableManager.UnitTable[unitID[i]];
                    if (entry != null)
                        m_Icon[i].sprite = entry.LoadTexture();
                    else
                        m_Icon[i].sprite = ResourcesInterface.Load<Sprite>("Textures/Unknown");
                }
                else
                {
                    m_Icon[i].gameObject.SetActive(false);
                }
            }
        }
    }
}