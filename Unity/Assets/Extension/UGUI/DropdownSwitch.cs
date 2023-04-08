using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityEngine.UI.Extension
{
    [RequireComponent(typeof(Dropdown))]
    public class DropdownSwitch : MonoBehaviour
    {
        [Serializable]
        public struct Config
        {
            public string RegexStr;
            [NonSerialized]
            public Regex Regex;
            public UnityEvent Action;
        }

        /* field */
        private Dropdown m_Dropdown;
        [Tooltip("OnStart 时自动触发一次值变更事件")]
        [SerializeField]
        private bool m_TriggerValueChangedOnStart = true;
        [SerializeField]
        private Config[] m_Configs;

        /* ctor */
        private void Awake()
        {
            m_Dropdown = GetComponent<Dropdown>();
            for (int i = 0; i < m_Configs.Length; i++)
                m_Configs[i].Regex = new Regex(m_Configs[i].RegexStr);
        }

        private void Start()
        {
            if (m_TriggerValueChangedOnStart)
                OnValueChanged();
        }

        /* func */
        public void OnValueChanged()
        {
            string option = m_Dropdown.options[m_Dropdown.value].text;
            foreach (Config config in m_Configs)
            {
                if (config.Regex.IsMatch(option))
                {
                    Debug.Log(config.RegexStr);
                    config.Action.Invoke();
                }
            }
        }
    }
}