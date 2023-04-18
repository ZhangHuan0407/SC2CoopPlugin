using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Game.OCR;
using Tween;
using UnityEngine;
using UnityEngine.UI;
using Game.Model;

namespace Game.UI
{
    public class TestDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        [SerializeField]
        private Image m_DebugImage;

        public string PrefabPath { get; set; }

        private Tweener m_MapTimeRecognizeTweener;
        private float m_LastParseTime = 0f;
        private volatile float m_MapTimeSeconds;

        private DrawGizmosDialog GizmosDialog;

        private GameObject m_AttackWaveTemplate;
        private GameObject m_HintItemTemplate;

        private CoopTimeline m_CoopTimeline;
        private Dictionary<Guid, IEventView> m_ViewReference;

        private void Awake()
        {
            Application.targetFrameRate = 10;
            Camera.main.GetComponent<TransparentWindow>().SetWindowState(WindowState.TopMostAndRaycastIgnore);
            GizmosDialog = CameraCanvas.PushDialog(GameDefined.DrawGizmosDialogPath) as DrawGizmosDialog;
            GizmosDialog.DrawRectAnchor(Global.UserSetting.RectPositions[RectAnchorKey.MapTime]);

            m_MapTimeSeconds = 0f;
            m_CoopTimeline = new CoopTimeline();
            m_CoopTimeline.AI = AIModel.CreateDebug();
            m_CoopTimeline.Map = MapModel.CreateDebug();
            m_CoopTimeline.Commander = CommanderModel.CreateDebug();
            m_ViewReference = new Dictionary<Guid, IEventView>();
            RebuildView();
        }

        public void Hide()
        {
            m_Canvas.enabled = false;
        }

        public void Show()
        {
            m_Canvas.enabled = true;
        }

        private void Update()
        {
            m_LastParseTime += Time.deltaTime;
            m_MapTimeSeconds += Time.deltaTime / 1.4f;
            if (m_LastParseTime > 0.5f)
            {
                m_LastParseTime = 0f;
                int seconds = -1;
                MapTimeParseResult result = MapTimeParseResult.Unknown;
                m_MapTimeRecognizeTweener = Global.BackThread.WaitingBackThreadTweener(() =>
                {
                    RectAnchor rectAnchor = Global.UserSetting.RectPositions[RectAnchorKey.MapTime];
                    Global.MapTime.UpdateScreenShot();
                    result = Global.MapTime.TryParse(false, rectAnchor, out seconds);
                    if (result == MapTimeParseResult.WellDone)
                    {
                        if (Mathf.Abs(m_MapTimeSeconds - seconds) > 0.5f)
                            m_MapTimeSeconds = seconds;
                    }
                    else
                        LogService.Error("MapTimeRecognize MapTimeParseResult: ", result);
                })
                    .DoIt();
            }

            RebuildView();

            TransparentWindow transparentWindow = Camera.main.GetComponent<TransparentWindow>();
            if (Input.GetKey(KeyCode.Escape))
                m_DebugImage.color = Color.red;
            else if (transparentWindow.WindowState == WindowState.TopMostAndBlockRaycast)
                m_DebugImage.color = Color.green;
            else
                m_DebugImage.color = Color.blue;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (transparentWindow.WindowState == WindowState.TopMostAndRaycastIgnore)
                    transparentWindow.SetWindowState(WindowState.TopMostAndBlockRaycast);
                else
                    transparentWindow.SetWindowState(WindowState.TopMostAndRaycastIgnore);
            }
        }

        private void RebuildView()
        {
            m_CoopTimeline.Time = m_MapTimeSeconds;
            m_CoopTimeline.Update(this);
        }
        public void UpdateModelView(IEventModel[] eventModels, float time)
        {
            HashSet<Guid> set = new HashSet<Guid>();
            for (int i = 0; i < eventModels.Length; i++)
            {
                IEventModel eventModel = eventModels[i];
                if (!m_ViewReference.TryGetValue(eventModel.Guid, out _))
                    m_ViewReference.Add(eventModel.Guid, CreateView(eventModel));
                set.Add(eventModel.Guid);
            }
            List<Guid> deleteGuidList = new List<Guid>();
            foreach (Guid guid in m_ViewReference.Keys)
            {
                if (!set.Contains(guid))
                    deleteGuidList.Add(guid);
            }
            for (int i = 0; i < deleteGuidList.Count; i++)
            {
                IEventView eventView = m_ViewReference[deleteGuidList[i]];
                UnityEngine.Object.Destroy(eventView.gameObject);
                m_ViewReference.Remove(deleteGuidList[i]);
            }
            for (int i = 0; i < eventModels.Length; i++)
            {
                IEventModel eventModel = eventModels[i];
                IEventView eventView = m_ViewReference[eventModel.Guid];
                eventView.Update(time);
            }
        }
        private IEventView CreateView(IEventModel eventModel)
        {
            GameObject template = null;
            switch (eventModel)
            {
                case AttackWaveEventModel attackWaveEventModel:
                    template = m_AttackWaveTemplate;
                    break;
                case PlayerOperatorEventModel playerOperatorEventModel:
                    template = m_HintItemTemplate;
                    break;
                default:
                    break;
            }
            GameObject go = Instantiate(m_AttackWaveTemplate);
            IEventView eventView = go.GetComponent<AttackWaveEventView>();
            eventView.SetModel(eventModel);
            return eventView;
        }

        private void OnDestroy()
        {
            CameraCanvas.PopDialog(GizmosDialog);
        }
    }
}