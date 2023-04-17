﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class MainMenuDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        public string PrefabPath { get; set; }

        [SerializeField]
        private Button m_SettingButton;
        [SerializeField]
        private Button m_GameStartButton;
        [SerializeField]
        private Button m_CommanderEditorButton;
        [SerializeField]
        private Button m_UpdateResourceButton;
        [SerializeField]
        private Button m_ExitButton;
        [SerializeField]
        private Button m_TestButton;

        private void Awake()
        {
            Application.targetFrameRate = 30;
            Camera.main.GetComponent<TransparentWindow>().SetWindowState(WindowState.Normal);

            m_SettingButton.onClick.AddListener(OnClickSettingButton);
            m_GameStartButton.onClick.AddListener(OnClickGameStartButton);
            m_CommanderEditorButton.onClick.AddListener(OnClickCommanderEditorButton);
            m_UpdateResourceButton.onClick.AddListener(OnClickUpdateResourceButton);
            m_ExitButton.onClick.AddListener(OnClickExitButton);

            m_TestButton.onClick.AddListener(OnClickTestButton);
        }

        private void Start()
        {
            if (Global.UserSetting.NewUser)
                OnClickSettingButton();
        }

        public void Hide()
        {
            m_Canvas.enabled = false;
        }

        public void Show()
        {
            m_Canvas.enabled = true;
        }

        public void OnClickSettingButton()
        {
            IDialog dialog = CameraCanvas.PushDialog(GameDefined.SettingDialogPath);
            HideUntilDialogClose(dialog, () =>
            {
                Camera.main.GetComponent<TransparentWindow>().SetWindowState(WindowState.Normal);
            });
        }
        public void OnClickGameStartButton()
        {
            throw new NotImplementedException();
            CameraCanvas.PopDialog(this);
        }
        public void OnClickCommanderEditorButton()
        {
            IDialog dialog = CameraCanvas.PushDialog(GameDefined.OpenCommanderFileDialog);
            HideUntilDialogClose(dialog);
        }
        public void OnClickUpdateResourceButton()
        {

        }
        public void OnClickExitButton()
        {
            Application.Quit();
        }
        public void OnClickTestButton()
        {
            CameraCanvas.PushDialog(GameDefined.TestDialog);
            CameraCanvas.PopDialog(this);
        }

        IEnumerator HideUntilDialogClose(IDialog dialog, Action callback = null)
        {
            Hide();
            while (dialog.gameObject)
            {
                yield return null;
            }
            Show();
            callback?.Invoke();
        }
    }
}