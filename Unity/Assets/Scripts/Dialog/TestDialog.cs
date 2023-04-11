using System;
using System.Threading.Tasks;
using Game.OCR;
using Game.UI;
using Tween;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class TestDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        [SerializeField]
        private Text m_Text;

        private RectAnchor rect;

        public string PrefabPath { get; set; }

        private float m_LastParseTime = 0f;

        private DrawGizmosDialog GizmosDialog;

        private void Awake()
        {
            Camera.main.GetComponent<TransparentWindow>().SetWindowState(WindowState.TopMostAndRaycastIgnore);
            GizmosDialog = CameraCanvas.PushDialog(GameDefined.DrawGizmosDialogPath) as DrawGizmosDialog;
            rect = new RectAnchor(264, 770, 80, 36);
        }

        public void Hide()
        {
            m_Canvas.enabled = false;
        }

        public void Show()
        {
            m_Canvas.enabled = true;
        }

        private void Update()
        {
            m_LastParseTime += Time.deltaTime;
            if (m_LastParseTime > 0.5f)
            {
                m_LastParseTime = 0f;
                int seconds = -1;
                object obj = new object();
                MapTimeParseResult result = MapTimeParseResult.Unknown;
                Tweener tweener = Global.BackThread.WaitingBackThreadTweener(() =>
                {
                    lock (obj)
                    {
                        result = Global.MapTime.TryParse(false, rect, out seconds);
                    }
                })
                    .OnComplete(() =>
                    {
                        lock (obj)
                        {
                            m_Text.text = $"{result}\n{seconds}";
                        }
                    })
                    .DoIt();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var aa = Camera.main.GetComponent<TransparentWindow>();
                if (aa.WindowState == WindowState.TopMostAndRaycastIgnore)
                    aa.SetWindowState(WindowState.TopMostAndBlockRaycast);
                else
                    aa.SetWindowState(WindowState.TopMostAndRaycastIgnore);
            }
        }
    }
}