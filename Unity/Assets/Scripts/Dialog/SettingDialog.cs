using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class SettingDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        public bool DestroyFlag { get; set; }
        public string PrefabPath { get; set; }

        [SerializeField]
        private Button m_CloseButton;
        [SerializeField]
        private Button m_SaveButton;
        [SerializeField]
        private SettingPage[] m_SettingPageList;
        [SerializeField]
        private Transform m_TabList;
        [SerializeField]
        private Text m_SaveFinishTips;

        public UserSetting UserSetting { get; private set; }

        public DrawGizmosDialog DrawGizmos { get; private set; }

        private void Awake()
        {
            m_SaveButton.onClick.AddListener(OnClickSaveButton);
            m_CloseButton.onClick.AddListener(OnClickCloseButton);

            UserSetting = JSONMap.JSONDeepClone(Global.UserSetting);
            if (UserSetting.NewUser)
            {
                m_SaveButton.gameObject.SetActive(false);
            }
            DrawGizmos = CameraCanvas.PushDialog(GameDefined.DrawGizmosDialogPath) as DrawGizmosDialog;
            DrawGizmos.Clear();
            for (int i = 0; i < m_SettingPageList.Length; i++)
            {
                SettingPage settingPage = m_SettingPageList[i];
                settingPage.SettingDialog = this;
                settingPage.gameObject.SetActive(false);
                settingPage.gameObject.SetActive(true);
            }
            SelectPage(m_SettingPageList[0]);

            m_SaveFinishTips.gameObject.SetActive(false);
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
            bool anyNeverLookup = false;
            sender.HaveLookup = true;
            for (int i = 0; i < m_SettingPageList.Length; i++)
            {
                SettingPage settingPage = m_SettingPageList[i];
                settingPage.gameObject.SetActive(m_SettingPageList[i] == sender);
                if (!settingPage.HaveLookup)
                    anyNeverLookup = true;
            }
            m_TabList.position = sender.TabListPosition.position;
            if (UserSetting.NewUser && !anyNeverLookup)
            {
                m_SaveButton.gameObject.SetActive(true);
            }
            DrawGizmos.Hide();
        }

        private void OnClickCloseButton()
        {
            CameraCanvas.PopDialog(this);
            CameraCanvas.PopDialog(DrawGizmos);
        }
        private void OnClickSaveButton()
        {
            foreach (SettingPage settingPage in m_SettingPageList)
                settingPage.BeforeSave();
            bool newUser = UserSetting.NewUser;
            Global.UserSetting = UserSetting;
            UserSetting.Save();
            StopCoroutine(nameof(SaveFinishTips));
            StartCoroutine(nameof(SaveFinishTips));
        }
        private IEnumerator SaveFinishTips()
        {
            m_SaveFinishTips.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            m_SaveFinishTips.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
        }
    }
}