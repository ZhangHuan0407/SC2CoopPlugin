using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class AttackWaveView : MonoBehaviour
    {
        [SerializeField]
        private Image[] m_Icon;
        [SerializeField]
        private Image m_Progress;

        public void SetIconList(params string[] iconPathList)
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

        public void SetCountdown(float time, float startTime, float endTime)
        {
            m_Progress.fillAmount = Mathf.Clamp01((time - startTime) / (endTime - startTime));
            int countdown = Mathf.FloorToInt((endTime - time) * 10f);
            m_Progress.color = time >= endTime ? Color.red : Color.green;
        }
    }
}