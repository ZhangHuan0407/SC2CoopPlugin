using System;
using Game.Model;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tween;
using Table;

namespace Game.UI
{
    public class CoopTimelineDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        public bool DestroyFlag { get; set; }
        public string PrefabPath { get; set; }

        private RectTransform m_RectTrans;

        [Header("Event Model View")]
        [SerializeField]
        private GameObject m_AttackWaveTemplate;
        [SerializeField]
        private GameObject m_MapTriggerTemplate;
        [SerializeField]
        private GameObject m_PlayerOperatorTemplate;
        [SerializeField]
        private Transform m_EventModelParentTrans;

        [Header("Tool")]
        [SerializeField]
        private Button m_BackButton;
        [SerializeField]
        private Button m_SelectCommanderButton;
        [SerializeField]
        private Dropdown m_MapDropdown;
        private List<(MapName, string)> m_MapNameDataList;
        [SerializeField]
        private Dropdown m_SubTypeDropdown;
        private List<(MapSubType, string)> m_SubTypeDataList;

        private Tweener m_MapTimeRecognizeTweener;
        private float m_LastParseTime;
        private volatile float m_MapTimeSeconds;

        private CoopTimeline m_CoopTimeline;
        private Dictionary<Guid, IEventView> m_ViewReference;

        private void Awake()
        {
            Application.targetFrameRate = 10;

            OCR.RectAnchor rectAnchor = Global.UserSetting.RectPositions[RectAnchorKey.PluginDialog];
            m_RectTrans.anchoredPosition = new Vector2(rectAnchor.Left, rectAnchor.Top);
            m_RectTrans.sizeDelta = new Vector2(rectAnchor.Width, rectAnchor.Height);

            m_AttackWaveTemplate.SetActive(false);
            m_MapTriggerTemplate.SetActive(false);
            m_PlayerOperatorTemplate.SetActive(false);

            m_MapTimeRecognizeTweener = null;
            m_LastParseTime = 0f;
            m_MapTimeSeconds = 0f;

            m_BackButton.onClick.AddListener(OnClickButtonBack);
            m_SelectCommanderButton.onClick.AddListener(OnClickButtonSelectCommander);
            m_MapDropdown.ClearOptions();
            m_MapNameDataList = new List<(MapName, string)>();
            foreach (MapName mapName in Enum.GetValues(typeof(MapName)))
            {
                string content = TableManager.LocalizationTable[mapName];
                m_MapNameDataList.Add((mapName, content));
                m_MapDropdown.options.Add(new Dropdown.OptionData(content));
            }
            m_MapDropdown.onValueChanged.AddListener(OnMapDropdown_ValueChanged);
            m_SubTypeDropdown.ClearOptions();
            m_SubTypeDataList = new List<(MapSubType, string)>()
            {
                (MapSubType.A, string.Empty),
                (MapSubType.B, string.Empty),
                (MapSubType.AorB, string.Empty),
            };
            for (int i = 0; i < m_SubTypeDataList.Count; i++)
            {
                var pair = m_SubTypeDataList[i];
                string content = TableManager.LocalizationTable[pair.Item1];
                pair.Item2 = content;
                m_SubTypeDataList[i] = pair;
                m_SubTypeDropdown.options.Add(new Dropdown.OptionData(content));
            }
            m_SubTypeDropdown.onValueChanged.AddListener(OnSubTypeDropdown_ValueChanged);

            m_CoopTimeline = new CoopTimeline();
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

        private void Update()
        {
            
        }

        private void OnClickButtonBack()
        {
            LogService.System(nameof(CoopTimelineDialog), nameof(OnClickButtonBack));
            CameraCanvas.PopDialog(this);
            CameraCanvas.PushDialog(GameDefined.MainManuDialog);
        }
        private void OnClickButtonSelectCommander()
        {
            LogService.System(nameof(CoopTimelineDialog), nameof(OnClickButtonSelectCommander));
            OpenCommanderFileDialog dialog = CameraCanvas.PushDialog(GameDefined.OpenCommanderFileDialog) as OpenCommanderFileDialog;
            Hide();
            LogicTween.WaitUntil(() => dialog.DestroyFlag)
                .OnComplete(() =>
                {
                    if (dialog.DialogResult == DialogResult.OK)
                    {
                        string id = dialog.CommanderPipelineId;
                        LogService.System("CommanderPipelineTable.Instantiate", id);
                        m_CoopTimeline.Commander = TableManager.CommanderPipelineTable.Instantiate(id);
                    }
                    Show();
                })
                .DoIt();
        }
        private void OnMapDropdown_ValueChanged(int index)
        {
            LogService.System(nameof(CoopTimelineDialog), $"{nameof(OnMapDropdown_ValueChanged)} index: {index}");
            MapName mapName = m_MapNameDataList[index].Item1;
            if (m_CoopTimeline.Map.MapName != mapName)
            {
                m_CoopTimeline.Map = TableManager.ModelTable.InstantiateModel<MapModel>(mapName);
                m_CoopTimeline.RebuildEventModels = true;
            }
        }
        private void OnSubTypeDropdown_ValueChanged(int index)
        {
            LogService.System(nameof(CoopTimelineDialog), $"{nameof(OnSubTypeDropdown_ValueChanged)} index: {index}");
            MapSubType mapSubType = m_SubTypeDataList[index].Item1;
            m_CoopTimeline.Map.MapSubType = mapSubType;
            m_CoopTimeline.RebuildEventModels = true;
        }
    }
}