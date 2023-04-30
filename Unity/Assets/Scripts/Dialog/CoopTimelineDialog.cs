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
        private GameObject m_MapTriggerTemplate;
        [SerializeField]
        private GameObject m_PlayerOperatorTemplate;

        [SerializeField]
        private Button m_BackButton;

        private Tweener m_MapTimeRecognizeTweener;
        private float m_LastParseTime;
        private volatile float m_MapTimeSeconds;

        private CoopTimeline m_CoopTimeline;
        private Dictionary<Guid, IEventView> m_ViewReference;

        private void Awake()
        {
            Application.targetFrameRate = 10;

            m_AttackWaveTemplate.SetActive(false);
            m_MapTriggerTemplate.SetActive(false);
            m_PlayerOperatorTemplate.SetActive(false);

            m_MapTimeRecognizeTweener = null;
            m_LastParseTime = 0f;
            m_MapTimeSeconds = 0f;

            m_ViewReference = new Dictionary<Guid, IEventView>();
        }

        public void Hide()
        {
            m_Canvas.enabled = false;
        }

        public void Show()
        {
            m_Canvas.enabled = true;
        }

        public void SetCoopTimeline(CoopTimeline coopTimeline)
        {
            LogService.System(nameof(SetCoopTimeline), coopTimeline.Commander.Title);
            m_CoopTimeline = coopTimeline;
        }

        private void Update()
        {
            
        }
    }
}