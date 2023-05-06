using System.Collections.Generic;
using System.IO;
using Game.Model;
using Table;
using Tween;
using UnityEngine;
using UnityEngine.UI;
using Guid = System.Guid;

namespace Game.UI
{
    public class CommanderEditorDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        public bool DestroyFlag { get; set; }
        public string PrefabPath { get; set; }

        [Header("File Menu")]
        [SerializeField]
        private Button m_FileMenuButton;
        [SerializeField]
        private GameObject m_FileMenuDropdown;
        [SerializeField]
        private Button m_CreateFileButton;
        [SerializeField]
        private Button m_SaveFileButton;
        [SerializeField]
        private Button m_OpenFileButton;
        [SerializeField]
        private Button m_CloseButton;
        [SerializeField]
        private Button m_ExitButton;

        [Header("Edit Menu")]
        [SerializeField]
        private Button m_EditMenuButton;
        [SerializeField]
        private GameObject m_EditMenuDropdown;
        [SerializeField]
        private Button m_UndoButton;
        [SerializeField]
        private Button m_RedoButton;
        [SerializeField]
        private Button m_AppendModelEventButton;

        [Header("Commander Content Menu")]
        [SerializeField]
        private Text m_CommanderContentMenuLabel;
        [SerializeField]
        private Button m_CommanderContentMenuButton;
        [SerializeField]
        private GameObject m_CommanderContentDropdown;
        [SerializeField]
        private Button m_CommanderContentTemplate;
        public List<CommanderContentDialog> CommanderContentDialogs { get; private set; }

        [Header("Help Menu")]
        [SerializeField]
        private Button m_HelpMenuButton;
        [SerializeField]
        private GameObject m_HelpMenuDropdown;
        [SerializeField]
        private Button m_HowToShareButton;
        [SerializeField]
        private Button m_HowToUseButton;

        private int m_MouseIgnoreFrame;

        public void Hide()
        {
            m_Canvas.enabled = false;
        }
        public void Show()
        {
            m_Canvas.enabled = true;
        }

        private void Awake()
        {
            CommanderContentDialogs = new List<CommanderContentDialog>();

            // File Menu
            m_FileMenuButton.onClick.AddListener(OnClickFileMenuButton);
            m_FileMenuDropdown.SetActive(false);
            m_CreateFileButton.onClick.AddListener(OnClickCreateFileButton);
            m_SaveFileButton.onClick.AddListener(OnClickSaveFileMenuButton);
            m_OpenFileButton.onClick.AddListener(OnClickOpenFileButton);
            m_CloseButton.onClick.AddListener(OnClickCloseButton);
            m_ExitButton.onClick.AddListener(OnClickExitButton);

            // Edit Menu
            m_EditMenuButton.onClick.AddListener(OnClickEditMenuButton);
            m_UndoButton.onClick.AddListener(OnClickUndoButton);
            m_RedoButton.onClick.AddListener(OnClickRedoButton);
            m_AppendModelEventButton.onClick.AddListener(OnClickAppendModelEventButton);
            m_EditMenuDropdown.SetActive(false);

            // Commander Content Menu
            m_CommanderContentMenuButton.onClick.AddListener(OnClickCommanderContentMenuButton);
            m_CommanderContentDropdown.SetActive(false);
            m_CommanderContentTemplate.gameObject.SetActive(false);

            // Help Menu
            m_HelpMenuButton.onClick.AddListener(OnClickHelpMenuButton);
            m_HelpMenuDropdown.SetActive(false);
            m_HowToShareButton.onClick.AddListener(OnClickHowToShareButtonButton);
            m_HowToUseButton.onClick.AddListener(OnClickHowToUseButtonButton);

            Application.targetFrameRate = 20;
        }

        private void Update()
        {
            if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2)) &&
                m_MouseIgnoreFrame != Time.frameCount)
            {
                m_FileMenuDropdown.SetActive(false);
                m_EditMenuDropdown.SetActive(false);
                m_CommanderContentDropdown.SetActive(false);
                m_HelpMenuDropdown.SetActive(false);
            }
        }

        private CommanderContentDialog GetFocusCCDialog()
        {
            foreach (CommanderContentDialog commanderContentDialog in CameraCanvas.GetDialogs<CommanderContentDialog>())
            {
                if (!commanderContentDialog.DestroyFlag &&
                    commanderContentDialog.CommanderEditorDialog == this)
                {
                    if (commanderContentDialog.enabled)
                        return commanderContentDialog;
                    else
                        return null;
                }
            }
            return null;
        }

        #region File Menu
        private void OnClickFileMenuButton()
        {
            LogService.System(nameof(OnClickFileMenuButton), string.Empty);
            m_FileMenuDropdown.SetActive(true);
            m_MouseIgnoreFrame = Time.frameCount;
            bool containsAny = GetFocusCCDialog() != null;
            m_SaveFileButton.interactable = containsAny;
            m_CloseButton.interactable = containsAny;
        }

        private void OnClickCreateFileButton()
        {
            LogService.System(nameof(OnClickCreateFileButton), string.Empty);
            CommanderContentDialog commanderContentDialog = CameraCanvas.PushDialog(GameDefined.CommanderContentDialogPath) as CommanderContentDialog;
            commanderContentDialog.CommanderEditorDialog = this;
            CommanderContentDialogs.Add(commanderContentDialog);
            CommanderPipeline commanderPipeline = TableManager.ModelTable.InstantiateModel<CommanderPipeline>("CommanderPipeline_Template");
            commanderPipeline.Language = Global.UserSetting.InterfaceLanguage;
            commanderPipeline.EventModels.Add(new PlayerOperatorEventModel()
            {
                Guid = Guid.NewGuid(),
                StartTime = 1,
                TriggerTime = 1,
                EndTime = 5,
            });
            commanderContentDialog.SetCommanderPipeline(commanderPipeline);
        }

        private void OnClickSaveFileMenuButton()
        {
            LogService.System(nameof(OnClickSaveFileMenuButton), string.Empty);
            CommanderContentDialog dialog = GetFocusCCDialog();
            if (dialog)
            {
                CameraCanvas.SetTopMost(dialog);
                dialog.PlayerWannaSave();
            }
        }

        private void OnClickOpenFileButton()
        {
            LogService.System(nameof(OnClickOpenFileButton), string.Empty);
            OpenCommanderFileDialog dialog = CameraCanvas.PushDialog(GameDefined.OpenCommanderFileDialog) as OpenCommanderFileDialog;
            Tweener tweener = LogicTween.WaitUntil(() => dialog.DestroyFlag);
            tweener.Then(LogicTween.AppendCallback(() =>
                    {
                        if (dialog.DialogResult != DialogResult.OK)
                        {
                            tweener.FromHeadToEndIfNeedStop(out _);
                            return;
                        }
                        string fullName = TableManager.CommanderPipelineTable[dialog.CommanderPipelineId].FullName;
                        if (!File.Exists(fullName))
                        {
                            LogService.Error($"CommanderEditor OpenFile, file is not exists", fullName);
                            tweener.FromHeadToEndIfNeedStop(out _);
                            return;
                        }
                        else
                        {
                            CommanderContentDialog commanderContentDialog = CameraCanvas.PushDialog(GameDefined.CommanderContentDialogPath) as CommanderContentDialog;
                            commanderContentDialog.CommanderEditorDialog = this;
                            CommanderContentDialogs.Add(commanderContentDialog);
                            commanderContentDialog.FilePath = fullName;
                            CommanderPipeline model = TableManager.CommanderPipelineTable.Instantiate(dialog.CommanderPipelineId);
                            commanderContentDialog.SetCommanderPipeline(model);
                        }
                    }))
                      .DoIt();
        }

        private void OnClickCloseButton()
        {
            LogService.System(nameof(OnClickCloseButton), string.Empty);
            CommanderContentDialog dialog = GetFocusCCDialog();
            if (dialog)
            {
                CameraCanvas.SetTopMost(dialog);
                dialog.PlayerWannaClose();

                bool containsAny = GetFocusCCDialog() != null;
                m_SaveFileButton.interactable = containsAny;
                m_CloseButton.interactable = containsAny;
            }
        }

        private void OnClickExitButton()
        {
            LogService.System(nameof(OnClickExitButton), string.Empty);
            CameraCanvas.PopDialog(this);
            for (int i = 0; i < CommanderContentDialogs.Count; i++)
            {
                if (CommanderContentDialogs[i].DestroyFlag)
                    continue;
                CameraCanvas.PopDialog(CommanderContentDialogs[i]);
            }
            IDialog dialog = CameraCanvas.PushDialog(GameDefined.MainManuDialog);
        }
        #endregion

        #region Edit Menu
        private void OnClickEditMenuButton()
        {
            LogService.System(nameof(OnClickEditMenuButton), string.Empty);
            m_EditMenuDropdown.SetActive(true);
            m_MouseIgnoreFrame = Time.frameCount;
            CommanderContentDialog commanderContentDialog = GetFocusCCDialog();
            if (commanderContentDialog)
            {
                m_RedoButton.interactable = commanderContentDialog.RedoUseable;
                m_UndoButton.interactable = commanderContentDialog.UndoUseable;
            }
            else
            {
                m_RedoButton.interactable = false;
                m_UndoButton.interactable = false;
            }
        }
        private void OnClickUndoButton()
        {
            LogService.System(nameof(OnClickUndoButton), string.Empty);
            CommanderContentDialog dialog = GetFocusCCDialog();
            if (dialog)
            {
                CameraCanvas.SetTopMost(dialog);
                dialog.Undo();
            }
        }
        private void OnClickRedoButton()
        {
            LogService.System(nameof(OnClickRedoButton), string.Empty);
            CommanderContentDialog dialog = GetFocusCCDialog();
            if (dialog)
            {
                CameraCanvas.SetTopMost(dialog);
                dialog.Redo();
            }
        }
        private void OnClickAppendModelEventButton()
        {
            LogService.System(nameof(OnClickAppendModelEventButton), string.Empty);
            CommanderContentDialog dialog = GetFocusCCDialog();
            if (dialog)
            {
                CameraCanvas.SetTopMost(dialog);
                dialog.AppendModelEvent();
            }
        }
        #endregion

        #region Commander Content Menu
        private void OnClickCommanderContentMenuButton()
        {
            LogService.System(nameof(OnClickCommanderContentMenuButton), string.Empty);
            m_CommanderContentDropdown.SetActive(true);
            m_MouseIgnoreFrame = Time.frameCount;
            foreach (Transform child in m_CommanderContentDropdown.transform)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
            for (int i = 0; i < CommanderContentDialogs.Count; i++)
            {
                CommanderContentDialog dialog = CommanderContentDialogs[i];
                Button button = Instantiate(m_CommanderContentTemplate, m_CommanderContentDropdown.transform);
                button.gameObject.SetActive(true);
                string fileName;
                if (string.IsNullOrWhiteSpace(dialog.FilePath))
                    fileName = "new file.json";
                else
                    fileName = Path.GetFileName(dialog.FilePath);
                button.GetComponentInChildren<Text>().text = fileName;
                button.onClick.AddListener(() =>
                {
                    dialog.Show();
                    CameraCanvas.SetTopMost(dialog);
                });
            }
        }
        #endregion

        #region Help Menu
        private void OnClickHelpMenuButton()
        {
            LogService.System(nameof(OnClickHelpMenuButton), string.Empty);
            m_HelpMenuDropdown.SetActive(true);
            m_MouseIgnoreFrame = Time.frameCount;
        }
        private void OnClickHowToShareButtonButton()
        {
            LogService.System(nameof(OnClickHowToShareButtonButton), string.Empty);
            Application.OpenURL(GameDefined.ShareCommanderPipelineChineseWiki);
        }
        private void OnClickHowToUseButtonButton()
        {
            LogService.System(nameof(OnClickHowToUseButtonButton), string.Empty);
            Application.OpenURL(GameDefined.CommanderEditorChineseWiki);
        }
        #endregion
    }
}