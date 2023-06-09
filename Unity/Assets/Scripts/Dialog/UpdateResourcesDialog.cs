﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GitRepository;
using Table;
using System.Diagnostics;

namespace Game.UI
{
    public class UpdateResourcesDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        public bool DestroyFlag { get; set; }
        public string PrefabPath { get; set; }

        [SerializeField]
        private Text m_RemoteRepositoryText;
        private float m_Percentage;
        [SerializeField]
        private Text m_PercentageText;
        [SerializeField]
        private Button m_ForceQuitButton;
        [SerializeField]
        private Text m_DescribeUpdateText;

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
            m_ForceQuitButton.onClick.AddListener(OnClickForceQuitButton);
            m_RemoteRepositoryText.text = Global.ResourceRepositoryConfig.RepositoryUri;
            m_PercentageText.text = "0%";
            bool needUpdateClient = PlayerPrefs.GetInt(GameDefined.MaxClentVersionKey) > GameDefined.Version;
            string strKey = needUpdateClient ? "UI.UpdateResource.NeedDownloadClient" : "UI.UpdateResource.Desc2";
            m_DescribeUpdateText.text = TableManager.LocalizationTable[strKey];
        }

        private IEnumerator Start()
        {
            DownloadResourceTool tool = new DownloadResourceTool(Global.ResourceRepositoryConfig, GameDefined.Version);
            var task = tool.DownloadUpdateAsync();
            while (!task.IsCompleted)
            {
                m_Percentage += Mathf.Max(Mathf.Sin(Time.time) / 2f, 0.1f) * Time.deltaTime;
                m_PercentageText.text = $"{m_Percentage * 5f:0.0}%";
                yield return null;
            }
            if (task.Result == ResourceUpdateResult.Success)
            {
                TableManager.LoadInnerTables();
                TableManager.LoadLocalizationTable(Global.UserSetting.InterfaceLanguage);
            }
            m_PercentageText.text = TableManager.LocalizationTable["UI.UpdateResource.Finish"];
            int maxClientVersion = PlayerPrefs.GetInt(GameDefined.MaxClentVersionKey);
            if (maxClientVersion < tool.MaxClentVersion)
                maxClientVersion = tool.MaxClentVersion;
            PlayerPrefs.SetInt(GameDefined.MaxClentVersionKey, maxClientVersion);
            yield return new WaitForSeconds(1.5f);
            if (maxClientVersion > GameDefined.Version)
                Application.OpenURL(GameDefined.ClientNewVersionWebPage);
            CameraCanvas.PopDialog(this);
        }

        private void OnClickForceQuitButton()
        {
            Application.Quit(0);
        }
    }
}