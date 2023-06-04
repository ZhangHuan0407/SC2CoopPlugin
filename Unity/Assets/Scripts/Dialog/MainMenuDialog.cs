using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Tween;

namespace Game.UI
{
    public class MainMenuDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        public bool DestroyFlag { get; set; }
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
        private Text m_VersionLabel;

        [SerializeField]
        private Button m_TestButton;

        private void Awake()
        {
            Application.targetFrameRate = 60;

            m_SettingButton.onClick.AddListener(OnClickSettingButton);
            m_GameStartButton.onClick.AddListener(OnClickGameStartButton);
            m_CommanderEditorButton.onClick.AddListener(OnClickCommanderEditorButton);
            m_UpdateResourceButton.onClick.AddListener(OnClickUpdateResourceButton);
            m_ExitButton.onClick.AddListener(OnClickExitButton);
#if ALPHA
            m_VersionLabel.text = Application.version + " alpha";
#else
            m_VersionLabel.text = Application.version;
#endif
            m_TestButton.gameObject.SetActive(Global.UserSetting.IsProgrammer);
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
            LogService.System(nameof(OnClickSettingButton), string.Empty);
            IDialog dialog = CameraCanvas.PushDialog(GameDefined.SettingDialogPath);
            Hide();
            LogicTween.WaitUntil(() => dialog.DestroyFlag)
                .OnComplete(() =>
                {
                    Show();
                })
                .DoIt();
        }
        public void OnClickGameStartButton()
        {
            LogService.System(nameof(OnClickGameStartButton), string.Empty);
            CameraCanvas.PopDialog(this);
            IDialog dialog = CameraCanvas.PushDialog(GameDefined.CoopTimelineDialogPath);
        }
        public void OnClickCommanderEditorButton()
        {
            LogService.System(nameof(OnClickCommanderEditorButton), string.Empty);
            IDialog dialog = CameraCanvas.PushDialog(GameDefined.CommanderEditorDialogPath);
            CameraCanvas.PopDialog(this);
        }
        public void OnClickUpdateResourceButton()
        {
            IDialog dialog = CameraCanvas.PushDialog(GameDefined.UpdateResourceDialog);
            Hide();
            LogicTween.WaitUntil(() => dialog.DestroyFlag)
                .OnComplete(() =>
                {
                    Show();
                })
                .DoIt();
        }
        public void OnClickExitButton()
        {
            LogService.System(nameof(OnClickExitButton), string.Empty);
            Application.Quit();
        }
        public void OnClickTestButton()
        {
            LogService.System(nameof(OnClickTestButton), string.Empty);
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

        private void OnApplicationFocus(bool focus)
        {
#if UNITY_EDITOR
            return;
#endif
            WindowState windowState;
            if (focus)
                windowState = WindowState.TopMostAndBlockRaycast;
            else
                windowState = WindowState.HideAllAndRaycastIgnore;
            Camera.main.GetComponent<TransparentWindow>().SetWindowState(windowState);
        }
    }
}