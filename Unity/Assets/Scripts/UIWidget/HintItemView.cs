using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class HintItemView : MonoBehaviour
    {
        [SerializeField]
        private Image m_Icon;
        [SerializeField]
        private Text m_Countdown;
        [SerializeField]
        private Image m_Progress;

        public void SetIcon(string iconPath)
        {
            m_Icon.sprite = Resources.Load<Sprite>(iconPath);
        }

        public void SetCountdown(float time, float startTime, float endTime)
        {
            m_Progress.fillAmount = Mathf.Clamp01((time - startTime) / (endTime - startTime));
            int countdown = Mathf.FloorToInt((endTime - time) * 10f);
            m_Countdown.text = $"{Math.Sign(countdown)}{(countdown > 0 ? countdown : -countdown) / 10}.{countdown % 10}";
            m_Progress.color = time >= endTime ? Color.red : Color.green;
        }
    }
}