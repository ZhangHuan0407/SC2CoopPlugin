using System;
using Table;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extension;

namespace Game.UI
{
    public class PluginSettingPage : SettingPage
    {
        [SerializeField]
        private InputField m_InterfaceLanguage;
        [SerializeField]
        private Dropdown m_InGameLanguageDropDown;
        [SerializeField]
        private Toggle m_IsProgrammer;

        private void Start()
        {
            m_InterfaceLanguage.SetTextWithoutNotify(SettingDialog.UserSetting.InGameLanguage);
            string usedLanguage = SettingDialog.UserSetting.InterfaceLanguage.ToString();
            m_InGameLanguageDropDown.SetValueWithoutNotify(m_InGameLanguageDropDown.options.FindIndex(data => data.text == usedLanguage));
            m_InGameLanguageDropDown.onValueChanged.AddListener((int index) =>
            {
                SystemLanguage language = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), m_InGameLanguageDropDown.options[index].text);
                SettingDialog.UserSetting.InterfaceLanguage = language;
                TableManager.LoadLocalizationTable(language);
            });
            m_IsProgrammer.SetIsOnWithoutNotify(SettingDialog.UserSetting.IsProgrammer);
            m_IsProgrammer.onValueChanged.AddListener((bool value) =>
            {
                SettingDialog.UserSetting.IsProgrammer = value;
            });
        }
    }
}