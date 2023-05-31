using System;
using System.Collections;
using System.Collections.Generic;
using Game.Model;
using Table;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class OpenCommanderFileDialog : MonoBehaviour, IDialog
    {
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

        [Header("Option")]
        [SerializeField]
        private InputField m_FilterStrInput;
        [SerializeField]
        private Dropdown m_Language;
        private List<SystemLanguage?> m_RawLanguageOptions;
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
        private Button m_CancelButton;

        [Header("Content")]
        [SerializeField]
        private ScrollRect m_ScrollRect;
        [SerializeField]
        private Transform m_ContentTrans;
        [SerializeField]
        private GameObject m_Template;

        [Header("Selected")]
        [SerializeField]
        private CanvasGroup m_SelectedFileGroup;
        [SerializeField]
        private Text m_FileTitle;
        [SerializeField]
        private Text m_FileDesc;
        [SerializeField]
        private Text m_FilePath;
        [SerializeField]
        private Button m_DemoButton;

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
            m_RawLanguageOptions = new List<SystemLanguage?>();
            {
                string str = TableManager.LocalizationTable["SystemLanguage.All"];
                m_Language.options.Add(new Dropdown.OptionData(str));
                m_RawLanguageOptions.Add(null);
            }
            foreach (SystemLanguage systemLanguage in GameDefined.SupportedLanguages)
            {
                string str = TableManager.LocalizationTable[systemLanguage];
                m_Language.options.Add(new Dropdown.OptionData(str));
                m_RawLanguageOptions.Add(systemLanguage);
            }
            m_Language.value = 0;
            string preferenceLanguage = PlayerPrefs.GetString(GameDefined.CommanderPreferenceLanguage, Global.UserSetting.InterfaceLanguage.ToString());
            SystemLanguage? language;
            if (Enum.TryParse<SystemLanguage>(preferenceLanguage, out SystemLanguage v))
                language = v;
            else
                language = null;
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

            m_OKButton.onClick.AddListener(OnClickOKButton);
            m_OKButton.interactable = false;
            m_CancelButton.onClick.AddListener(OnClickCancelButton);

            m_Template.gameObject.SetActive(false);

            m_SelectedFileGroup.alpha = 0f;
            m_SelectedFileGroup.interactable = false;
        }

        private void Start()
        {
            FilterCommanderPipelines();
        }

        private void FilterCommanderPipelines()
        {
            LogService.System(nameof(FilterCommanderPipelines), string.Empty);
            SystemLanguage? languageValue = m_RawLanguageOptions[m_Language.value];
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
                GameObject commanderPipelineGo = Instantiate(m_Template, m_ContentTrans);
                commanderPipelineGo.SetActive(true);
                CommanderPipelineEntryView commanderPipelineView = commanderPipelineGo.GetComponent<CommanderPipelineEntryView>();
                CommanderPipelineTable.Entry entry = m_Entries[i];
                commanderPipelineView.SetEntry(entry);
                commanderPipelineView.SelectButton.onClick.RemoveAllListeners();
                commanderPipelineView.SelectButton.onClick.AddListener(() =>
                {
                    CommanderPipelineId = entry.Id;
                    m_OKButton.interactable = true;
                    RebuildSelectedView();
                });
                if (i % 20 == 19)
                    yield return null;
            }
        }

        private void RebuildSelectedView()
        {
            CommanderPipeline commanderPipiline = TableManager.CommanderPipelineTable.Instantiate(CommanderPipelineId);
            m_SelectedFileGroup.alpha = 1f;
            m_SelectedFileGroup.interactable = true;
            m_FileTitle.text = commanderPipiline.Title;
            m_FileDesc.text = commanderPipiline.Desc;
            m_FilePath.text = TableManager.CommanderPipelineTable[CommanderPipelineId].FullName.Replace("\\", "/");
            m_DemoButton.onClick.RemoveAllListeners();
            m_DemoButton.interactable = !string.IsNullOrWhiteSpace(commanderPipiline.DemoURL);
            m_DemoButton.onClick.AddListener(() =>
            {
                Application.OpenURL(commanderPipiline.DemoURL);
            });
        }

        private void OnClickCancelButton()
        {
            LogService.System(nameof(OnClickCancelButton), string.Empty);
            DialogResult = DialogResult.Cancel;
            CameraCanvas.PopDialog(this);
        }
        private void OnClickOKButton()
        {
            LogService.System(nameof(OnClickOKButton), string.Empty);
            DialogResult = DialogResult.OK;
            CameraCanvas.PopDialog(this);
        }
    }
}