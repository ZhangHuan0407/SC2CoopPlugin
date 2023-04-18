using System;
using UnityEngine;
using UnityEngine.UI;
using RectAnchor = Game.OCR.RectAnchor;

namespace Game.UI
{
    public class SCMenuSettingPage : SettingPage
    {
        [SerializeField]
        private Toggle m_CoopMenuRectEdit;
        [SerializeField]
        private Text m_CoopMenuRectText;
        [SerializeField]
        private InputField m_CoopMenuRectInput;

        [SerializeField]
        private Toggle m_CommanderNameRectEdit;
        [SerializeField]
        private Text m_CommanderNameRectText;
        [SerializeField]
        private InputField m_CommanderNameRectInput;

        [SerializeField]
        private Toggle m_MasteriesRectEdit;
        [SerializeField]
        private Text m_MasteriesRectText;
        [SerializeField]
        private InputField m_MasteriesRectInput;

        private void Start()
        {
            RectAnchor rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.CoopMenu];
            m_CoopMenuRectText.text = rectAnchor.ToString();
            m_CoopMenuRectInput.SetTextWithoutNotify(rectAnchor.ToString());
            m_CoopMenuRectInput.onValueChanged.AddListener((string input) => OnRectInputFieldChanged(input, m_CoopMenuRectInput));
            m_CoopMenuRectEdit.SetIsOnWithoutNotify(false);
            m_CoopMenuRectEdit.onValueChanged.AddListener(OnClickCoopMenuRectEdit);

            rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.CommanderName];
            m_CommanderNameRectText.text = rectAnchor.ToString();
            m_CommanderNameRectInput.SetTextWithoutNotify(rectAnchor.ToString());
            m_CommanderNameRectInput.onValueChanged.AddListener((string input) => OnRectInputFieldChanged(input, m_CommanderNameRectInput));
            m_CommanderNameRectEdit.SetIsOnWithoutNotify(false);
            m_CommanderNameRectEdit.onValueChanged.AddListener(OnClickCommanderNameRectEdit);

            rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.Masteries];
            m_MasteriesRectText.text = rectAnchor.ToString();
            m_MasteriesRectInput.SetTextWithoutNotify(rectAnchor.ToString());
            m_MasteriesRectInput.onValueChanged.AddListener((string input) => OnRectInputFieldChanged(input, m_MasteriesRectInput));
            m_MasteriesRectEdit.SetIsOnWithoutNotify(false);
            m_MasteriesRectEdit.onValueChanged.AddListener(OnClickMasteriesRectEdit);
        }

        private void OnClickCoopMenuRectEdit(bool enable)
        {
            if (enable)
            {
                m_CoopMenuRectText.gameObject.SetActive(false);
                m_CoopMenuRectInput.gameObject.SetActive(true);
                RectAnchor rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.CoopMenu];
                m_CoopMenuRectInput.SetTextWithoutNotify(rectAnchor.ToString());
                m_CoopMenuRectInput.textComponent.color = Color.black;
            }
            else
            {
                string input = m_CoopMenuRectInput.text;
                if (RectAnchor.TryParse(input, out RectAnchor rectAnchor))
                    SettingDialog.UserSetting.RectPositions[RectAnchorKey.CoopMenu] = rectAnchor;
                else
                    rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.CoopMenu];
                m_CoopMenuRectText.gameObject.SetActive(true);
                m_CoopMenuRectText.text = rectAnchor.ToString();
                m_CoopMenuRectInput.gameObject.SetActive(false);
            }
        }
        private void OnClickCommanderNameRectEdit(bool enable)
        {
            if (enable)
            {
                m_CommanderNameRectText.gameObject.SetActive(false);
                m_CommanderNameRectInput.gameObject.SetActive(true);
                RectAnchor rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.CommanderName];
                m_CommanderNameRectInput.SetTextWithoutNotify(rectAnchor.ToString());
                m_CommanderNameRectInput.textComponent.color = Color.black;
            }
            else
            {
                string input = m_CommanderNameRectInput.text;
                if (RectAnchor.TryParse(input, out RectAnchor rectAnchor))
                    SettingDialog.UserSetting.RectPositions[RectAnchorKey.CommanderName] = rectAnchor;
                else
                    rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.CommanderName];
                m_CommanderNameRectText.gameObject.SetActive(true);
                m_CommanderNameRectText.text = rectAnchor.ToString();
                m_CommanderNameRectInput.gameObject.SetActive(false);
            }
        }
        private void OnClickMasteriesRectEdit(bool enable)
        {
            if (enable)
            {
                m_MasteriesRectText.gameObject.SetActive(false);
                m_MasteriesRectInput.gameObject.SetActive(true);
                RectAnchor rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.Masteries];
                m_MasteriesRectInput.SetTextWithoutNotify(rectAnchor.ToString());
                m_MasteriesRectInput.textComponent.color = Color.black;
            }
            else
            {
                string input = m_MasteriesRectInput.text;
                if (RectAnchor.TryParse(input, out RectAnchor rectAnchor))
                    SettingDialog.UserSetting.RectPositions[RectAnchorKey.Masteries] = rectAnchor;
                else
                    rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.Masteries];
                m_MasteriesRectText.gameObject.SetActive(true);
                m_MasteriesRectText.text = rectAnchor.ToString();
                m_MasteriesRectInput.gameObject.SetActive(false);
            }
        }
    }
}