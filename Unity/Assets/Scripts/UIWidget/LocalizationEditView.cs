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
        private CommanderModel m_CommanderModel;

        public void SetCommanderModel(CommanderModel model)
        {
            m_CommanderModel = model;
        }
    }
}