using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class ImageRandomColor : MonoBehaviour
    {
        [SerializeField]
        private Color m_ColorA;
        [SerializeField]
        private Color m_ColorB;

        [ContextMenu("RandomColor")]
        private void RandomColor()
        {
            Image[] imageList = GetComponentsInChildren<Image>();
            for (int i = 0; i < imageList.Length; i++)
            {
                imageList[i].color = (Random.Range(0f, 2f) > 1f) ? m_ColorA : m_ColorB;
            }
        }
    }
}