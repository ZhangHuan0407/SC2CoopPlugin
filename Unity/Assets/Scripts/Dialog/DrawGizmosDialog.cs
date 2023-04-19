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
            Rect cameraRect = Camera.main.rect;
            Rect rect = default;
            rect.xMin = Mathf.LerpUnclamped(cameraRect.xMin, cameraRect.xMax, rectAnchor.Left / GameDefined.ScreenWidth);
            rect.yMin = Mathf.LerpUnclamped(cameraRect.yMax, cameraRect.yMin, rectAnchor.Top / GameDefined.ScreenWidth);
            rect.width = Mathf.LerpUnclamped(0f, cameraRect.width, rectAnchor.Width / GameDefined.ScreenWidth);
            rect.height = Mathf.LerpUnclamped(0f, cameraRect.height, rectAnchor.Height / GameDefined.ScreenWidth);
            DrawRect(rect);
        }
        public void DrawRect(Rect rect)
        {
            while (m_Group.Count < 4)
            {
                m_Group.Add(Instantiate(m_LineImage));
            } 
            Image topLine = m_Group[0];
            topLine.gameObject.SetActive(true);
            RectTransform imageRectTrans = (topLine.transform as RectTransform);
            imageRectTrans.position = new Vector3(rect.x, rect.yMin, 0f);
            float length = imageRectTrans.InverseTransformVector(rect.width, 0f, 0f).x;
            imageRectTrans.sizeDelta = new Vector2(length, 1f);

            Image bottomLine = m_Group[1];
            bottomLine.gameObject.SetActive(true);
            imageRectTrans = (bottomLine.transform as RectTransform);
            imageRectTrans.position = new Vector3(rect.x, rect.yMax, 0f);
            length = imageRectTrans.InverseTransformVector(rect.width, 0f, 0f).x;
            imageRectTrans.sizeDelta = new Vector2(length, 1f);

            Image leftLine = m_Group[2];
            leftLine.gameObject.SetActive(true);
            imageRectTrans = (leftLine.transform as RectTransform);
            imageRectTrans.position = new Vector3(rect.xMin, rect.y, 0f);
            length = imageRectTrans.InverseTransformVector(0f, rect.height, 0f).y;
            imageRectTrans.sizeDelta = new Vector2(1f, length);

            Image rightLine = m_Group[3];
            rightLine.gameObject.SetActive(true);
            imageRectTrans = (rightLine.transform as RectTransform);
            imageRectTrans.position = new Vector3(rect.xMax, rect.y, 0f);
            length = imageRectTrans.InverseTransformVector(0f, rect.height, 0f).y;
            imageRectTrans.sizeDelta = new Vector2(1f, length);
        }

        public void Clear()
        {
            for (int i = 0; i < m_Group.Count; i++)
                m_Group[i].gameObject.SetActive(false);
        }
    }
}