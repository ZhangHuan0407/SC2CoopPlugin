using System;
using Game.Model;
using UnityEngine;
using UnityEngine.UI;
using Table;

namespace Game.UI
{
    public class MapTriggerEventView : MonoBehaviour, IEventView
    {
        [SerializeField]
        private Image[] m_Icon;
        [SerializeField]
        private Image m_Progress;
        [SerializeField]
        private Text m_Countdown;

        private MapTriggerEventModel m_Model;
        public Guid Guid => m_Model.Guid;

        public void UpdateView(float time)
        {
            m_Progress.fillAmount = Mathf.Clamp01((time - m_Model.StartTime) / (m_Model.TriggerTime - m_Model.StartTime));
            int countdown = Mathf.FloorToInt((m_Model.TriggerTime - time) * 10f);
            m_Countdown.text = $"{countdown / 10f:0.0}";
            m_Progress.color = time >= m_Model.TriggerTime ? Color.red : Color.green;
        }

        public void SetModel(IEventModel eventModel, CoopTimeline coopTimeline)
        {
            m_Model = eventModel as MapTriggerEventModel;
            AttackWaveTable.Entry attackWaveEntry = TableManager.AttackWaveTable[coopTimeline.AI.AIName, m_Model.Technology];
            for (int i = 0; i < m_Icon.Length; i++)
            {
                if (i < attackWaveEntry.UnitID.Length)
                {
                    m_Icon[i].gameObject.SetActive(true);
                    UnitTable.Entry entry = TableManager.UnitTable[attackWaveEntry.UnitID[i]];
                    if (entry != null)
                        m_Icon[i].sprite = entry.LoadTexture();
                    else
                        m_Icon[i].sprite = Resources.Load<Sprite>("Textures/Unknown");
                }
                else
                {
                    m_Icon[i].gameObject.SetActive(false);
                }
            }
        }
    }
}