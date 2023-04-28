using System;
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
        private InputField m_FileNameInput;
        [SerializeField]
        private Button m_CancelButton;
        [SerializeField]
        private Button m_OKButton;

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
        }

        private static bool IsValid(string fileName) => Regex.IsMatch(fileName, "\\A[\\w\\s_-\\.]+\\Z");

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
            DialogResult = DialogResult.Cancel;
            CameraCanvas.PopDialog(this);
        }
        private void OnClickOKButton()
        {
            DialogResult = DialogResult.OK;
            CameraCanvas.PopDialog(this);
        }
    }
}