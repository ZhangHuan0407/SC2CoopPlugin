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
        private Dropdown m_InterfaceLanguage;
        [SerializeField]
        private InputField m_InGameLanguageDropDown;
        [SerializeField]
        private Toggle m_IsProgrammer;

        private void Start()
        {
            m_InGameLanguageDropDown.SetTextWithoutNotify(SettingDialog.UserSetting.InGameLanguage);
            string usedLanguage = SettingDialog.UserSetting.InterfaceLanguage.ToString();
            m_InterfaceLanguage.SetValueWithoutNotify(m_InterfaceLanguage.options.FindIndex(data => data.text == usedLanguage));
            m_InterfaceLanguage.onValueChanged.AddListener((int index) =>
            {
                SystemLanguage language = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), m_InterfaceLanguage.options[index].text);
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