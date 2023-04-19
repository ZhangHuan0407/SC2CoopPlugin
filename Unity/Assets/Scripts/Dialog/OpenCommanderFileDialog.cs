using System;
using UnityEngine;

namespace Game.UI
{
    public class OpenCommanderFileDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        public bool DestroyFlag { get; set; }
        public string PrefabPath { get; set; }

        public DialogResult DialogResult { get; set; }
        public string FilePath { get; set; }

        public void Hide()
        {
            m_Canvas.enabled = false;
        }

        public void Show()
        {
            m_Canvas.enabled = true;
        }

        private void Awake()
        {
            
        }
    }
}