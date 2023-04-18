using System;
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

        private void Start()
        {
            RectAnchor rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.MapTime];
            m_MapTimeRectText.text = rectAnchor.ToString();
            m_MapTimeRectInput.SetTextWithoutNotify(rectAnchor.ToString());
            m_MapTimeRectInput.onValueChanged.AddListener((string input) => OnRectInputFieldChanged(input, m_MapTimeRectInput));
            m_MapTimeRectEdit.SetIsOnWithoutNotify(false);
            m_MapTimeRectEdit.onValueChanged.AddListener(OnClickMapTimeRectEdit);

            rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.MapTask];
            m_MapTaskRectText.text = rectAnchor.ToString();
            m_MapTaskRectInput.SetTextWithoutNotify(rectAnchor.ToString());
            m_MapTaskRectInput.onValueChanged.AddListener((string input) => OnRectInputFieldChanged(input, m_MapTaskRectInput));
            m_MapTaskRectEdit.SetIsOnWithoutNotify(false);
            m_MapTaskRectEdit.onValueChanged.AddListener(OnClickMapTaskRectEdit);
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
                    SettingDialog.UserSetting.RectPositions[RectAnchorKey.CoopMenu] = rectAnchor;
                else
                    rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.MapTask];
                m_MapTaskRectText.gameObject.SetActive(true);
                m_MapTaskRectText.text = rectAnchor.ToString();
                m_MapTaskRectInput.gameObject.SetActive(false);
            }
        }
    }
}