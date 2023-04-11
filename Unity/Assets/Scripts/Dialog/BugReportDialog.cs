using System;
using Game.UI;
using UnityEngine;

namespace Game
{
    public class BugReportDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        public string PrefabPath { get; set; }

        public void Hide()
        {
            m_Canvas.enabled = false;
        }

        public void Show()
        {
            m_Canvas.enabled = true;
        }
    }
}