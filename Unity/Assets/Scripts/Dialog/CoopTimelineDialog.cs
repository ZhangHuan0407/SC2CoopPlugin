using System;
using Game.Model;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tween;
using Table;
using Game.OCR;
using System.Text.RegularExpressions;

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
        private RectTransform m_RectTrans;

        [Header("Event Model View")]
        [SerializeField]
        private GameObject m_AttackWaveTemplate;
        [SerializeField]
        private GameObject m_MapTriggerTemplate;
        [SerializeField]
        private GameObject m_PlayerOperatorTemplate;
        [SerializeField]
        private GameObject m_PlayerTechnologyTemplate;
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

        [Header("Debug")]
        [SerializeField]
        private Text m_DebugText;

        public bool HaveSyncMapTime;
        private Tweener m_MapTimeRecognizeTweener;
        private float m_LastParseTime;
        private volatile float m_MapTimeSeconds;

        private CoopTimeline m_CoopTimeline;
        private Dictionary<Guid, IEventView> m_ViewReference;

        private DrawGizmosDialog m_DrawGizmosDialog;

        private void Awake()
        {
            Application.targetFrameRate = 24;

            OCR.RectAnchor rectAnchor = Global.UserSetting.RectPositions[RectAnchorKey.PluginDialog];
            m_RectTrans.anchoredPosition = new Vector2(rectAnchor.Left, -rectAnchor.Top);
            m_RectTrans.sizeDelta = new Vector2(rectAnchor.Width, rectAnchor.Height);

            m_AttackWaveTemplate.SetActive(false);
            m_MapTriggerTemplate.SetActive(false);
            m_PlayerOperatorTemplate.SetActive(false);
            m_PlayerTechnologyTemplate.SetActive(false);

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
            m_CoopTimeline.AI = TableManager.ModelTable.InstantiateModel<AIModel>("AI_Template");

            // todo 重写此处的选择逻辑，如果OCR已经给出了结果，默认使用OCR给出的结果
            m_CoopTimeline.Map = TableManager.ModelTable.InstantiateModel<MapModel>("UnknownMap");
            int mapNameIndex = 0;
            for (int i = 0; i < m_MapNameDataList.Count; i++)
                if (m_MapNameDataList[i].Item1 == m_CoopTimeline.Map.MapName)
                {
                    mapNameIndex = i;
                    break;
                }
            m_MapDropdown.SetValueWithoutNotify(mapNameIndex);

            int mapSubTypeIndex = 0;
            for (int i = 0; i < m_SubTypeDataList.Count; i++)
                if (m_SubTypeDataList[i].Item1 == m_CoopTimeline.Map.MapSubType)
                {
                    mapSubTypeIndex = i;
                    break;
                }
            m_SubTypeDropdown.SetValueWithoutNotify(mapSubTypeIndex);

            m_CoopTimeline.Commander = TableManager.ModelTable.InstantiateModel<CommanderPipeline>("CommanderPipeline_Template");

            m_ViewReference = new Dictionary<Guid, IEventView>();
        }

        private void Start()
        {
            if (Global.UserSetting.IsProgrammer)
            {
                m_DrawGizmosDialog = CameraCanvas.PushDialog(GameDefined.DrawGizmosDialogPath) as DrawGizmosDialog;
                m_DrawGizmosDialog.DrawRectAnchor(Global.UserSetting.RectPositions[RectAnchorKey.MapTime]);
            }
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
            if (HaveSyncMapTime)
                m_MapTimeSeconds += Time.deltaTime * 1.4f;
            if (m_LastParseTime > 1f)
            {
                m_LastParseTime = 0f;
                int seconds = -1;
                //MapTimeParseResult result = MapTimeParseResult.Unknown;
                RectAnchor rectAnchor = Global.UserSetting.RectPositions[RectAnchorKey.MapTime];
                bool debug = Global.UserSetting.IsProgrammer;
                m_MapTimeRecognizeTweener = Global.BackThread.WaitingBackThreadTweener(() =>
                {
                    RecognizeWindowArea_Request.Task[] tasks = new RecognizeWindowArea_Request.Task[1]
                    {
                        new RecognizeWindowArea_Request.Task()
                        {
                            RectAnchor = rectAnchor,
                            Tag = "MapTime",
                        }
                    };
                    var task = OCRConnectorA.Instance.SendRequestAsync<RecognizeWindowArea_Response>(ProtocolId.RecognizeWindowArea,
                                                                                                     new RecognizeWindowArea_Request(tasks, debug));
                    (HeadData, RecognizeWindowArea_Response) response = task.GetAwaiter().GetResult();
                    if (response.Item1.StatusCode == ErrorCode.OK)
                    {
                        RecognizeWindowArea_Response.Result[] list = response.Item2.ResultList;
                        if (list.Length > 0 && list[0].Contents.Length == 1 &&
                            Regex.IsMatch(list[0].Contents[0], "[0-9\\.:,]{3,}"))
                        {
                            HaveSyncMapTime = true;
                            string content = list[0].Contents[0].Replace(".", "").Replace(":", "").Replace(",", "");
                            int value = int.Parse(content);
                            List<int> numberSymbols = new List<int>();
                            while (value > 0)
                            {
                                numberSymbols.Add(value % 10);
                                value /= 10;
                            }
                            seconds = 0;
                            int baseValue = 1;
                            for (int i = 0; i < numberSymbols.Count; i += 2)
                            {
                                int numberSymbolA = numberSymbols[i];
                                int numberSymbolB = 0;
                                if (i + 1 < numberSymbols.Count)
                                {
                                    numberSymbolB = numberSymbols[i + 1];
                                    if (numberSymbolB >= 7)
                                        seconds = int.MinValue;
                                }

                                seconds += baseValue * (numberSymbolA + numberSymbolB * 10);
                                baseValue *= 60;
                            }
                        }
                        if (seconds >= 0)
                        {
                            HaveSyncMapTime = true;
                            if (Mathf.FloorToInt(seconds + 0.1f) != m_MapTimeSeconds)
                            {
                                if (Mathf.Abs(seconds - m_MapTimeSeconds) > 1.5f)
                                    m_MapTimeSeconds = seconds + 0.1f;
                                else
                                    m_MapTimeSeconds = Mathf.Lerp(m_MapTimeSeconds, seconds + 0.1f, 0.3f) ;
                            }
                        }
                    }
                    Color color = response.Item1.StatusCode == ErrorCode.OK ? new Color(0.3f, 1f, 0.3f) : new Color(0.9f, 0.3f, 0.3f);
                    Global.BackThread.RunInMainThread(() =>
                    {
                        m_DebugText.color = color;
                        m_DebugText.text = m_MapTimeSeconds.ToString("0.0");
                    }, BackThread.UpdateMode.AsSoonAsPossible);
                })
                    .DoIt();
            }

            RebuildView();
        }
        private void RebuildView()
        {
            m_CoopTimeline.Update(m_MapTimeSeconds);
            IReadOnlyList<IEventModel> eventModels = m_CoopTimeline.EventModels;

            HashSet<Guid> set = new HashSet<Guid>();
            for (int i = 0; i < eventModels.Count; i++)
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
                RectTransform eventViewRectTramsform = (eventView.gameObject.transform as RectTransform);
                float startTime = Time.time;
                eventViewRectTramsform.DoLocalScale(new Vector3(1f, 0f, 1f), 0.3f)
                                      .OnUpdate(() =>
                                      {
                                          eventView.gameObject.GetComponent<CanvasGroup>().alpha = 1f - (Time.time - startTime) / 0.18f;
                                      })
                                      .OnComplete(() => UnityEngine.Object.Destroy(eventView.gameObject))
                                      .DoIt();
                
                m_ViewReference.Remove(deleteGuidList[i]);
            }
            for (int i = 0; i < eventModels.Count; i++)
            {
                IEventModel eventModel = eventModels[i];
                IEventView eventView = m_ViewReference[eventModel.Guid];
                eventView.UpdateView(m_MapTimeSeconds);
            }
            LayoutRebuilder.MarkLayoutForRebuild(m_EventModelParentTrans as RectTransform);
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
                    template = m_PlayerOperatorTemplate;
                    break;
                case MapTriggerEventModel mapTriggerEventModel:
                    template = m_MapTriggerTemplate;
                    break;
                case PlayerTechnologyEventModel playerTechnologyEventModel:
                    template = m_PlayerTechnologyTemplate;
                    break;
                default:
                    break;
            }
            GameObject go = Instantiate(template, m_EventModelParentTrans);
            go.SetActive(true);
            IEventView eventView = go.GetComponent<IEventView>();
            try
            {
                eventView.SetModel(eventModel, m_CoopTimeline);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return eventView;
        }

        private void OnClickButtonBack()
        {
            LogService.System(nameof(CoopTimelineDialog), nameof(OnClickButtonBack));
            if (m_DrawGizmosDialog)
                CameraCanvas.PopDialog(m_DrawGizmosDialog);
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
                        m_CoopTimeline.RebuildEventModels = true;
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
            if (m_CoopTimeline.Map.MapSubType != mapSubType)
            {
                m_CoopTimeline.Map.MapSubType = mapSubType;
                m_CoopTimeline.RebuildEventModels = true;
            }
        }
    }
}