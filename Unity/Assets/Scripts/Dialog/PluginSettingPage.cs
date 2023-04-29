using System;
using System.Collections.Generic;
using Table;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class PluginSettingPage : SettingPage
    {
        private List<(string languageName, SystemLanguage language)> SystemLanguageList = new List<(string, SystemLanguage)>()
        {
            (null, SystemLanguage.ChineseSimplified),
            (null, SystemLanguage.ChineseTraditional),
            (null, SystemLanguage.English),
            (null, SystemLanguage.French),
            (null, SystemLanguage.German),
            (null, SystemLanguage.Korean),
        };
        [SerializeField]
        private Dropdown m_InterfaceLanguage;
        [SerializeField]
        private InputField m_InGameLanguageDropDown;
        [SerializeField]
        private Toggle m_IsProgrammer;

        private void Start()
        {
            m_InGameLanguageDropDown.SetTextWithoutNotify(SettingDialog.UserSetting.InGameLanguage);
            for (int i = 0; i < SystemLanguageList.Count; i++)
            {
                string languageName = TableManager.LocalizationTable[SystemLanguageList[i].language];
                SystemLanguageList[i] = (languageName, SystemLanguageList[i].language);
            }
            m_InterfaceLanguage.ClearOptions();
            for (int i = 0;i < SystemLanguageList.Count; i++)
                m_InterfaceLanguage.options.Add(new Dropdown.OptionData(SystemLanguageList[i].languageName));
            SystemLanguage usedLanguage = SettingDialog.UserSetting.InterfaceLanguage;
            m_InterfaceLanguage.SetValueWithoutNotify(SystemLanguageList.FindIndex(data => data.language == usedLanguage));
            m_InterfaceLanguage.onValueChanged.AddListener((int index) =>
            {
                SettingDialog.UserSetting.InterfaceLanguage = SystemLanguageList[index].language;
                TableManager.LoadLocalizationTable(SystemLanguageList[index].language);
            });
            m_IsProgrammer.SetIsOnWithoutNotify(SettingDialog.UserSetting.IsProgrammer);
            m_IsProgrammer.onValueChanged.AddListener((bool value) =>
            {
                SettingDialog.UserSetting.IsProgrammer = value;
            });
        }
    }
}