using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class TransparentTipDialog : MonoBehaviour
    {
        /* field */
        [SerializeField]
        private Canvas m_Canvas;
        [SerializeField]
        private Image m_Image;
        [SerializeField]
        private Sprite m_HideAllAndRaycastIgnore;
        [SerializeField]
        private Sprite m_TopMostAndBlockRaycast;
        [SerializeField]
        private Sprite m_TopMostAndRaycastIgnore;

        private TransparentWindow m_TransparentWindow;
        private float m_FadeOut;
        private WindowState m_OldWindowState;

        private void Awake()
        {
            m_Canvas.worldCamera = Camera.main;
            m_OldWindowState = WindowState.None;
            m_TransparentWindow = Camera.main.GetComponent<TransparentWindow>();
        }

        private void Update()
        {
            m_Image.enabled = m_FadeOut > 0f;
            m_FadeOut -= Time.deltaTime;
            if (m_OldWindowState == m_TransparentWindow.WindowState)
                return;
            m_OldWindowState = m_TransparentWindow.WindowState;
            m_FadeOut = 1.5f;
            switch (m_OldWindowState)
            {
                case WindowState.TopMostAndRaycastIgnore:
                    m_Image.sprite = m_TopMostAndRaycastIgnore;
                    break;
                case WindowState.TopMostAndBlockRaycast:
                    m_Image.sprite = m_TopMostAndBlockRaycast;
                    break;
                case WindowState.HideAllAndRaycastIgnore:
                    m_Image.sprite = m_HideAllAndRaycastIgnore;
                    break;
                default:
                    break;
            }
        }
    }
}