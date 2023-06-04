using UnityEngine;
using UnityEngine.UI;
using RectAnchor = Game.OCR.RectAnchor;

namespace Game.UI
{
    public abstract class SettingPage : MonoBehaviour
    {
        [SerializeField]
        private Transform m_TabListPosition;
        public Transform TabListPosition => m_TabListPosition;

        public SettingDialog SettingDialog { get; set; }

        [SerializeField]
        private Button m_TabButton;
        public Button TabButton => m_TabButton;

        public bool HaveLookup { get; set; }

        protected virtual void Awake()
        {
            m_TabButton.onClick.AddListener(OnClickTabButton);
        }

        private void OnClickTabButton()
        {
            SettingDialog.SelectPage(this);
        }
        protected void OnRectInputFieldChanged(string input, InputField inputField)
        {
            bool parse = RectAnchor.TryParse(input, out RectAnchor rectAnchor);
            inputField.textComponent.color = parse ? Color.black : Color.red;
        }

        public virtual void BeforeSave()
        {
        }
    }
}