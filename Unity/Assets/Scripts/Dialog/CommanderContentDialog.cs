using System;
using System.IO;
using Game.Model;
using Tween;
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
            CommanderEditorDialog.CommanderContentDialogs.Remove(this);
        }

        public void SetCommanderPipeline(CommanderPipeline pipeline)
        {
            CommanderPipeline = pipeline;
            m_InfoEditView.SetCommanderPipeline(pipeline);
            m_LocalizationEditView.SetCommanderPipeline(pipeline);
            m_NeedSave = false;
        }

        private void Update()
        {
            if (CameraCanvas.GetTopMost() == this as IDialog)
            {
                HotkeyTrigger();
            }
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
            tweener.Then(LogicTween.AppendCallback(() =>
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
                                    tweener.FromHeadToEndIfNeedStop(out _);
                                });
            }
            tweener.Then(LogicTween.AppendCallback(() =>
                                    {
                                        JSONObject @object = JSONMap.ToJSON(CommanderPipeline);
                                        JSONObject @eventModels = @object[nameof(CommanderPipeline.EventModels)];
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
    }
}