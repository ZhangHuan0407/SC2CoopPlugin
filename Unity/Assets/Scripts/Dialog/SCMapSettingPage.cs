using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using RectAnchor = Game.OCR.RectAnchor;

namespace Game.UI
{
    public class SCMapSettingPage : SettingPage
    {
        [SerializeField]
        private Toggle m_MapTimeRectEdit;
        [SerializeField]
        private Text m_MapTimeRectText;
        [SerializeField]
        private InputField m_MapTimeRectInput;

        [SerializeField]
        private Toggle m_MapTaskRectEdit;
        [SerializeField]
        private Text m_MapTaskRectText;
        [SerializeField]
        private InputField m_MapTaskRectInput;

        [SerializeField]
        private Toggle m_PluginDialogRectEdit;
        [SerializeField]
        private Text m_PluginDialogRectText;
        [SerializeField]
        private InputField m_PluginDialogRectInput;

        private void Start()
        {
            RectAnchor rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.MapTime];
            m_MapTimeRectText.text = rectAnchor.ToString();
            m_MapTimeRectInput.SetTextWithoutNotify(rectAnchor.ToString());
            m_MapTimeRectInput.onValueChanged.AddListener((string input) => OnRectInputFieldChanged(input, m_MapTimeRectInput));
            m_MapTimeRectInput.gameObject.SetActive(false);
            m_MapTimeRectEdit.SetIsOnWithoutNotify(false);
            m_MapTimeRectEdit.onValueChanged.AddListener(OnClickMapTimeRectEdit);

            rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.MapTask];
            m_MapTaskRectText.text = rectAnchor.ToString();
            m_MapTaskRectInput.SetTextWithoutNotify(rectAnchor.ToString());
            m_MapTaskRectInput.onValueChanged.AddListener((string input) => OnRectInputFieldChanged(input, m_MapTaskRectInput));
            m_MapTaskRectInput.gameObject.SetActive(false);
            m_MapTaskRectEdit.SetIsOnWithoutNotify(false);
            m_MapTaskRectEdit.onValueChanged.AddListener(OnClickMapTaskRectEdit);

            rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.PluginDialog];
            m_PluginDialogRectText.text = rectAnchor.ToString();
            m_PluginDialogRectInput.SetTextWithoutNotify(rectAnchor.ToString());
            m_PluginDialogRectInput.onValueChanged.AddListener((string input) => OnRectInputFieldChanged(input, m_PluginDialogRectInput));
            m_PluginDialogRectInput.gameObject.SetActive(false);
            m_PluginDialogRectEdit.SetIsOnWithoutNotify(false);
            m_PluginDialogRectEdit.onValueChanged.AddListener(OnClickPluginDialogRectEdit);
        }

        private void OnClickMapTimeRectEdit(bool enable)
        {
            if (enable)
            {
                m_MapTimeRectText.gameObject.SetActive(false);
                m_MapTimeRectInput.gameObject.SetActive(true);
                RectAnchor rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.CoopMenu];
                m_MapTimeRectInput.SetTextWithoutNotify(rectAnchor.ToString());
                m_MapTimeRectInput.textComponent.color = Color.black;
            }
            else
            {
                string input = m_MapTimeRectInput.text;
                if (RectAnchor.TryParse(input, out RectAnchor rectAnchor))
                    SettingDialog.UserSetting.RectPositions[RectAnchorKey.CoopMenu] = rectAnchor;
                else
                    rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.CoopMenu];
                m_MapTimeRectText.gameObject.SetActive(true);
                m_MapTimeRectText.text = rectAnchor.ToString();
                m_MapTimeRectInput.gameObject.SetActive(false);
            }
        }
        private void OnClickMapTaskRectEdit(bool enable)
        {
            if (enable)
            {
                m_MapTaskRectText.gameObject.SetActive(false);
                m_MapTaskRectInput.gameObject.SetActive(true);
                RectAnchor rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.MapTask];
                m_MapTaskRectInput.SetTextWithoutNotify(rectAnchor.ToString());
                m_MapTaskRectInput.textComponent.color = Color.black;
            }
            else
            {
                string input = m_MapTaskRectInput.text;
                if (RectAnchor.TryParse(input, out RectAnchor rectAnchor))
                    SettingDialog.UserSetting.RectPositions[RectAnchorKey.MapTask] = rectAnchor;
                else
                    rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.MapTask];
                m_MapTaskRectText.gameObject.SetActive(true);
                m_MapTaskRectText.text = rectAnchor.ToString();
                m_MapTaskRectInput.gameObject.SetActive(false);
            }
        }
        private void OnClickPluginDialogRectEdit(bool enable)
        {
            if (enable)
            {
                m_PluginDialogRectText.gameObject.SetActive(false);
                m_PluginDialogRectInput.gameObject.SetActive(true);
                RectAnchor rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.PluginDialog];
                m_PluginDialogRectInput.SetTextWithoutNotify(rectAnchor.ToString());
                m_PluginDialogRectInput.textComponent.color = Color.black;
            }
            else
            {
                string input = m_PluginDialogRectInput.text;
                if (RectAnchor.TryParse(input, out RectAnchor rectAnchor))
                    SettingDialog.UserSetting.RectPositions[RectAnchorKey.PluginDialog] = rectAnchor;
                else
                    rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.PluginDialog];
                m_PluginDialogRectText.gameObject.SetActive(true);
                m_PluginDialogRectText.text = rectAnchor.ToString();
                m_PluginDialogRectInput.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            InputField input = null;
            if (m_MapTimeRectEdit.isOn)
                input = m_MapTimeRectInput;
            else if (m_MapTaskRectEdit.isOn)
                input = m_MapTaskRectInput;
            else if (m_PluginDialogRectEdit.isOn)
                input = m_PluginDialogRectInput;
            if (input)
            {
                if (RectAnchor.TryParse(input.text, out RectAnchor rectAnchor))
                    SettingDialog.DrawGizmos.DrawRectAnchor(rectAnchor);
                SettingDialog.DrawGizmos.Show();
            }
            else
            {
                SettingDialog.DrawGizmos.Clear();
                SettingDialog.DrawGizmos.Hide();
            }
        }
    }
}