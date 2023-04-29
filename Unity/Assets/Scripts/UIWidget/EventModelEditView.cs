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

        private void Awake()
        {
            
        }

        public void SetCommanderModel(CommanderPipeline pipeline, IEventModel eventModel)
        {
            m_CommanderPipeline = pipeline;
            m_Guid = eventModel.Guid;
        }

        public void EventModel_Copy()
        {

        }
        public void EventModel_Delete()
        {
            int dataIndex = transform.GetSiblingIndex();
            var eventModel = m_CommanderPipeline.EventModels.First(m => m.Guid == m_Guid);
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
                                                    PlayerOperatorEventModel eventModel2 = JSONMap.ParseJSON<PlayerOperatorEventModel>(JSONObject.Create(modelString));
                                                    view.SetCommanderModel(m_CommanderPipeline, eventModel2);
                                                    view.transform.SetSiblingIndex(dataIndex);
                                                });
        }
    }
}