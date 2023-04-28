using System;
using Game.Model;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tween;

namespace Game.UI
{
    public class CoopTimelineDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        public bool DestroyFlag { get; set; }
        public string PrefabPath { get; set; }

        [SerializeField]
        private GameObject m_AttackWaveTemplate;
        [SerializeField]
        private GameObject m_HintItemTemplate;

        private Tweener m_MapTimeRecognizeTweener;
        private float m_LastParseTime = 0f;
        private volatile float m_MapTimeSeconds;

        private CoopTimeline m_CoopTimeline;
        private Dictionary<Guid, IEventView> m_ViewReference;

        private void Awake()
        {
            Application.targetFrameRate = 10;
        }

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