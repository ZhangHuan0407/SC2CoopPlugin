using System;
using Game.Model;
using UnityEngine;
using UnityEngine.UI;
using Table;

namespace Game.UI
{
    public class AttackWaveEventView : MonoBehaviour, IEventView
    {
        [SerializeField]
        private Image[] m_Icon;
        [SerializeField]
        private Image m_Progress;

        private AttackWaveEventModel m_Model;
        public Guid Guid => m_Model.Guid;

        public void UpdateView(float time)
        {
            m_Progress.fillAmount = Mathf.Clamp01((time - m_Model.StartTime) / (m_Model.TriggerTime - m_Model.StartTime));
            int countdown = Mathf.FloorToInt((m_Model.TriggerTime - time) * 10f);
            m_Progress.color = time >= m_Model.TriggerTime ? Color.red : Color.green;
        }

        public void SetModel(IEventModel eventModel, CoopTimeline coopTimeline)
        {
            m_Model = eventModel as AttackWaveEventModel;
            AttackWaveTable.Entry attackWaveEntry = TableManager.AttackWaveTable[coopTimeline.AI.AIName, m_Model.Technology];
            string[] iconList = new string[attackWaveEntry.UnitID.Length];
            for (int i = 0; i < attackWaveEntry.UnitID.Length; i++)
            {
                int unitID = attackWaveEntry.UnitID[i];
                UnitTable.Entry unitEntry = TableManager.UnitTable.Data[unitID];
                iconList[i] = unitEntry.Texture;
            }
            SetIconList(iconList);
        }
        private void SetIconList(params string[] iconPathList)
        {
            for (int i = 0; i < m_Icon.Length; i++)
            {
                if (i < iconPathList.Length)
                {
                    m_Icon[i].enabled = true;
                    m_Icon[i].sprite = Resources.Load<Sprite>(iconPathList[i]);
                }
                else
                    m_Icon[i].enabled = false;
            }
        }
    }
}