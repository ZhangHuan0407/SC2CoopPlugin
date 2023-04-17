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
            m_CoopMenuRectEdit.SetIsOnWithoutNotify(false);
            m_CoopMenuRectEdit.onValueChanged.AddListener(OnClickCoopMenuRectEdit);

            rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.CommanderName];
            m_CommanderNameRectText.text = rectAnchor.ToString();
            m_CommanderNameRectInput.SetTextWithoutNotify(rectAnchor.ToString());
            m_CommanderNameRectEdit.SetIsOnWithoutNotify(false);
            m_CommanderNameRectEdit.onValueChanged.AddListener(OnClickCommanderNameRectEdit);

            rectAnchor = SettingDialog.UserSetting.RectPositions[RectAnchorKey.Masteries];
            m_MasteriesRectText.text = rectAnchor.ToString();
            m_MasteriesRectInput.SetTextWithoutNotify(rectAnchor.ToString());
            m_MasteriesRectEdit.SetIsOnWithoutNotify(false);
            m_MasteriesRectEdit.onValueChanged.AddListener(OnClickMasteriesRectEdit);
        }

        private void OnClickCoopMenuRectEdit(bool enable)
        {
            if (enable)
            {
                m_CoopMenuRectText.gameObject.SetActive(false);
                m_CoopMenuRectInput.gameObject.SetActive(true);
            }
            else
            {
                m_CoopMenuRectText.gameObject.SetActive(true);
                m_CoopMenuRectInput.gameObject.SetActive(false);
            }
        }
        private void OnClickCommanderNameRectEdit(bool enable)
        {

        }
        private void OnClickMasteriesRectEdit(bool enable)
        {

        }
    }
}