using Game.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class DrawGizmosDialog : MonoBehaviour, IDialog
    {
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas Canvas => m_Canvas;

        public string PrefabPath { get; set; }

        [SerializeField]
        private Image m_LineImage;

        private HashSet<Image> m_Group;

        private void Awake()
        {
            m_Group = new HashSet<Image>();
        }

        public void Hide()
        {
            m_Canvas.enabled = false;
        }

        public void Show()
        {
            m_Canvas.enabled = true;
        }

        public void DrawRect(Rect rect)
        {
            Image topLine = Instantiate(m_LineImage);
            RectTransform imageRectTrans = (topLine.transform as RectTransform);
            imageRectTrans.position = new Vector3(rect.x, rect.yMin, 0f);
            float length = imageRectTrans.InverseTransformVector(rect.width, 0f, 0f).x;
            imageRectTrans.sizeDelta = new Vector2(length, 1f);

            Image bottomLine = Instantiate(m_LineImage);
            imageRectTrans = (bottomLine.transform as RectTransform);
            imageRectTrans.position = new Vector3(rect.x, rect.yMax, 0f);
            length = imageRectTrans.InverseTransformVector(rect.width, 0f, 0f).x;
            imageRectTrans.sizeDelta = new Vector2(length, 1f);

            Image leftLine = Instantiate(m_LineImage);
            imageRectTrans = (leftLine.transform as RectTransform);
            imageRectTrans.position = new Vector3(rect.xMin, rect.y, 0f);
            length = imageRectTrans.InverseTransformVector(0f, rect.height, 0f).y;
            imageRectTrans.sizeDelta = new Vector2(1f, length);

            Image rightLine = Instantiate(m_LineImage);
            imageRectTrans = (rightLine.transform as RectTransform);
            imageRectTrans.position = new Vector3(rect.xMax, rect.y, 0f);
            length = imageRectTrans.InverseTransformVector(0f, rect.height, 0f).y;
            imageRectTrans.sizeDelta = new Vector2(1f, length);
        }

        public void Clear()
        {
            foreach (Image image in m_Group)
                Destroy(image.gameObject);
        }
    }
}