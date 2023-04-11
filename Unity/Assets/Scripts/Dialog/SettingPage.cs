using System;
using Table;
using UnityEngine;

namespace Game
{
    public abstract class SettingPage : MonoBehaviour
    {
        [SerializeField]
        private Transform m_TabListPosition;
        public Transform TabListPosition => m_TabListPosition;

        [SerializeField]
        private SettingDialog m_SettingDialog;
    }
}