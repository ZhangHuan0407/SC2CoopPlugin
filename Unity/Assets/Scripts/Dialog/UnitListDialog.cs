using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Table;

namespace Game.UI
{
    public class UnitListDialog : MonoBehaviour, IDialog
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

        public void Hide()
        {
            m_Canvas.enabled = false;
        }
        public void Show()
        {
            m_Canvas.enabled = true;
        }

        public DialogResult DialogResult { get; set; }
        public int UnitID { get; set; }

        //[SerializeField]
        //private InputField m_UnitRegexInput;
        [SerializeField]
        private Dropdown m_CommanderDropdown;
        private List<string> m_CommanderOptions;

        [SerializeField]
        private Transform m_ContentTrans;
        [SerializeField]
        private Button m_UnitTemplate;
        [SerializeField]
        private Text m_UnitLabel;

        [SerializeField]
        private Button m_OKButton;
        [SerializeField]
        private Button m_CancelButton;

        private void Awake()
        {
            m_CommanderDropdown.ClearOptions();
            m_CommanderOptions = new List<string>();
            foreach (string key in StrToCommander.Keys)
            {
                string str = TableManager.LocalizationTable[key];
                m_CommanderDropdown.options.Add(new Dropdown.OptionData(str));
                m_CommanderOptions.Add(key);
            }
            m_CommanderDropdown.value = 0;
            m_CommanderDropdown.onValueChanged.AddListener((_) =>
            {
                m_OKButton.interactable = false;
                m_UnitLabel.text = string.Empty;
                StopCoroutine(nameof(RebuildList));
                StartCoroutine(nameof(RebuildList));
            });
            m_UnitTemplate.gameObject.SetActive(false);
            m_UnitLabel.text = string.Empty;

            m_OKButton.onClick.AddListener(OnClickOKButton);
            m_OKButton.interactable = false;
            m_CancelButton.onClick.AddListener(OnClickCancelButton);
        }

        private void OnClickOKButton()
        {
            DialogResult = DialogResult.OK;
            CameraCanvas.PopDialog(this);
        }
        private void OnClickCancelButton()
        {
            DialogResult = DialogResult.Cancel;
            CameraCanvas.PopDialog(this);
        }

        private IEnumerator RebuildList()
        {
            yield return new WaitForEndOfFrame();
            foreach (Transform childTrans in m_ContentTrans)
            {
                UnityEngine.Object.Destroy(childTrans.gameObject);
            }
            string commanderKey = m_CommanderOptions[m_CommanderDropdown.value];
            CommanderName? commanderName = StrToCommander[commanderKey];
            //string regexStr = m_UnitRegexInput.text;
            //if (string.IsNullOrWhiteSpace(regexStr))
            //    regexStr = ".*";
            List<int> allUnits = new List<int>();
            foreach (UnitTable.Entry entry in TableManager.UnitTable.Data.Values)
            {
                if (commanderName != null)
                {
                    if (entry.Commander != commanderName)
                        continue;
                }
                allUnits.Add(entry.ID);
            }
            for (int i = 0; i < allUnits.Count; i++)
            {
                int unitID = allUnits[i];
                Button unitButton = Instantiate(m_UnitTemplate, m_ContentTrans);
                unitButton.gameObject.SetActive(true);
                (unitButton.targetGraphic as Image).sprite = TableManager.UnitTable[unitID].LoadTexture();
                unitButton.onClick.AddListener(() =>
                {
                    UnitID = unitID;
                    UnitTable.Entry entry = TableManager.UnitTable[unitID];
                    m_UnitLabel.text = $"{entry.ID}, {entry.Name.Localization}";
                    m_OKButton.interactable = true;
                });
                if (i % 50 == 49)
                    yield return null;
            }
        }

        private void OnDestroy()
        {
            Resources.UnloadUnusedAssets();
        }
    }
}