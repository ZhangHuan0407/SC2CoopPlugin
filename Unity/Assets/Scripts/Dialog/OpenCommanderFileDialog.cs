using System;
using System.Collections;
using System.Collections.Generic;
using Table;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class OpenCommanderFileDialog : MonoBehaviour, IDialog
    {
        private static readonly Dictionary<string, SystemLanguage?> StrToLanguage = new Dictionary<string, SystemLanguage?>()
        {
            { "SystemLanguage.All", null },
            { "SystemLanguage.ChineseSimplified", SystemLanguage.ChineseSimplified },
            { "SystemLanguage.ChineseTraditional", SystemLanguage.ChineseTraditional },
            { "SystemLanguage.English", SystemLanguage.English },
            { "SystemLanguage.French", SystemLanguage.French },
            { "SystemLanguage.German", SystemLanguage.German },
            { "SystemLanguage.Korean", SystemLanguage.Korean },
        };
        private static readonly Dictionary<string, CommanderName?> StrToCommander = new Dictionary<string, CommanderName?>()
        {
            { "CommanderName.All", null },
            { "CommanderName.Abathur", CommanderName.Abathur },
            { "CommanderName.Alarak", CommanderName.Alarak },
            { "CommanderName.Artanis", CommanderName.Artanis },
            { "CommanderName.Dehaka", CommanderName.Dehaka },
            { "CommanderName.Fenix", CommanderName.Fenix },
            { "CommanderName.HanAndHorner", CommanderName.HanAndHorner },
            { "CommanderName.Karax", CommanderName.Karax },
            { "CommanderName.Kerrigan", CommanderName.Kerrigan },
            { "CommanderName.Mengsk", CommanderName.Mengsk },
            { "CommanderName.Nova", CommanderName.Nova },
            { "CommanderName.Raynor", CommanderName.Raynor },
            { "CommanderName.Stetmann", CommanderName.Stetmann },
            { "CommanderName.Stukov", CommanderName.Stukov },
            { "CommanderName.Swann", CommanderName.Swann },
            { "CommanderName.Tychus", CommanderName.Tychus },
            { "CommanderName.Vorazun", CommanderName.Vorazun },
            { "CommanderName.Zagara", CommanderName.Zagara },
            { "CommanderName.Zeratul", CommanderName.Zeratul },
        };

        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;
        public bool DestroyFlag { get; set; }
        public string PrefabPath { get; set; }

        public DialogResult DialogResult { get; set; }
        public string CommanderPipelineId { get; set; }

        [SerializeField]
        private InputField m_FilterStrInput;
        [SerializeField]
        private Dropdown m_Language;
        private List<string> m_RawLanguageOptions;
        [SerializeField]
        private Dropdown m_Commander;
        private List<string> m_RawCommanderOptions;
        [SerializeField]
        private Slider m_LevelSlider;
        [SerializeField]
        private Text m_LevelText;

        [SerializeField]
        private Button m_OKButton;
        [SerializeField]
        private Button m_CloseButton;

        [SerializeField]
        private ScrollRect m_ScrollRect;
        [SerializeField]
        private Transform m_ContentTrans;
        [SerializeField]
        private GameObject m_Template;

        private CommanderPipelineTable.Entry[] m_Entries;

        public void Hide()
        {
            m_Canvas.enabled = false;
        }

        public void Show()
        {
            m_Canvas.enabled = true;
        }

        private void Awake()
        {
            m_Language.ClearOptions();
            m_RawLanguageOptions = new List<string>();
            foreach (string key in StrToLanguage.Keys)
            {
                string str = TableManager.LocalizationTable[key];
                m_Language.options.Add(new Dropdown.OptionData(str));
                m_RawLanguageOptions.Add(key);
            }
            m_Language.value = 0;
            string language = PlayerPrefs.GetString(GameDefined.CommanderPreferenceLanguage, Global.UserSetting.InterfaceLanguage.ToString());
            for (int i = 0; i < m_Language.options.Count; i++)
                if (m_RawLanguageOptions[i] == language)
                {
                    m_Language.value = i;
                    break;
                }
            m_Language.onValueChanged.AddListener((_) => FilterCommanderPipelines());

            m_Commander.ClearOptions();
            m_RawCommanderOptions = new List<string>();
            foreach (string key in StrToCommander.Keys)
            {
                string str = TableManager.LocalizationTable[key];
                m_Commander.options.Add(new Dropdown.OptionData(str));
                m_RawCommanderOptions.Add(key);
            }
            m_Commander.value = 0;
            string currentUsedCommander = Global.CurrentUsedCommander == CommanderName.None ? "CommanderName.All" : Global.CurrentUsedCommander.ToString();
            for (int i = 0; i < m_Commander.options.Count; i++)
            {
                if (m_RawCommanderOptions[i] == currentUsedCommander)
                {
                    m_Commander.value = i;
                    break;
                }
            }
            m_Commander.onValueChanged.AddListener((_) => FilterCommanderPipelines());

            m_LevelSlider.minValue = 0.1f;
            m_LevelSlider.maxValue = 15.49f;
            m_LevelSlider.onValueChanged.AddListener((float value) => m_LevelText.text = Mathf.RoundToInt(value).ToString());
            m_LevelSlider.value = 15.49f;

            m_OKButton.onClick.AddListener(OnSelectFile);
            m_OKButton.gameObject.SetActive(false);
            m_CloseButton.onClick.AddListener(OnClickClose);
            m_CloseButton.gameObject.SetActive(false);
        }

        private void Start()
        {
            FilterCommanderPipelines();
        }

        private void FilterCommanderPipelines()
        {
            string languageOption = m_RawLanguageOptions[m_Language.value];
            SystemLanguage? languageValue = StrToLanguage[languageOption];
            string commanderOption = m_RawCommanderOptions[m_Commander.value];
            CommanderName? commanderValue = StrToCommander[commanderOption];
            string str = m_FilterStrInput.text;
            int level = Mathf.RoundToInt(m_LevelSlider.value);
            var commanderPipelineList =  TableManager.CommanderPipelineTable.Filter(str, languageValue, commanderValue, level);
            m_Entries = commanderPipelineList.ToArray();
            StopCoroutine(nameof(DelayRebuildContents));
            StartCoroutine(nameof(DelayRebuildContents));
        }
        private IEnumerator DelayRebuildContents()
        {
            for (int i = m_ContentTrans.childCount - 1; i >= 0; i--)
            {
                Transform childTrans = m_ContentTrans.GetChild(i);
                UnityEngine.Object.Destroy(childTrans.gameObject);
                childTrans.gameObject.SetActive(false);
                childTrans.SetParent(null);
            }
            Vector2 sizeDelta = (m_ContentTrans as RectTransform).sizeDelta;
            sizeDelta.y = m_Entries.Length * (m_Template.transform as RectTransform).rect.height;
            (m_ContentTrans as RectTransform).sizeDelta = sizeDelta;
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < m_Entries.Length; i++)
            {
                GameObject commanderPipelineGo = Instantiate(m_Template, m_Template.transform.parent);
                commanderPipelineGo.GetComponent<CommanderPipelineEntryView>().SetEntry(m_Entries[i]);
                if (i % 20 == 19)
                    yield return null;
            }
        }

        private void OnClickClose()
        {
            DialogResult = DialogResult.Cancel;
            CameraCanvas.PopDialog(this);
        }
        private void OnSelectFile()
        {
            DialogResult = DialogResult.OK;
            CameraCanvas.PopDialog(this);
        }
    }
}