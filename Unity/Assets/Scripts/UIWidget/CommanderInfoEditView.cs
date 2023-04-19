using System;
using System.Linq;
using Game.Model;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extension;
using Table;
using System.Collections.Generic;

namespace Game.UI
{
    public class CommanderInfoEditView : MonoBehaviour
    {
        public CommanderContentDialog CommanderContentDialog { get; set; }
        private CommanderModel m_CommanderModel;

        private static List<(CommanderName name, int dropdownIndex)> m_DropdownValues;

        [SerializeField]
        private Dropdown m_CommanderNameDropDown;
        [SerializeField]
        private Slider[] m_MasteriesSliders;
        [SerializeField]
        private Toggle[] m_PrestigesToggles;
        [SerializeField]
        private InputField m_TitleInput;
        [SerializeField]
        private InputField m_DescInput;

        private void Awake()
        {
            if (m_DropdownValues is null)
            {
                m_DropdownValues = new List<(CommanderName, int)>();
                foreach (CommanderName name in Enum.GetValues(typeof(CommanderName)))
                    m_DropdownValues.Add((name, m_DropdownValues.Count));
            }

            m_CommanderNameDropDown.ClearOptions();
            List<string> commanderNames = new List<string>();
            for (int i = 0; i < m_DropdownValues.Count; i++)
                commanderNames.Add(TableManager.LocalizationTable[m_DropdownValues[i].name]);
            m_CommanderNameDropDown.AddOptions(commanderNames);
            m_CommanderNameDropDown.onValueChanged.AddListener(OnCommanderNameDropDown_ValueChanged);

            for (int i = 0; i < m_MasteriesSliders.Length; i++)
            {
                Slider slider = m_MasteriesSliders[i];
               (slider.GetComponent<SliderEndEdit>()).onEndEdit.AddListener((float value) =>
                                                                           {
                                                                               OnMasteriySlider_EndEdit(slider, value);
                                                                           });
            }
            for (int i = 0; i < m_PrestigesToggles.Length; i++)
            {
                Toggle toggle = m_PrestigesToggles[i];
                toggle.onValueChanged.AddListener((bool value) =>
                                                 {
                                                     OnPrestigeToggle_ValueChanged(toggle, value);
                                                 });
            }

            m_TitleInput.onEndEdit.AddListener(OnTitleInput_EndEdit);
            m_DescInput.onEndEdit.AddListener(OnDescInput_EndEdit);
        }

        public void SetCommanderModel(CommanderModel model)
        {
            m_CommanderModel = model;
            string commanderPrefix = ((int)m_CommanderModel.Commander).ToString();
            int index = -1;
            for (int i = 0; i < m_CommanderNameDropDown.options.Count; i++)
            {
                if (m_CommanderNameDropDown.options[i].text.StartsWith(commanderPrefix))
                {
                    index = i;
                    break;
                }
            }
            m_CommanderNameDropDown.SetValueWithoutNotify(index);
            for (int i = 0; i < model.Masteries.Length; i++)
                m_MasteriesSliders[i].SetValueWithoutNotify(model.Masteries[i] * 30.49f);
            m_PrestigesToggles[model.Prestige].SetIsOnWithoutNotify(true);
        }

        private void OnCommanderNameDropDown_ValueChanged(int newIndex)
        {
            CommanderName newCommanderName = CommanderName.None;
            CommanderName oldCommanderName = m_CommanderModel.Commander;
            int oldIndex = 0;
            foreach ((CommanderName name, int dropdownIndex) pair in m_DropdownValues)
            {
                if (pair.dropdownIndex == newIndex)
                    newCommanderName = pair.name;
                if (pair.name == oldCommanderName)
                    oldIndex = pair.dropdownIndex;
            }
            CommanderContentDialog.AppendRecord(nameof(OnCommanderNameDropDown_ValueChanged),
                                                (dialog) =>
                                                {
                                                    dialog.CommanderModel.Commander = newCommanderName;
                                                    m_CommanderNameDropDown.SetValueWithoutNotify(newIndex);
                                                },
                                                (dialog) =>
                                                {
                                                    dialog.CommanderModel.Commander = oldCommanderName;
                                                    m_CommanderNameDropDown.SetValueWithoutNotify(oldIndex);
                                                });
        }
        private void OnMasteriySlider_EndEdit(Slider sender, float newValue)
        {
            int index = Array.IndexOf(m_MasteriesSliders, sender);
            int oldMastery = m_CommanderModel.Masteries[index];
            int newMastety = Mathf.RoundToInt(newValue * 30.49f);
            CommanderContentDialog.AppendRecord(nameof(OnMasteriySlider_EndEdit),
                                                (dialog) =>
                                                {
                                                    dialog.CommanderModel.Masteries[index] = newMastety;
                                                    m_MasteriesSliders[index].SetValueWithoutNotify(newValue);
                                                },
                                                (dialog) =>
                                                {
                                                    dialog.CommanderModel.Masteries[index] = oldMastery;
                                                    m_MasteriesSliders[index].SetValueWithoutNotify(oldMastery / 30.49f);
                                                });
        }
        private void OnPrestigeToggle_ValueChanged(Toggle sender, bool newValue)
        {
            if (!newValue)
                return;

            int newPrestige = Array.IndexOf(m_PrestigesToggles, sender);
            int oldPrestige = m_CommanderModel.Prestige;
            CommanderContentDialog.AppendRecord(nameof(OnPrestigeToggle_ValueChanged),
                                                (dialog) =>
                                                {
                                                    dialog.CommanderModel.Prestige = newPrestige;
                                                    m_PrestigesToggles[newPrestige].SetIsOnWithoutNotify(true);
                                                },
                                                (dialog) =>
                                                {
                                                    dialog.CommanderModel.Prestige = oldPrestige;
                                                    m_PrestigesToggles[oldPrestige].SetIsOnWithoutNotify(true);
                                                });
        }
        private void OnTitleInput_EndEdit(string input)
        {
            string oldContent = m_CommanderModel.Title;
            CommanderContentDialog.AppendRecord(nameof(OnTitleInput_EndEdit),
                                                (dialog) =>
                                                {
                                                    dialog.CommanderModel.Title = input;
                                                    m_TitleInput.SetTextWithoutNotify(input);
                                                },
                                                (dialog) =>
                                                {
                                                    dialog.CommanderModel.Title = oldContent;
                                                    m_TitleInput.SetTextWithoutNotify(oldContent);
                                                });
        }
        private void OnDescInput_EndEdit(string input)
        {
            string oldContent = m_CommanderModel.Desc;
            CommanderContentDialog.AppendRecord(nameof(OnDescInput_EndEdit),
                                                (dialog) =>
                                                {
                                                    dialog.CommanderModel.Desc = input;
                                                    m_DescInput.SetTextWithoutNotify(input);
                                                },
                                                (dialog) =>
                                                {
                                                    dialog.CommanderModel.Desc = oldContent;
                                                    m_DescInput.SetTextWithoutNotify(oldContent);
                                                });
        }
    }
}