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

        public Dictionary<string, Image> AllGizmos = new Dictionary<string, Image>();

        public void Hide()
        {
            m_Canvas.enabled = false;
        }

        public void Show()
        {
            m_Canvas.enabled = true;
        }

        public void DrawLine(string key, Vector2 startPoint, Vector2 endPoint)
        {
            if (!AllGizmos.TryGetValue(key, out Image image))
            {
                image = Instantiate(m_LineImage);
                image.gameObject.SetActive(true);
                AllGizmos.Add(key, image);
            }
            RectTransform imageRectTrans = (image.transform as RectTransform);
            Vector3 startLocalPoint = imageRectTrans.InverseTransformPoint(startPoint);
            Vector3 endLocalPoint = imageRectTrans.InverseTransformPoint(endPoint);
            imageRectTrans.localPosition += (startLocalPoint + endLocalPoint) / 2f;
        }
    }
}