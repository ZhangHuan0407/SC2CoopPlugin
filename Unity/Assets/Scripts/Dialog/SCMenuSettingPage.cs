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

        private void Awake()
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
    }
}