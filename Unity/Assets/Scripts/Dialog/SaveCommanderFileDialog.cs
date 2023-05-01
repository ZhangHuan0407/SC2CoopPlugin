using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class SaveCommanderFileDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        public bool DestroyFlag { get; set; }
        public string PrefabPath { get; set; }

        public DialogResult DialogResult { get; set; }
        public string FilePath { get; set; }

        [SerializeField]
        private Transform m_ContentTrans;
        [SerializeField]
        private Text m_LabelTemplate;
        [SerializeField]
        private InputField m_FileNameInput;
        [SerializeField]
        private Button m_CancelButton;
        [SerializeField]
        private Button m_OKButton;

        public string FileName { get; set; }

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
            m_FileNameInput.onValueChanged.AddListener(OnFileNameInput_ValueChanged);
            m_FileNameInput.onEndEdit.AddListener(OnFileNameInput_EndEdit);
            m_CancelButton.onClick.AddListener(OnClickCancelButton);
            m_OKButton.onClick.AddListener(OnClickOKButton);
            m_OKButton.interactable = false;
            m_LabelTemplate.gameObject.SetActive(false);
        }
        private void Start()
        {
            m_FileNameInput.text = FileName;
            foreach (string filePath in Directory.GetFiles(GameDefined.CustomCommanderPipelineDirectoryPath, "*.json"))
            {
                Text label = Instantiate(m_LabelTemplate, m_ContentTrans);
                label.gameObject.SetActive(true);
                FileInfo fileInfo = new FileInfo(filePath);
                label.text = $"{fileInfo.Name.PadRight(45)}, {fileInfo.LastWriteTime}";
            }
        }

        private static bool IsValid(string fileName) => Regex.IsMatch(fileName, "\\A[\\w\\s_\\-\\.]+\\Z");

        private void OnFileNameInput_ValueChanged(string input)
        {
            m_OKButton.interactable = IsValid(input);
        }
        private void OnFileNameInput_EndEdit(string input)
        {
            if (!IsValid(input))
                return;
            if (!input.EndsWith(".json", StringComparison.InvariantCulture))
                input += ".json";
            m_FileNameInput.SetTextWithoutNotify(input);
        }
        private void OnClickCancelButton()
        {
            LogService.System(nameof(SaveCommanderFileDialog), nameof(OnClickCancelButton));
            DialogResult = DialogResult.Cancel;
            CameraCanvas.PopDialog(this);
        }
        private void OnClickOKButton()
        {
            LogService.System(nameof(SaveCommanderFileDialog), nameof(OnClickOKButton));
            FileName = m_FileNameInput.text;
            DialogResult = DialogResult.OK;
            CameraCanvas.PopDialog(this);
        }
    }
}