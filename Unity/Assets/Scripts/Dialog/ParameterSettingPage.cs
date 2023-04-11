using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extension;

namespace Game
{
    public class ParameterSettingPage : SettingPage
    {
        [SerializeField]
        private Dropdown m_InterfaceLanguage;
        [SerializeField]
        private DropdownSwitch m_InGameLanguage;
        [SerializeField]
        private Toggle m_IsProgrammer;

        private void Awake()
        {
            //m_InterfaceLanguage.
            m_IsProgrammer.onValueChanged.AddListener((bool value) =>
            {
                Global.UserSetting.IsProgrammer = value;
            });
        }
    }
}