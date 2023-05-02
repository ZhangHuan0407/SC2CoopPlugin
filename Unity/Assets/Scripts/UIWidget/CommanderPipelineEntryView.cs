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
        private Text m_Masteries;
        [SerializeField]
        private Text m_Title;
        [SerializeField]
        private Text m_FileName;
        [SerializeField]
        private Button m_SelectButton;
        public Button SelectButton => m_SelectButton;

        public void SetEntry(CommanderPipelineTable.Entry entry)
        {
            m_CommanderName.text = TableManager.LocalizationTable[entry.CommanderName];
            m_Level.text = entry.Level.ToString();
            m_Masteries.text = 0.ToString();
            m_Title.text = entry.Title;
            string fullName = entry.Fileinfo.FullName;
            if (fullName.Length > 120)
                fullName = $"{fullName.Substring(0, 60)} ... {fullName.Substring(fullName.Length - 55)}";
            m_FileName.text = fullName;
        }
    }
}