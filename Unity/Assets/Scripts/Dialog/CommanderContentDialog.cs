using Game.Model;
using System;
using System.IO;
using Tween;
using UnityEngine;
using UnityEngine.UI;

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
        public bool InShow => enabled;

        public CommanderModel CommanderModel { get; set; }
        public string FilePath { get; set; }

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

        }
        private void OnDestroy()
        {
            CommanderEditorDialog.CommanderContentDialogs.Remove(this);
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
                                        JSONObject @object = JSONMap.ToJSON(CommanderModel);
                                        JSONObject @eventModels = @object[nameof(CommanderModel.EventModels)];
                                        for (int i = 0; i < @eventModels.list.Count; i++)
                                            @eventModels.list[i].Bake(true);
                                        File.WriteAllText(FilePath, @object.ToString(true));
                                    }));
            tweener.DoIt();
        }
        public void Undo()
        {

        }
        public void Redo()
        {

        }
    }
}