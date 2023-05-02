using System;
using System.Linq;
using System.Reflection;
using Game.Model;
using Table;
using Tween;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class EventModelEditView : MonoBehaviour
    {
        public CommanderContentDialog CommanderContentDialog 
        {
            get; 
            set;
        }
        private CommanderPipeline m_CommanderPipeline;
        private Guid m_Guid;

        [SerializeField]
        private Dropdown m_EventModelType;
        [SerializeField]
        private Text m_GuidLabel;
        [SerializeField]
        private Button m_DeleteButton;
        [SerializeField]
        private Button m_CopyButton;
        [SerializeField]
        private Button m_UpButton;
        [SerializeField]
        private Button m_DownButton;

        [Header("Time")]
        [SerializeField]
        private Transform m_TimeTrans;
        [SerializeField]
        private InputField m_StartTimeInput;
        [SerializeField]
        private InputField m_TriggerTimeInput;
        [SerializeField]
        private InputField m_EndTimeInput;

        [Header("Unit")]
        [SerializeField]
        private Transform m_UnitTrans;
        [SerializeField]
        private Button[] m_UnitButtonList;

        private void Awake()
        {
            m_EventModelType.ClearOptions();
            m_EventModelType.options.Add(new Dropdown.OptionData(nameof(PlayerOperatorEventModel)));
            m_DeleteButton.onClick.AddListener(EventModel_Delete);
            m_CopyButton.onClick.AddListener(EventModel_Copy);
            m_UpButton.onClick.AddListener(EventModel_Up);
            m_DownButton.onClick.AddListener(EventModel_Down);

            m_TimeTrans.gameObject.SetActive(false);
            m_StartTimeInput.onValueChanged.AddListener(OnStartTime_ValueChanged);
            m_TriggerTimeInput.onValueChanged.AddListener(OnTriggerTime_ValueChanged);
            m_EndTimeInput.onValueChanged.AddListener(OnEndTime_OnValueChanged);

            m_UnitTrans.gameObject.SetActive(false);
            for (int i = 0; i < m_UnitButtonList.Length; i++)
            {
                int index = i;
                m_UnitButtonList[i].onClick.AddListener(() => OnClickUnitButton(index));
            }
        }

        public void SetCommanderModel(CommanderPipeline pipeline, IEventModel eventModel)
        {
            m_CommanderPipeline = pipeline;
            name = eventModel.Guid.ToString();
            m_Guid = eventModel.Guid;
            m_GuidLabel.text = m_Guid.ToString();

            m_TimeTrans.gameObject.SetActive(true);
            m_StartTimeInput.SetTextWithoutNotify(eventModel.StartTime.ToString());
            m_TriggerTimeInput.SetTextWithoutNotify(eventModel.TriggerTime.ToString());
            m_EndTimeInput.SetTextWithoutNotify(eventModel.EndTime.ToString());
            if (eventModel is PlayerOperatorEventModel playerOperatorEventModel)
            {
                m_UnitTrans.gameObject.SetActive(true);
                for (int i = 0; i < m_UnitButtonList.Length; i++)
                {
                    Button button = m_UnitButtonList[i];
                    if (i < playerOperatorEventModel.UnitIDList.Length &&
                        TableManager.UnitTable[playerOperatorEventModel.UnitIDList[i]] is UnitTable.Entry entry)
                        (button.targetGraphic as Image).sprite = entry.LoadTexture();
                    else
                    {
                        (button.targetGraphic as Image).sprite = Resources.Load<Sprite>("Textures/plus");
                        // 自定义或高版本不兼容数据
                        if (i < playerOperatorEventModel.UnitIDList.Length)
                            playerOperatorEventModel.UnitIDList[i] = 0;
                    }
                }
            }
            else
            {
                m_UnitTrans.gameObject.SetActive(false);
            }
        }

        public void EventModel_Copy()
        {
            int dataIndex = transform.GetSiblingIndex();
            LogService.System(nameof(EventModel_Copy), $"dataIndex: {dataIndex}, guid: {m_Guid}");
            IEventModel eventModel = m_CommanderPipeline.EventModels.First(m => m.Guid == m_Guid);
            string modelString = JSONMap.ToJSON(typeof(IEventModel), eventModel).ToString();
            Guid newGuid = Guid.NewGuid();
            CommanderContentDialog.AppendRecord(nameof(EventModel_Copy),
                                                (dialog) =>
                                                {
                                                    EventModelEditView template = CommanderContentDialog.PlayerOperatorTemplateView;
                                                    EventModelEditView view = UnityEngine.Object.Instantiate(template, CommanderContentDialog.EventModelsRectTrans);
                                                    view.gameObject.SetActive(true);
                                                    IEventModel eventModel2 = JSONMap.ParseJSON<IEventModel>(JSONObject.Create(modelString));
                                                    eventModel2.Guid = newGuid;
                                                    m_CommanderPipeline.EventModels.Insert(dataIndex + 1, eventModel2);
                                                    view.CommanderContentDialog = dialog;
                                                    view.SetCommanderModel(m_CommanderPipeline, eventModel2);
                                                    view.transform.SetSiblingIndex(dataIndex + 1);
                                                },
                                                (dialog) =>
                                                {
                                                    m_CommanderPipeline.EventModels.RemoveAt(dataIndex + 1);
                                                    Transform cloneView = CommanderContentDialog.EventModelsRectTrans.GetChild(dataIndex + 1);
                                                    cloneView.SetParent(null);
                                                    Destroy(cloneView.gameObject);
                                                });
            CommanderContentDialog.Redo();
        }
        public void EventModel_Delete()
        {
            int dataIndex = transform.GetSiblingIndex();
            LogService.System(nameof(EventModel_Delete), $"dataIndex: {dataIndex}, guid: {m_Guid}");
            IEventModel eventModel = m_CommanderPipeline.EventModels.First(m => m.Guid == m_Guid);
            string modelString = JSONMap.ToJSON(typeof(IEventModel), eventModel).ToString();
            CommanderContentDialog.AppendRecord(nameof(EventModel_Delete),
                                                (dialog) =>
                                                {
                                                    EventModelEditView view = CommanderContentDialog.EventModelsRectTrans.GetChild(dataIndex).GetComponent<EventModelEditView>();
                                                    view.transform.SetParent(null);
                                                    Destroy(view.gameObject);
                                                    m_CommanderPipeline.EventModels.RemoveAt(dataIndex);
                                                },
                                                (dialog) =>
                                                {
                                                    var template = CommanderContentDialog.PlayerOperatorTemplateView;
                                                    var view = UnityEngine.Object.Instantiate(template, CommanderContentDialog.EventModelsRectTrans);
                                                    view.CommanderContentDialog = dialog;
                                                    view.gameObject.SetActive(true);
                                                    IEventModel eventModel2 = JSONMap.ParseJSON<IEventModel>(JSONObject.Create(modelString));
                                                    m_CommanderPipeline.EventModels.Insert(dataIndex, eventModel2);
                                                    view.SetCommanderModel(m_CommanderPipeline, eventModel2);
                                                    view.transform.SetSiblingIndex(dataIndex);
                                                });
            CommanderContentDialog.Redo();
        }
        private void EventModel_Up()
        {
            int dataIndex = transform.GetSiblingIndex();
            int childCount = CommanderContentDialog.EventModelsRectTrans.childCount;
            LogService.System(nameof(EventModel_Up), $"dataIndex: {dataIndex}, childCount:{childCount}, guid: {m_Guid}");
            if (dataIndex == 0)
                return;
            CommanderContentDialog.AppendRecord(nameof(EventModel_Up),
                                                (dialog) =>
                                                {
                                                    IEventModel eventModel = m_CommanderPipeline.EventModels[dataIndex];
                                                    m_CommanderPipeline.EventModels[dataIndex] = m_CommanderPipeline.EventModels[dataIndex - 1];
                                                    m_CommanderPipeline.EventModels[dataIndex - 1] = eventModel;
                                                    Transform childTrans = CommanderContentDialog.EventModelsRectTrans.GetChild(dataIndex);
                                                    childTrans.SetSiblingIndex(dataIndex - 1);
                                                },
                                                (dialog) =>
                                                {
                                                    IEventModel eventModel = m_CommanderPipeline.EventModels[dataIndex];
                                                    m_CommanderPipeline.EventModels[dataIndex] = m_CommanderPipeline.EventModels[dataIndex - 1];
                                                    m_CommanderPipeline.EventModels[dataIndex - 1] = eventModel;
                                                    Transform childTrans = CommanderContentDialog.EventModelsRectTrans.GetChild(dataIndex);
                                                    childTrans.SetSiblingIndex(dataIndex - 1);
                                                });
            CommanderContentDialog.Redo();
        }
        private void EventModel_Down()
        {
            int dataIndex = transform.GetSiblingIndex();
            int childCount = CommanderContentDialog.EventModelsRectTrans.childCount;
            LogService.System(nameof(EventModel_Down), $"dataIndex: {dataIndex}, childCount:{childCount}, guid: {m_Guid}");
            if (dataIndex == childCount - 1)
                return;
            CommanderContentDialog.AppendRecord(nameof(EventModel_Down),
                                                (dialog) =>
                                                {
                                                    IEventModel eventModel = m_CommanderPipeline.EventModels[dataIndex];
                                                    m_CommanderPipeline.EventModels[dataIndex] = m_CommanderPipeline.EventModels[dataIndex + 1];
                                                    m_CommanderPipeline.EventModels[dataIndex + 1] = eventModel;
                                                    Transform childTrans = CommanderContentDialog.EventModelsRectTrans.GetChild(dataIndex + 1);
                                                    childTrans.SetSiblingIndex(dataIndex);
                                                },
                                                (dialog) =>
                                                {
                                                    IEventModel eventModel = m_CommanderPipeline.EventModels[dataIndex];
                                                    m_CommanderPipeline.EventModels[dataIndex] = m_CommanderPipeline.EventModels[dataIndex + 1];
                                                    m_CommanderPipeline.EventModels[dataIndex + 1] = eventModel;
                                                    Transform childTrans = CommanderContentDialog.EventModelsRectTrans.GetChild(dataIndex + 1);
                                                    childTrans.SetSiblingIndex(dataIndex);
                                                });
            CommanderContentDialog.Redo();
        }

        private void OnStartTime_ValueChanged(string input)
        {
            int dataIndex = transform.GetSiblingIndex();
            LogService.System(nameof(OnStartTime_ValueChanged), $"dataIndex: {dataIndex}, guid: {m_Guid}");
            int.TryParse(input, out int newValue);
            IEventModel eventModel = m_CommanderPipeline.EventModels.First(m => m.Guid == m_Guid);
            float oldValue = eventModel.StartTime;
            CommanderContentDialog.AppendRecord(nameof(OnStartTime_ValueChanged),
                                                (dialog) =>
                                                {
                                                    IEventModel eventModel2 = m_CommanderPipeline.EventModels.First(m => m.Guid == m_Guid);
                                                    eventModel2.StartTime = newValue;
                                                    EventModelEditView view = CommanderContentDialog.EventModelsRectTrans.GetChild(dataIndex).GetComponent<EventModelEditView>();
                                                    view.m_StartTimeInput.text = newValue.ToString();
                                                },
                                                (dialog) =>
                                                {
                                                    IEventModel eventModel2 = m_CommanderPipeline.EventModels.First(m => m.Guid == m_Guid);
                                                    eventModel2.StartTime = oldValue;
                                                    EventModelEditView view = CommanderContentDialog.EventModelsRectTrans.GetChild(dataIndex).GetComponent<EventModelEditView>();
                                                    view.m_StartTimeInput.text = oldValue.ToString();
                                                });
            CommanderContentDialog.Redo();
        }
        private void OnTriggerTime_ValueChanged(string input)
        {
            int dataIndex = transform.GetSiblingIndex();
            LogService.System(nameof(OnTriggerTime_ValueChanged), $"dataIndex: {dataIndex}, guid: {m_Guid}");
            int.TryParse(input, out int newValue);
            IEventModel eventModel = m_CommanderPipeline.EventModels.First(m => m.Guid == m_Guid);
            float oldValue = eventModel.TriggerTime;
            CommanderContentDialog.AppendRecord(nameof(OnTriggerTime_ValueChanged),
                                                (dialog) =>
                                                {
                                                    IEventModel eventModel2 = m_CommanderPipeline.EventModels.First(m => m.Guid == m_Guid);
                                                    eventModel2.TriggerTime = newValue;
                                                    EventModelEditView view = CommanderContentDialog.EventModelsRectTrans.GetChild(dataIndex).GetComponent<EventModelEditView>();
                                                    view.m_TriggerTimeInput.text = newValue.ToString();
                                                },
                                                (dialog) =>
                                                {
                                                    IEventModel eventModel2 = m_CommanderPipeline.EventModels.First(m => m.Guid == m_Guid);
                                                    eventModel2.TriggerTime = oldValue;
                                                    EventModelEditView view = CommanderContentDialog.EventModelsRectTrans.GetChild(dataIndex).GetComponent<EventModelEditView>();
                                                    view.m_TriggerTimeInput.text = oldValue.ToString();
                                                });
            CommanderContentDialog.Redo();
        }
        private void OnEndTime_OnValueChanged(string input)
        {
            int dataIndex = transform.GetSiblingIndex();
            LogService.System(nameof(OnEndTime_OnValueChanged), $"dataIndex: {dataIndex}, guid: {m_Guid}");
            int.TryParse(input, out int newValue);
            IEventModel eventModel = m_CommanderPipeline.EventModels.First(m => m.Guid == m_Guid);
            float oldValue = eventModel.EndTime;
            CommanderContentDialog.AppendRecord(nameof(OnEndTime_OnValueChanged),
                                                (dialog) =>
                                                {
                                                    IEventModel eventModel2 = m_CommanderPipeline.EventModels.First(m => m.Guid == m_Guid);
                                                    eventModel2.EndTime = newValue;
                                                    EventModelEditView view = CommanderContentDialog.EventModelsRectTrans.GetChild(dataIndex).GetComponent<EventModelEditView>();
                                                    view.m_EndTimeInput.text = newValue.ToString();
                                                },
                                                (dialog) =>
                                                {
                                                    IEventModel eventModel2 = m_CommanderPipeline.EventModels.First(m => m.Guid == m_Guid);
                                                    eventModel2.EndTime = oldValue;
                                                    EventModelEditView view = CommanderContentDialog.EventModelsRectTrans.GetChild(dataIndex).GetComponent<EventModelEditView>();
                                                    view.m_EndTimeInput.text = oldValue.ToString();
                                                });
            CommanderContentDialog.Redo();
        }

        private void OnClickUnitButton(int index)
        {
            UnitListDialog unitListDialog = CameraCanvas.PushDialog(GameDefined.UnitListDialog) as UnitListDialog;
            int dataIndex = transform.GetSiblingIndex();
            int[] newUnitID = new int[1];
            Tweener tweener = LogicTween.WaitUntil(() => unitListDialog.DestroyFlag);
            tweener.OnComplete(() =>
                    {
                        if (unitListDialog.DialogResult == DialogResult.OK &&
                            this &&
                            CommanderContentDialog)
                            newUnitID[0] = unitListDialog.UnitID;
                        else
                            tweener.FromHeadToEndIfNeedStop(out _);
                    });
            tweener = tweener.Then(LogicTween.AppendCallback(() =>
                                {
                                    IEventModel eventModel = m_CommanderPipeline.EventModels.First(m => m.Guid == m_Guid);
                                    int[] oldUnitID = new int[1];
                                    if (eventModel is PlayerOperatorEventModel playerOperatorEventModel)
                                    {
                                        int[] unitList = playerOperatorEventModel.UnitIDList;
                                        if (index < unitList.Length)
                                            oldUnitID[0] = unitList[index];
                                    }
                                    CommanderContentDialog.AppendRecord(nameof(OnClickUnitButton),
                                                                        (dialog) =>
                                                                        {
                                                                            OnClickUnitButton_Redo(dialog, dataIndex, index, newUnitID);
                                                                        },
                                                                        (dialog) =>
                                                                        {
                                                                            OnClickUnitButton_Undo(dialog, dataIndex, index, oldUnitID);
                                                                        });
                                    CommanderContentDialog.Redo();
                                }));
            tweener.DoIt();
        }
        private static void OnClickUnitButton_Redo(CommanderContentDialog dialog, int dataIndex, int index, int[] newUnitID)
        {
            IEventModel eventModel = dialog.CommanderPipeline.EventModels[dataIndex];
            if (eventModel is PlayerOperatorEventModel playerOperatorEventModel)
            {
                if (playerOperatorEventModel.UnitIDList.Length <= index)
                {
                    int[] newList = new int[index + 1];
                    Array.Copy(playerOperatorEventModel.UnitIDList, newList, playerOperatorEventModel.UnitIDList.Length);
                    playerOperatorEventModel.UnitIDList = newList;
                }
                playerOperatorEventModel.UnitIDList[index] = newUnitID[0];
            }
            UnitTable.Entry unitEntry = TableManager.UnitTable[newUnitID[0]];
            EventModelEditView view = dialog.EventModelsRectTrans.GetChild(dataIndex).GetComponent<EventModelEditView>();
            (view.m_UnitButtonList[index].targetGraphic as Image).sprite = unitEntry.LoadTexture();
        }
        private static void OnClickUnitButton_Undo(CommanderContentDialog dialog, int dataIndex, int index, int[] oldUnitID)
        {
            IEventModel eventModel = dialog.CommanderPipeline.EventModels[dataIndex];
            if (eventModel is PlayerOperatorEventModel playerOperatorEventModel)
            {
                if (playerOperatorEventModel.UnitIDList.Length < index)
                {
                    int[] newList = new int[index + 1];
                    Array.Copy(playerOperatorEventModel.UnitIDList, newList, playerOperatorEventModel.UnitIDList.Length);
                    playerOperatorEventModel.UnitIDList = newList;
                }
                playerOperatorEventModel.UnitIDList[index] = oldUnitID[0];
            }
            Sprite unitSprite;
            if (oldUnitID[0] != 0 &&
                TableManager.UnitTable[oldUnitID[0]] is UnitTable.Entry unitEntry)
                unitSprite = unitEntry.LoadTexture();
            else
                unitSprite = Resources.Load<Sprite>("Textures/plus");
            EventModelEditView view = dialog.EventModelsRectTrans.GetChild(dataIndex).GetComponent<EventModelEditView>();
            (view.m_UnitButtonList[index].targetGraphic as Image).sprite = unitSprite;
        }
    }
}