using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Game.OCR;
using Tween;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class TestDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        [SerializeField]
        private Text m_Text;
        [SerializeField]
        private InputField m_Rect;

        private RectAnchor rect;

        public string PrefabPath { get; set; }

        private float m_LastParseTime = 0f;

        private DrawGizmosDialog GizmosDialog;

        private void Awake()
        {
            Camera.main.GetComponent<TransparentWindow>().SetWindowState(WindowState.TopMostAndRaycastIgnore);
            GizmosDialog = CameraCanvas.PushDialog(GameDefined.DrawGizmosDialogPath) as DrawGizmosDialog;
            rect = new RectAnchor(264, 770, 80, 36);
            m_Rect.text = rect.ToString();
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
            string rectStr = m_Rect.text;
            Match match = Regex.Match(rectStr, "L:(?<Left>[0-9]+),T:(?<Top>[0-9]+),W:(?<Width>[0-9]+),H:(?<Height>[0-9]+)");
            if (match.Success)
            {
                rect.Left = int.Parse(match.Groups["Left"].Value);
                rect.Top = int.Parse(match.Groups["Top"].Value);
                rect.Width = int.Parse(match.Groups["Width"].Value);
                rect.Height = int.Parse(match.Groups["Height"].Value);
            }
        }
    }
}