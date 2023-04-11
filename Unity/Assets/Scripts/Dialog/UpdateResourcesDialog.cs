using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GitRepository;

namespace Game.UI
{
    public class UpdateResourcesDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        public string PrefabPath { get; set; }

        [SerializeField]
        private Text m_DescribeText;
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
            CameraCanvas.PopDialog(this);
        }
    }
}