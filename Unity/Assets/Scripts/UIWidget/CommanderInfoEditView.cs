using System;
using System.Linq;
using Game.Model;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extension;
using Table;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Game.UI
{
    public class CommanderInfoEditView : MonoBehaviour
    {
        public CommanderContentDialog CommanderContentDialog { get; set; }
        private CommanderPipeline m_CommanderPipeline;

        private static List<(CommanderName name, int dropdownIndex)> m_DropdownValues;

        [SerializeField]
        private Dropdown m_CommanderNameDropDown;
        [SerializeField]
        private Dropdown m_LanguageDropDown;
        [SerializeField]
        private Slider m_LevelSlider;
        [SerializeField]
        private Text m_LevelValueText;
        [SerializeField]
        private Text[] m_MasteriesNameTexts;
        [SerializeField]
        private Slider[] m_MasteriesSliders;
        [SerializeField]
        private Text[] m_MasteriesValueTexts;
        [SerializeField]
        private Text[] m_PrestigeNameTexts;
        [SerializeField]
        private Toggle[] m_PrestigeToggles;
        [SerializeField]

        private InputField m_TitleInput;
        [SerializeField]
        private InputField m_DescInput;
        [SerializeField]
        private InputField m_GuidInput;
        [SerializeField]
        private InputField m_DemoInput;
        [SerializeField]
        private Button m_DemoButton;

        private void Awake()
        {
            if (m_DropdownValues is null)
            {
                m_DropdownValues = new List<(CommanderName, int)>();
                foreach (CommanderName name in Enum.GetValues(typeof(CommanderName)))
                    m_DropdownValues.Add((name, m_DropdownValues.Count));
            }

            m_CommanderNameDropDown.ClearOptions();
            for (int i = 0; i < m_DropdownValues.Count; i++)
            {
                string name = TableManager.LocalizationTable[m_DropdownValues[i].name];
                m_CommanderNameDropDown.options.Add(new Dropdown.OptionData(name));
            }
            m_CommanderNameDropDown.onValueChanged.AddListener(OnCommanderNameDropDown_ValueChanged);

            m_LanguageDropDown.ClearOptions();
            for (int i = 0; i < GameDefined.SupportedLanguages.Count; i++)
            {
                string languageName = TableManager.LocalizationTable[GameDefined.SupportedLanguages[i]];
                m_LanguageDropDown.options.Add(new Dropdown.OptionData(languageName));
            }
            m_LanguageDropDown.value = 0;
            m_LanguageDropDown.onValueChanged.AddListener(OnLanguageDropDown_ValueChanged);

            (m_LevelSlider.GetComponent<SliderEndEdit>()).onEndEdit.AddListener(OnLevelSlider_ValueChanged);
            m_LevelSlider.onValueChanged.AddListener(i => m_LevelValueText.text = Mathf.RoundToInt(i).ToString());

            for (int i = 0; i < m_MasteriesSliders.Length; i++)
            {
                Slider slider = m_MasteriesSliders[i];
                Text text = m_MasteriesValueTexts[i];
                slider.onValueChanged.AddListener((float value) => text.text = Mathf.RoundToInt(value).ToString());
               (slider.GetComponent<SliderEndEdit>()).onEndEdit.AddListener((float value) =>
                                                                           {
                                                                               OnMasteriySlider_EndEdit(slider, text, value);
                                                                           });
            }
            for (int i = 0; i < m_PrestigeToggles.Length; i++)
            {
                Toggle toggle = m_PrestigeToggles[i];
                toggle.onValueChanged.AddListener((bool value) =>
                                                 {
                                                     OnPrestigeToggle_ValueChanged(toggle, value);
                                                 });
            }

            m_TitleInput.onEndEdit.AddListener(OnTitleInput_EndEdit);
            m_DescInput.onEndEdit.AddListener(OnDescInput_EndEdit);
            m_GuidInput.onEndEdit.AddListener(OnGuidInput_EndEdit);

            m_DemoInput.onEndEdit.AddListener(OnDemoInput_EndEdit);
            m_DemoButton.onClick.AddListener(OnClickDemoButton);
        }

        public void SetCommanderPipeline(CommanderPipeline pipeline)
        {
            m_CommanderPipeline = pipeline;
            int index = 0;
            for (int i = 0; i < m_DropdownValues.Count; i++)
            {
                if (m_DropdownValues[i].name == pipeline.Commander)
                {
                    index = i;
                    break;
                }
            }
            m_CommanderNameDropDown.SetValueWithoutNotify(index);

            m_LevelSlider.value = pipeline.Level;
            m_LevelValueText.text = pipeline.Level.ToString();
            int languageIndex = GameDefined.SupportedLanguages.IndexOf(pipeline.Language);
            if (languageIndex == -1)
                languageIndex = 0;
            m_LanguageDropDown.value = languageIndex;

            for (int i = 0; i < pipeline.Masteries.Length; i++)
            {
                m_MasteriesSliders[i].SetValueWithoutNotify(pipeline.Masteries[i]);
                m_MasteriesValueTexts[i].text = pipeline.Masteries[i].ToString();
            }
            m_PrestigeToggles[pipeline.Prestige].SetIsOnWithoutNotify(true);
            OnChangeCommanderName(m_CommanderPipeline.Commander);
            m_TitleInput.SetTextWithoutNotify(m_CommanderPipeline.Title);
            m_DescInput.SetTextWithoutNotify(m_CommanderPipeline.Desc);
            m_GuidInput.SetTextWithoutNotify(m_CommanderPipeline.Guid.ToString());
            m_CommanderPipeline.DemoURL = m_CommanderPipeline.DemoURL ?? string.Empty;
            m_DemoInput.SetTextWithoutNotify(m_CommanderPipeline.DemoURL);
        }
        private void OnChangeCommanderName(CommanderName commanderName)
        {
            LogService.System(nameof(OnChangeCommanderName), commanderName.ToString());
            if (TableManager.MasteriesTable.Data.TryGetValue(commanderName, out MasteriesTable.Entry[] entries))
            {
                for (int i = 0; i < m_MasteriesNameTexts.Length; i++)
                {
                    MasteriesTable.Entry entry = entries[i];
                    m_MasteriesNameTexts[i].text = entry.Name.Localization;
                }
            }
            else
            {
                string content = TableManager.LocalizationTable["UI.CommanderEditor.UnknownMasteries"];
                for (int i = 0; i < m_MasteriesNameTexts.Length; i++)
                    m_MasteriesNameTexts[i].text = string.Format(content, i + 1);
            }
            if (TableManager.PrestigeTable.Data.TryGetValue(commanderName, out PrestigeTable.Entry[] entries2))
            {
                for (int i = 0; i < m_PrestigeNameTexts.Length; i++)
                {
                    PrestigeTable.Entry entry = entries2[i];
                    m_PrestigeNameTexts[i].text = entry.Name.Localization;
                }
            }
            else
            {
                string content = TableManager.LocalizationTable["UI.CommanderEditor.UnknownPrestige"];
                for (int i = 0; i < m_PrestigeNameTexts.Length; i++)
                    m_PrestigeNameTexts[i].text = string.Format(content, i + 1);
            }
        }

        private void OnCommanderNameDropDown_ValueChanged(int newIndex)
        {
            LogService.System(nameof(OnCommanderNameDropDown_ValueChanged), newIndex.ToString());
            CommanderName newCommanderName = CommanderName.None;
            CommanderName oldCommanderName = m_CommanderPipeline.Commander;
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
                                                    dialog.CommanderPipeline.Commander = newCommanderName;
                                                    m_CommanderNameDropDown.SetValueWithoutNotify(newIndex);
                                                    OnChangeCommanderName(newCommanderName);
                                                },
                                                (dialog) =>
                                                {
                                                    dialog.CommanderPipeline.Commander = oldCommanderName;
                                                    m_CommanderNameDropDown.SetValueWithoutNotify(oldIndex);
                                                    OnChangeCommanderName(oldCommanderName);
                                                });
            CommanderContentDialog.Redo();
        }

        private void OnLanguageDropDown_ValueChanged(int newValue)
        {
            SystemLanguage newLanguage = GameDefined.SupportedLanguages[newValue];
            SystemLanguage oldLanguage = m_CommanderPipeline.Language;
            CommanderContentDialog.AppendRecord(nameof(OnCommanderNameDropDown_ValueChanged),
                                                (dialog) =>
                                                {
                                                    dialog.CommanderPipeline.Language = newLanguage;
                                                    m_LanguageDropDown.SetValueWithoutNotify(GameDefined.SupportedLanguages.IndexOf(newLanguage));
                                                },
                                                (dialog) =>
                                                {
                                                    dialog.CommanderPipeline.Language = oldLanguage;
                                                    m_LanguageDropDown.SetValueWithoutNotify(GameDefined.SupportedLanguages.IndexOf(oldLanguage));
                                                });
            CommanderContentDialog.Redo();
        }

        private void OnLevelSlider_ValueChanged(float newValue)
        {
            int newLevel = Mathf.RoundToInt(newValue);
            int oldLevel = m_CommanderPipeline.Level;
            CommanderContentDialog.AppendRecord(nameof(OnLevelSlider_ValueChanged),
                                                (dialog) =>
                                                {
                                                    dialog.CommanderPipeline.Level = newLevel;
                                                    m_LevelSlider.SetValueWithoutNotify(newLevel);
                                                    m_LevelValueText.text = newLevel.ToString();
                                                },
                                                (dialog) =>
                                                {
                                                    dialog.CommanderPipeline.Level = oldLevel;
                                                    m_LevelSlider.SetValueWithoutNotify(oldLevel);
                                                    m_LevelValueText.text = oldLevel.ToString();
                                                });
            CommanderContentDialog.Redo();
        }
        private void OnMasteriySlider_EndEdit(Slider sender, Text text, float newValue)
        {
            int index = Array.IndexOf(m_MasteriesSliders, sender);
            int oldMastery = m_CommanderPipeline.Masteries[index];
            int newMastety = Mathf.RoundToInt(newValue);
            CommanderContentDialog.AppendRecord(nameof(OnMasteriySlider_EndEdit),
                                                (dialog) =>
                                                {
                                                    dialog.CommanderPipeline.Masteries[index] = newMastety;
                                                    text.text = newMastety.ToString();
                                                    m_MasteriesSliders[index].SetValueWithoutNotify(newMastety);
                                                },
                                                (dialog) =>
                                                {
                                                    dialog.CommanderPipeline.Masteries[index] = oldMastery;
                                                    text.text = oldMastery.ToString();
                                                    m_MasteriesSliders[index].SetValueWithoutNotify(oldMastery);
                                                });
            CommanderContentDialog.Redo();
        }

        private void OnPrestigeToggle_ValueChanged(Toggle sender, bool newValue)
        {
            if (!newValue)
                return;

            int newPrestige = Array.IndexOf(m_PrestigeToggles, sender);
            int oldPrestige = m_CommanderPipeline.Prestige;
            CommanderContentDialog.AppendRecord(nameof(OnPrestigeToggle_ValueChanged),
                                                (dialog) =>
                                                {
                                                    dialog.CommanderPipeline.Prestige = newPrestige;
                                                    m_PrestigeToggles[newPrestige].SetIsOnWithoutNotify(true);
                                                },
                                                (dialog) =>
                                                {
                                                    dialog.CommanderPipeline.Prestige = oldPrestige;
                                                    m_PrestigeToggles[oldPrestige].SetIsOnWithoutNotify(true);
                                                });
            CommanderContentDialog.Redo();
        }

        private void OnTitleInput_EndEdit(string input)
        {
            string oldContent = m_CommanderPipeline.Title;
            CommanderContentDialog.AppendRecord(nameof(OnTitleInput_EndEdit),
                                                (dialog) =>
                                                {
                                                    dialog.CommanderPipeline.Title = input;
                                                    m_TitleInput.SetTextWithoutNotify(input);
                                                },
                                                (dialog) =>
                                                {
                                                    dialog.CommanderPipeline.Title = oldContent;
                                                    m_TitleInput.SetTextWithoutNotify(oldContent);
                                                });
            CommanderContentDialog.Redo();
        }

        private void OnDescInput_EndEdit(string input)
        {
            string oldContent = m_CommanderPipeline.Desc;
            CommanderContentDialog.AppendRecord(nameof(OnDescInput_EndEdit),
                                                (dialog) =>
                                                {
                                                    dialog.CommanderPipeline.Desc = input;
                                                    m_DescInput.SetTextWithoutNotify(input);
                                                },
                                                (dialog) =>
                                                {
                                                    dialog.CommanderPipeline.Desc = oldContent;
                                                    m_DescInput.SetTextWithoutNotify(oldContent);
                                                });
            CommanderContentDialog.Redo();
        }

        private void OnGuidInput_EndEdit(string input)
        {
            string oldContent = m_CommanderPipeline.Guid.ToString();
            CommanderContentDialog.AppendRecord(nameof(OnGuidInput_EndEdit),
                                               (dialog) =>
                                               {
                                                   Guid.TryParse(input, out Guid resut);
                                                   dialog.CommanderPipeline.Guid = resut;
                                                   m_DescInput.SetTextWithoutNotify(resut.ToString());
                                               },
                                               (dialog) =>
                                               {
                                                   Guid.TryParse(oldContent, out Guid resut);
                                                   dialog.CommanderPipeline.Guid = resut;
                                                   m_DescInput.SetTextWithoutNotify(resut.ToString());
                                               });
            CommanderContentDialog.Redo();
        }

        private void OnDemoInput_EndEdit(string input)
        {
            string oldContent = m_CommanderPipeline.DemoURL;
            CommanderContentDialog.AppendRecord(nameof(OnDemoInput_EndEdit),
                                               (dialog) =>
                                               {
                                                   dialog.CommanderPipeline.DemoURL = input;
                                                   m_DemoInput.SetTextWithoutNotify(input);
                                               },
                                               (dialog) =>
                                               {
                                                   dialog.CommanderPipeline.DemoURL = oldContent;
                                                   m_DemoInput.SetTextWithoutNotify(oldContent);
                                               });
            CommanderContentDialog.Redo();
        }
        private void OnClickDemoButton()
        {
            m_CommanderPipeline.DemoURL = m_CommanderPipeline.DemoURL.Trim();
            if (Regex.IsMatch(m_CommanderPipeline.DemoURL, "\\Ahttp(s)?://\\S+\\Z"))
                Application.OpenURL(m_CommanderPipeline.DemoURL);
        }
    }
}