using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extension
{
    [RequireComponent(typeof(Slider))]
    public class SliderEndEdit : MonoBehaviour, IPointerUpHandler
    {
        [Serializable]
        public class EndEditEvent : UnityEvent<float> { }

        [SerializeField]
        private EndEditEvent m_EndEdit = new EndEditEvent();
        public EndEditEvent onEndEdit => m_EndEdit;

        public void OnPointerUp(PointerEventData data)
        {
            m_EndEdit.Invoke(GetComponent<Slider>().value);
        }
    }
}