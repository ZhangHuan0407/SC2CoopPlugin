using Game.OCR;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Table;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class PluginSettingPage : SettingPage
    {
        private List<(string languageName, SystemLanguage language)> SystemLanguageList;
        [SerializeField]
        private Dropdown m_InterfaceLanguage;
        [SerializeField]
        private Dropdown m_InGameLanguageDropDown;
        [SerializeField]
        private Dropdown m_CanvasResolutionDropDown;
        [SerializeField]
        private Toggle m_IsProgrammer;

        private void Start()
        {
            (HeadData, AvailableRecognizerLanguages_Response) response = default;
            Global.BackThread.WaitingBackThreadTweener(() =>
            {
                var task = OCRConnectorA.Instance.SendRequestAsync<AvailableRecognizerLanguages_Response>(ProtocolId.AvailableRecognizerLanguages,
                                                                                                          new AvailableRecognizerLanguages_Request());
                response = task.GetAwaiter().GetResult();
            })
                .OnComplete(() =>
                {
                    if (response.Item1.StatusCode == ErrorCode.OK &&
                        this != null)
                    {
                        int index = 0;
                        m_InGameLanguageDropDown.ClearOptions();
                        for (int i = 0; i < response.Item2.Languages.Count; i++)
                        {
                            AvailableRecognizerLanguages_Response.LanguageItem languageItem = response.Item2.Languages[i];
                            m_InGameLanguageDropDown.options.Add(new Dropdown.OptionData($"({languageItem.LanguageTag}) {languageItem.DisplayName} {languageItem.NativeName}"));
                            if (SettingDialog.UserSetting.InGameLanguage == languageItem.LanguageTag)
                                index = i;
                        }
                        // unity 2021.3.23f1 bug, options数量变为1，value 0 变为 0，选中的显示不刷新
                        // 此处先改为-1，再改为0，让其检查到变更
                        m_InGameLanguageDropDown.SetValueWithoutNotify(-1);
                        m_InGameLanguageDropDown.value = index;
                    }
                })
                .DoIt();
            m_InGameLanguageDropDown.onValueChanged.AddListener((i) =>
            {
                string text = m_InGameLanguageDropDown.options[i].text;
                Match match = Regex.Match(text, "\\((?<LanguageTag>[a-zA-Z-_])+\\)");
                LogService.System("PluginSettingPage.InGameLanguageDropDown", $"text: {text}, match: {match.Success}");
                SettingDialog.UserSetting.InGameLanguage = match.Groups["LanguageTag"].Value;
            });
            
            SystemLanguageList = new List<(string languageName, SystemLanguage language)>();
            for (int i = 0; i < GameDefined.SupportedLanguages.Count; i++)
            {
                SystemLanguage systemLanguage = GameDefined.SupportedLanguages[i];
                string languageName = TableManager.LocalizationTable[systemLanguage];
                SystemLanguageList.Add((languageName, systemLanguage));
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

            m_CanvasResolutionDropDown.ClearOptions();
            for (int i = 0; i < GameDefined.StdCanvasResolution.Count; i++)
            {
                Vector2Int resolution = GameDefined.StdCanvasResolution[i];
                string str = $"{resolution.x}x{resolution.y}";
                m_CanvasResolutionDropDown.options.Add(new Dropdown.OptionData(str));
            }
            m_CanvasResolutionDropDown.value = -1;
            for (int i = 0; i < GameDefined.StdCanvasResolution.Count; i++)
            {
                Vector2Int resolution = GameDefined.StdCanvasResolution[i];
                if (resolution == SettingDialog.UserSetting.CanvasResolution)
                    m_CanvasResolutionDropDown.SetValueWithoutNotify(i);
            }
            m_CanvasResolutionDropDown.onValueChanged.AddListener((int index) =>
            {
                SettingDialog.UserSetting.CanvasResolution = GameDefined.StdCanvasResolution[index];
                CameraCanvas.ReferenceResolution = SettingDialog.UserSetting.CanvasResolution;
            });
        }
    }
}