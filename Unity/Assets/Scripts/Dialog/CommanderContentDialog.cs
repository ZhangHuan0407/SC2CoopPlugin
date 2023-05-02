using System;
using System.IO;
using Game.Model;
using Tween;
using UnityEditor;
using UnityEngine;

namespace Game.UI
{
    public class CommanderContentDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        public bool DestroyFlag { get; set; }
        public string PrefabPath { get; set; }

        public CommanderEditorDialog CommanderEditorDialog { get; set; }
        //public bool Focus { get; private set; }

        public CommanderPipeline CommanderPipeline { get; private set; }
        public string FilePath { get; set; }

        [SerializeField]
        private CommanderInfoEditView m_InfoEditView;
        public CommanderInfoEditView InfoEditView => m_InfoEditView;
        [SerializeField]
        private LocalizationEditView m_LocalizationEditView;
        //public LocalizationEditView LocalizationEditView => m_LocalizationEditView;

        [SerializeField]
        private EventModelEditView m_PlayerOperatorTemplateView;
        public EventModelEditView PlayerOperatorTemplateView => m_PlayerOperatorTemplateView;
        [SerializeField]
        private RectTransform m_EventModelsRectTrans;
        public Transform EventModelsRectTrans => m_EventModelsRectTrans;

        [SerializeField]
        private RectTransform m_ScrollContent;

        private OpRecord<CommanderContentDialog> m_OpRecord;
        public bool UndoUseable => m_OpRecord.UndoUseable;
        public bool RedoUseable => m_OpRecord.RedoUseable;

        private bool m_NeedSave;

        public void Hide()
        {
            m_Canvas.enabled = false;
            enabled = false;
        }

        public void Show()
        {
            m_Canvas.enabled = true;
            enabled = true;
        }

        private void Awake()
        {
            m_InfoEditView.CommanderContentDialog = this;
            m_LocalizationEditView.CommanderContentDialog = this;
            m_PlayerOperatorTemplateView.gameObject.SetActive(false);

            m_OpRecord = new OpRecord<CommanderContentDialog>();
        }
        private void OnDestroy()
        {
            if (!CommanderEditorDialog.DestroyFlag)
                CommanderEditorDialog.CommanderContentDialogs.Remove(this);
        }

        public void SetCommanderPipeline(CommanderPipeline pipeline)
        {
            CommanderPipeline = pipeline;
            m_InfoEditView.SetCommanderPipeline(pipeline);
            m_LocalizationEditView.SetCommanderPipeline(pipeline);
            for (int i = 0; i < pipeline.EventModels.Count; i++)
            {
                IEventModel eventModel = pipeline.EventModels[i];
                EventModelEditView view = UnityEngine.Object.Instantiate(PlayerOperatorTemplateView, EventModelsRectTrans);
                view.gameObject.SetActive(true);
                view.CommanderContentDialog = this;
                view.SetCommanderModel(CommanderPipeline, eventModel);
                view.transform.SetSiblingIndex(i);
            }
            m_NeedSave = true;
        }

        private void Update()
        {
            if (CameraCanvas.GetTopMost() == this as IDialog)
            {
                HotkeyTrigger();
            }
            float height = 0f;
            if (m_InfoEditView.gameObject.activeSelf)
            {
                Vector3 localPosition = m_InfoEditView.transform.localPosition;
                localPosition.y = -height;
                m_InfoEditView.transform.localPosition = localPosition;
                height += (m_InfoEditView.transform as RectTransform).rect.height;
            }
            if (m_LocalizationEditView.gameObject.activeSelf)
            {
                Vector3 localPosition = m_LocalizationEditView.transform.localPosition;
                localPosition.y = -height;
                m_LocalizationEditView.transform.localPosition = localPosition;
                height += (m_LocalizationEditView.transform as RectTransform).rect.height;
            }
            if (m_EventModelsRectTrans.gameObject.activeSelf)
            {
                Vector3 localPosition = m_EventModelsRectTrans.transform.localPosition;
                localPosition.y = -height;
                m_EventModelsRectTrans.transform.localPosition = localPosition;
                height += (m_EventModelsRectTrans.transform as RectTransform).rect.height;
            }
            m_ScrollContent.sizeDelta = new Vector2(m_ScrollContent.sizeDelta.x, height);
        }
        private void HotkeyTrigger()
        {
            bool control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (control && Input.GetKeyDown(KeyCode.S))
            {
                PlayerWannaSave();
            }
            else if(control && shift && Input.GetKeyDown(KeyCode.Z))
            {
                Redo();
            }
            else if (control && Input.GetKeyDown(KeyCode.Z))
            {
                Undo();
            }
        }

        public void PlayerWannaClose()
        {
            Tweener tweener = LogicTween.AppendCallback(() =>
                                        {
                                            LogService.System(nameof(CommanderContentDialog), nameof(PlayerWannaClose));
                                        });
            // double check if need save
            tweener = tweener.Then(LogicTween.AppendCallback(() =>
                                  {
                                      CameraCanvas.PopDialog(this);
                                  }));
            tweener.DoIt();
        }

        public void PlayerWannaSave()
        {
            m_NeedSave = false;
            Tweener tweener = LogicTween.AppendCallback(() =>
                                        {
                                            LogService.System(nameof(CommanderContentDialog), nameof(PlayerWannaSave));
                                        });
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                SaveCommanderFileDialog dialog = CameraCanvas.PushDialog(GameDefined.SaveCommanderFileDialog) as SaveCommanderFileDialog;
                tweener = tweener.Then(LogicTween.WaitUntil(() => dialog.DestroyFlag))
                                .OnComplete(() =>
                                {
                                    if (dialog.DialogResult == DialogResult.OK)
                                        FilePath = $"{GameDefined.CustomCommanderPipelineDirectoryPath}/{dialog.FileName}";
                                    else
                                        tweener.FromHeadToEndIfNeedStop(out _);
                                });
            }
            tweener = tweener.Then(LogicTween.AppendCallback(() =>
                                            {
                                                Debug.Log(FilePath);
                                                JSONObject @object = JSONMap.ToJSON(CommanderPipeline);
                                                JSONObject @eventModels = @object["m_EventModels"];
                                                for (int i = 0; i < @eventModels.list.Count; i++)
                                                    @eventModels.list[i].Bake(true);
                                                File.WriteAllText(FilePath, @object.ToString(true));
                                            }));
            tweener.DoIt();
        }

        public void Undo()
        {
            m_NeedSave = true;
            m_OpRecord.Undo(this);
        }
        public void Redo()
        {
            m_NeedSave = true;
            m_OpRecord.Redo(this);
        }
        public void AppendRecord(string debug,
                                 Action<CommanderContentDialog> redo,
                                 Action<CommanderContentDialog> undo)
        {
            m_OpRecord.AppendRecord(new OpRecordEntry<CommanderContentDialog>()
            {
                Debug = debug,
                Redo = redo,
                Undo = undo,
            });
        }

        public void AppendModelEvent()
        {
            int dataIndex = EventModelsRectTrans.childCount;
            LogService.System(nameof(AppendModelEvent), $"dataIndex: {dataIndex}");
            IEventModel eventModel = new PlayerOperatorEventModel()
            {
                Guid = Guid.NewGuid(),
            };
            string modelString = JSONMap.ToJSON(typeof(IEventModel), eventModel).ToString();
            AppendRecord(nameof(AppendModelEvent),
                        (dialog) =>
                        {
                            EventModelEditView template = PlayerOperatorTemplateView;
                            EventModelEditView view = UnityEngine.Object.Instantiate(template, EventModelsRectTrans);
                            view.gameObject.SetActive(true);
                            IEventModel eventModel2 = JSONMap.ParseJSON<IEventModel>(JSONObject.Create(modelString));
                            CommanderPipeline.EventModels.Insert(dataIndex + 1, eventModel);
                            view.CommanderContentDialog = this;
                            view.SetCommanderModel(CommanderPipeline, eventModel2);
                            view.transform.SetSiblingIndex(dataIndex);
                        },
                        (dialog) =>
                        {
                            CommanderPipeline.EventModels.RemoveAt(dataIndex);
                            Transform cloneView = EventModelsRectTrans.GetChild(dataIndex);
                            cloneView.SetParent(null);
                            Destroy(cloneView.gameObject);
                        });
            Redo();
        }
    }
}