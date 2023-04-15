using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GitRepository;
using Table;

namespace Game.UI
{
    public class UpdateResourcesDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        public string PrefabPath { get; set; }

        [SerializeField]
        private Text m_RemoteRepositoryText;
        [SerializeField]
        private Text m_PercentageText;

        public void Hide()
        {
            m_Canvas.enabled = false;
        }

        public void Show()
        {
            m_Canvas.enabled = true;
        }

        private IEnumerator Start()
        {
            DownloadResourceTool tool = new DownloadResourceTool(Global.ResourceRepositoryConfig);
            var task = tool.DownloadUpdateAsync();
            while (!task.IsCompleted)
            {
                yield return null;
            }
            if (task.Result == ResourceUpdateResult.Success)
            {
                TableManager.LoadInnerTables();
                TableManager.LoadLocalizationTable(Global.UserSetting.InterfaceLanguage);
            }
            CameraCanvas.PopDialog(this);
        }
    }
}