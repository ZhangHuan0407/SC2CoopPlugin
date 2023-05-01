using System;
using System.Linq;
using Game.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class EventModelEditView : MonoBehaviour
    {
        public CommanderContentDialog CommanderContentDialog { get; set; }
        private CommanderPipeline m_CommanderPipeline;
        private Guid m_Guid;

        [SerializeField]
        private Dropdown m_EventModelType;
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
            m_DeleteButton.onClick.AddListener(EventModel_Delete);
            m_CopyButton.onClick.AddListener(EventModel_Copy);
            m_UpButton.onClick.AddListener(EventModel_Up);
            m_DownButton.onClick.AddListener(EventModel_Down);

            m_TimeTrans.gameObject.SetActive(false);

            m_UnitTrans.gameObject.SetActive(false);
        }

        public void SetCommanderModel(CommanderPipeline pipeline, IEventModel eventModel)
        {
            m_CommanderPipeline = pipeline;
            name = eventModel.Guid.ToString();
            m_Guid = eventModel.Guid;

            m_StartTimeInput.text = eventModel.StartTime.ToString();
            m_StartTimeInput.onValueChanged.AddListener(OnStartTime_ValueChanged);
            m_TriggerTimeInput.text = eventModel.TriggerTime.ToString();
            m_TriggerTimeInput.onValueChanged.AddListener(OnTriggerTime_ValueChanged);
            m_EndTimeInput.text = eventModel.EndTime.ToString();
            m_EndTimeInput.onValueChanged.AddListener(OnEndTime_OnValueChanged);
        }

        public void EventModel_Copy()
        {
            int dataIndex = transform.GetSiblingIndex();
            LogService.System(nameof(EventModel_Copy), $"dataIndex: {dataIndex}, guid: {m_Guid}");
            IEventModel eventModel = m_CommanderPipeline.EventModels.First(m => m.Guid == m_Guid);
            string modelString = JSONMap.ToJSON(eventModel).ToString();
            CommanderContentDialog.AppendRecord(nameof(EventModel_Copy),
                                                (dialog) =>
                                                {
                                                    EventModelEditView template = CommanderContentDialog.PlayerOperatorTemplateView;
                                                    EventModelEditView view = UnityEngine.Object.Instantiate(template, CommanderContentDialog.EventModelsRectTrans);
                                                    IEventModel eventModel2 = JSONMap.ParseJSON<IEventModel>(JSONObject.Create(modelString));
                                                    eventModel2.Guid = Guid.NewGuid();
                                                    view.SetCommanderModel(m_CommanderPipeline, eventModel2);
                                                    view.transform.SetSiblingIndex(dataIndex + 1);
                                                },
                                                (dialog) =>
                                                {
                                                    m_CommanderPipeline.EventModels.RemoveAt(dataIndex + 1);
                                                    Transform cloneView = CommanderContentDialog.EventModelsRectTrans.GetChild(dataIndex);
                                                    cloneView.SetParent(null);
                                                    Destroy(cloneView.gameObject);
                                                });
        }
        public void EventModel_Delete()
        {
            int dataIndex = transform.GetSiblingIndex();
            LogService.System(nameof(EventModel_Delete), $"dataIndex: {dataIndex}, guid: {m_Guid}");
            IEventModel eventModel = m_CommanderPipeline.EventModels.First(m => m.Guid == m_Guid);
            string modelString = JSONMap.ToJSON(eventModel).ToString();
            CommanderContentDialog.AppendRecord(nameof(EventModel_Delete),
                                                (dialog) =>
                                                {
                                                    transform.SetParent(null);
                                                    Destroy(gameObject);
                                                    m_CommanderPipeline.EventModels.RemoveAt(dataIndex);
                                                },
                                                (dialog) =>
                                                {
                                                    var template = CommanderContentDialog.PlayerOperatorTemplateView;
                                                    var view = UnityEngine.Object.Instantiate(template, CommanderContentDialog.EventModelsRectTrans);
                                                    IEventModel eventModel2 = JSONMap.ParseJSON<IEventModel>(JSONObject.Create(modelString));
                                                    view.SetCommanderModel(m_CommanderPipeline, eventModel2);
                                                    view.transform.SetSiblingIndex(dataIndex);
                                                });
        }
        private void EventModel_Up()
        {
            int dataIndex = transform.GetSiblingIndex();
            LogService.System(nameof(EventModel_Up), $"dataIndex: {dataIndex}, guid: {m_Guid}");

        }
        private void EventModel_Down()
        {
            int dataIndex = transform.GetSiblingIndex();
            LogService.System(nameof(EventModel_Down), $"dataIndex: {dataIndex}, guid: {m_Guid}");

        }

        private void OnStartTime_ValueChanged(string input)
        {
            int dataIndex = transform.GetSiblingIndex();
            LogService.System(nameof(OnStartTime_ValueChanged), $"dataIndex: {dataIndex}, guid: {m_Guid}");

        }
        private void OnTriggerTime_ValueChanged(string input)
        {
            int dataIndex = transform.GetSiblingIndex();
            LogService.System(nameof(OnTriggerTime_ValueChanged), $"dataIndex: {dataIndex}, guid: {m_Guid}");

        }
        private void OnEndTime_OnValueChanged(string input)
        {
            int dataIndex = transform.GetSiblingIndex();
            LogService.System(nameof(OnEndTime_OnValueChanged), $"dataIndex: {dataIndex}, guid: {m_Guid}");

        }
    }
}