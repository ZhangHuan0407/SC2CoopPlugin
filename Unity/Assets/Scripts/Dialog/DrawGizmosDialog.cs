using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RectAnchor = Game.OCR.RectAnchor;

namespace Game.UI
{
    public class DrawGizmosDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        public bool DestroyFlag { get; set; }
        public string PrefabPath { get; set; }

        [SerializeField]
        private Image m_LineImage;

        private List<Image> m_Group;

        private void Awake()
        {
            m_Group = new List<Image>();
            m_LineImage.gameObject.SetActive(false);

            Vector2 referenceResolution = CameraCanvas.ReferenceResolution;
            float referenceRatio = referenceResolution.x / referenceResolution.y;
            Vector2 currentResolution = new Vector2(Screen.width, Screen.height);
            float currentRatio = currentResolution.x / currentResolution.y;
            float localScale;
            if (currentRatio > referenceRatio)
                localScale = referenceResolution.y / currentResolution.y;
            else
                localScale = referenceResolution.x / currentResolution.x;
            transform.localScale = new Vector3(localScale, localScale, 1f);
        }

        public void Hide()
        {
            m_Canvas.enabled = false;
        }

        public void Show()
        {
            m_Canvas.enabled = true;
        }

        public void DrawRectAnchor(RectAnchor rectAnchor)
        {
            Vector2 canvasSize = CameraCanvas.CanvasSize;
            Rect rect = default;
            rect.xMin = Mathf.LerpUnclamped(-canvasSize.x / 2f, canvasSize.x / 2f, (float)rectAnchor.Left / GameDefined.ScreenWidth);
            rect.yMin = Mathf.LerpUnclamped(canvasSize.y / 2f, -canvasSize.y / 2f, (float)(rectAnchor.Top + rectAnchor.Height) / GameDefined.ScreenHeight);
            rect.width = Mathf.LerpUnclamped(0f, canvasSize.x, (float)rectAnchor.Width / GameDefined.ScreenWidth);
            rect.height = Mathf.LerpUnclamped(0f, canvasSize.y, (float)rectAnchor.Height / GameDefined.ScreenHeight);

            while (m_Group.Count < 4)
            {
                m_Group.Add(Instantiate(m_LineImage, m_LineImage.transform.parent));
            } 
            Image topLine = m_Group[0];
            topLine.gameObject.SetActive(true);
            RectTransform imageRectTrans = (topLine.transform as RectTransform);
            Vector2 center = rect.center;
            imageRectTrans.localPosition = new Vector3(center.x, rect.yMax, 0f);
            imageRectTrans.sizeDelta = new Vector2(rect.width, 2f);

            Image bottomLine = m_Group[1];
            bottomLine.gameObject.SetActive(true);
            imageRectTrans = (bottomLine.transform as RectTransform);
            imageRectTrans.localPosition = new Vector3(center.x, rect.yMin, 0f);
            imageRectTrans.sizeDelta = new Vector2(rect.width, 2f);

            Image leftLine = m_Group[2];
            leftLine.gameObject.SetActive(true);
            imageRectTrans = (leftLine.transform as RectTransform);
            imageRectTrans.localPosition = new Vector3(rect.xMin, center.y, 0f);
            imageRectTrans.sizeDelta = new Vector2(2f, rect.height);

            Image rightLine = m_Group[3];
            rightLine.gameObject.SetActive(true);
            imageRectTrans = (rightLine.transform as RectTransform);
            imageRectTrans.localPosition = new Vector3(rect.xMax, center.y, 0f);
            imageRectTrans.sizeDelta = new Vector2(2f, rect.height);
        }

        public void Clear()
        {
            for (int i = 0; i < m_Group.Count; i++)
                m_Group[i].gameObject.SetActive(false);
        }
    }
}