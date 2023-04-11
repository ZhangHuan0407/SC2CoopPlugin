using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public abstract class SettingPage : MonoBehaviour
    {
        [SerializeField]
        private Transform m_TabListPosition;
        public Transform TabListPosition => m_TabListPosition;

        [SerializeField]
        private SettingDialog m_SettingDialog;
        public SettingDialog SettingDialog;

        [SerializeField]
        private Button m_TabButton;
        public Button TabButton => m_TabButton;

        public bool HaveLookup { get; set; }
    }
}