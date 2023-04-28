using System.Collections.Generic;
using System.IO;
using Game.Model;
using Table;
using Tween;
using UnityEngine;
using UnityEngine.UI;

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
        private void OnDestroy()
        {
            for (int i = 0; i < CommanderContentDialogs.Count; i++)
            {
                if (CommanderContentDialogs[i].DestroyFlag)
                    continue;
                CameraCanvas.PopDialog(CommanderContentDialogs[i]);
            }
        }

        private void Update()
        {
            if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2)) &&
                m_MouseIgnoreFrame != Time.frameCount)
            {
                m_FileMenuDropdown.SetActive(false);
                m_EditMenuDropdown.SetActive(false);
                m_CommanderContentDropdown.SetActive(false);
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
            m_FileMenuDropdown.SetActive(true);
            m_MouseIgnoreFrame = Time.frameCount;
            bool containsAny = GetFocusCCDialog() != null;
            m_SaveFileButton.interactable = containsAny;
            m_CloseButton.interactable = containsAny;
        }

        private void OnClickCreateFileButton()
        {
            m_MouseIgnoreFrame = Time.frameCount;
            CommanderContentDialog commanderContentDialog = CameraCanvas.PushDialog(GameDefined.CommanderContentDialogPath) as CommanderContentDialog;
            CommanderContentDialogs.Add(commanderContentDialog);
            commanderContentDialog.SetCommanderPipeline(CommanderPipeline.CreateDebug());
        }

        private void OnClickSaveFileMenuButton()
        {
            CommanderContentDialog dialog = GetFocusCCDialog();
            if (dialog)
            {
                CameraCanvas.SetTopMost(dialog);
                dialog.PlayerWannaSave();
            }
        }

        private void OnClickOpenFileButton()
        {
            OpenCommanderFileDialog dialog = CameraCanvas.PushDialog(GameDefined.OpenCommanderFileDialog) as OpenCommanderFileDialog;
            Tweener tweener = LogicTween.WaitUntil(() => dialog.DestroyFlag);
            tweener.Then(LogicTween.AppendCallback(() =>
                    {
                        if (dialog.DialogResult != DialogResult.OK ||
                            !File.Exists(dialog.CommanderPipelineId))
                            tweener.FromHeadToEndIfNeedStop(out _);
                        else
                        {
                            FileInfo fileInfo = TableManager.CommanderPipelineTable[dialog.CommanderPipelineId];
                            JSONObject @object = JSONObject.Create(File.ReadAllText(fileInfo.FullName));
                            CommanderPipeline model = JSONMap.ParseJSON<CommanderPipeline>(@object);

                            CommanderContentDialog commanderContentDialog = CameraCanvas.PushDialog(GameDefined.CommanderContentDialogPath) as CommanderContentDialog;
                            CommanderContentDialogs.Add(commanderContentDialog);
                            commanderContentDialog.FilePath = fileInfo.FullName;
                            commanderContentDialog.SetCommanderPipeline(model);
                        }
                    }))
                      .DoIt();
        }

        private void OnClickCloseButton()
        {
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
            CameraCanvas.PopDialog(this);
            IDialog dialog = CameraCanvas.PushDialog(GameDefined.MainManuDialog);
        }
        #endregion

        #region Edit Menu
        private void OnClickEditMenuButton()
        {
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
            CommanderContentDialog dialog = GetFocusCCDialog();
            if (dialog)
            {
                CameraCanvas.SetTopMost(dialog);
                dialog.Undo();
            }
        }
        private void OnClickRedoButton()
        {
            CommanderContentDialog dialog = GetFocusCCDialog();
            if (dialog)
            {
                CameraCanvas.SetTopMost(dialog);
                dialog.Redo();
            }
        }
        #endregion

        #region Commander Content Menu
        private void OnClickCommanderContentMenuButton()
        {
            m_CommanderContentDropdown.SetActive(true);
            m_MouseIgnoreFrame = Time.frameCount;
            foreach (Transform child in m_CommanderContentDropdown.transform)
            {
                UnityEngine.Object.Destroy(child);
            }
            for (int i = 0; i < CommanderContentDialogs.Count; i++)
            {
                CommanderContentDialog dialog = CommanderContentDialogs[i];
                Button button = Instantiate(m_CommanderContentTemplate);
                string fileName;
                if (string.IsNullOrWhiteSpace(dialog.FilePath))
                    fileName = "new.json";
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
            m_HelpMenuDropdown.SetActive(true);
            m_MouseIgnoreFrame = Time.frameCount;
        }
        private void OnClickHowToShareButtonButton()
        {
            Application.OpenURL(GameDefined.HowToShareWebPage);
        }
        private void OnClickHowToUseButtonButton()
        {
            Application.OpenURL(GameDefined.HowToUseWebPage);
        }
        #endregion
    }
}