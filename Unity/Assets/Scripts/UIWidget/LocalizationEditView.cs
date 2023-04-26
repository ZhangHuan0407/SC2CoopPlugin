using System;
using Game.Model;
using UnityEngine;
using UnityEngine.UI;
using Table;

namespace Game.UI
{
    public class LocalizationEditView : MonoBehaviour
    {
        public CommanderContentDialog CommanderContentDialog { get; set; }
        private CommanderPipeline m_CommanderPipeline;

        private void Awake()
        {
            
        }

        public void SetCommanderPipeline(CommanderPipeline pipeline)
        {
            m_CommanderPipeline = pipeline;
        }
    }
}