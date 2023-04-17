using UnityEngine;
using UnityEngine.UI;

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

        private void Awake()
        {
            m_TabButton.onClick.AddListener(OnClickTabButton);
        }

        private void OnClickTabButton()
        {
            SettingDialog.SelectPage(this);
        }
    }
}