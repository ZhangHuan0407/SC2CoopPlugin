using System;
using UnityEngine;
using UnityEngine.UI;
using Table;

namespace Game.UI
{
    public class LocalizationLabel : MonoBehaviour
    {
        [SerializeField]
        private Text m_Text;
        [SerializeField]
        private string m_StringID;

        private void Awake()
        {
            LocalizationLabelRefresh();
        }
        public void LocalizationLabelRefresh()
        {
            m_Text.text = TableManager.LocalizationTable[m_StringID];
        }
    }
}