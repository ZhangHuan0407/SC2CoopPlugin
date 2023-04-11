using Game.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class SettingDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        public string PrefabPath { get; set; }

        [SerializeField]
        private Button m_CloseButton;
        [SerializeField]
        private Button m_SaveButton;
        [SerializeField]
        private SettingPage[] m_SettingPageList;
        [SerializeField]
        private Transform m_TabList;

        private void Awake()
        {
            if (Global.UserSetting.NewUser)
            {
                m_CloseButton.gameObject.SetActive(false);
                m_SaveButton.gameObject.SetActive(false);
            }
            for (int i = 0; i < m_SettingPageList.Length; i++)
            {
                m_SettingPageList[i].gameObject.SetActive(true);
            }
            SelectPage(m_SettingPageList[0]);
        }

        public void Hide()
        {
            m_Canvas.enabled = false;
        }

        public void Show()
        {
            m_Canvas.enabled = true;
        }

        public void SelectPage(SettingPage sender)
        {
            for (int i = 0; i < m_SettingPageList.Length; i++)
            {
                m_SettingPageList[i].gameObject.SetActive(m_SettingPageList[i] == sender);
            }
            m_TabList.position = sender.TabListPosition.position;
        }

        private void OnClickCloseButton()
        {
            CameraCanvas.PopDialog(this);
        }
        private void OnClickSaveButton()
        {
            UserSetting.Save();
            m_CloseButton.gameObject.SetActive(true);
        }
    }
}