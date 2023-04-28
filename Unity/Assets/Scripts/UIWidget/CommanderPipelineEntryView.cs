using System;
using Table;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class CommanderPipelineEntryView : MonoBehaviour
    {
        [SerializeField]
        private Text m_CommanderName;
        [SerializeField]
        private Text m_Level;
        [SerializeField]
        private Text m_Title;
        [SerializeField]
        private Button m_SelectButton;
        public Button SelectButton => m_SelectButton;

        public void SetEntry(CommanderPipelineTable.Entry entry)
        {
            m_CommanderName.text = TableManager.LocalizationTable[entry.CommanderName];
            m_Level.text = entry.Level.ToString();
            m_Title.text = entry.Title;
        }
    }
}