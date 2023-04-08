using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UnityEngine.UI.Extension
{
    [AddComponentMenu("UI/ComplexButton", 31)]
    public class ComplexButton : Selectable, IPointerClickHandler, ISubmitHandler
    {
        [Serializable]
        public class ButtonClickedEvent : UnityEvent { }

        private const string LeftButtonFlag = nameof(ComplexButton) + "." + nameof(OnLeftClick);
        private const string RightButtonFlag = nameof(ComplexButton) + "." + nameof(OnRightClick);
        private const string MiddleButtonFlag = nameof(ComplexButton) + "." + nameof(OnMiddleClick);

        /* field */
        [SerializeField]
        private ButtonClickedEvent m_OnLeftClick = new ButtonClickedEvent();
        public ButtonClickedEvent OnLeftClick => m_OnLeftClick;

        [SerializeField]
        private bool m_RightClickStateTransition = false;
        [SerializeField]
        private ButtonClickedEvent m_OnRightClick = new ButtonClickedEvent();
        public ButtonClickedEvent OnRightClick => m_OnRightClick;

        [SerializeField]
        private bool m_MidClickStateTransition = false;
        [SerializeField]
        private ButtonClickedEvent m_OnMiddleClick = new ButtonClickedEvent();
        public ButtonClickedEvent OnMiddleClick => m_OnMiddleClick;

        private Coroutine m_SubmitCoroutine;

        /* ctor */
        protected ComplexButton() { }

        /* inter */
        public bool IsValid
        {
            get
            {
                if (!IsActive() || !IsInteractable())
                    return false;
                else
                    return true;
            }
        }

        /* func */
        private void OnPress(string flag, ButtonClickedEvent buttonClickedEvent, bool stateTransition)
        {
            if (IsValid)
            {
                if (m_SubmitCoroutine != null && stateTransition)
                    StopCoroutine(m_SubmitCoroutine);
                UISystemProfilerApi.AddMarker(flag, this);
                buttonClickedEvent.Invoke();
                if (stateTransition && IsValid)
                {
                    DoStateTransition(SelectionState.Pressed, false);
                    m_SubmitCoroutine = StartCoroutine(OnFinishSubmit());
                }
            }
        }

        private IEnumerator OnFinishSubmit()
        {
            float fadeTime = colors.fadeDuration;
            yield return new WaitForSecondsRealtime(fadeTime);
            DoStateTransition(currentSelectionState, false);
        }

        // ISubmitHandler
        public void OnSubmit(BaseEventData eventData)
        {
            OnPress(LeftButtonFlag, m_OnLeftClick, true);
        }

        // IPointerClickHandler
        public void OnPointerClick(PointerEventData eventData)
        {
            ButtonClickedEvent buttonClickedEvent;
            string flag;
            bool transitionState;
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    buttonClickedEvent = m_OnLeftClick;
                    flag = LeftButtonFlag;
                    transitionState = false; // unity ui have manage it
                    break;
                case PointerEventData.InputButton.Right:
                    buttonClickedEvent = m_OnRightClick;
                    flag = RightButtonFlag;
                    transitionState = m_RightClickStateTransition;
                    break;
                case PointerEventData.InputButton.Middle:
                    buttonClickedEvent = m_OnMiddleClick;
                    flag = MiddleButtonFlag;
                    transitionState = m_MidClickStateTransition;
                    break;
                default:
                    return;
            }
            OnPress(flag, buttonClickedEvent, transitionState);
        }

        public void RemoveAllListeners()
        {
            m_OnLeftClick.RemoveAllListeners();
            m_OnRightClick.RemoveAllListeners();
            m_OnMiddleClick.RemoveAllListeners();
        }
    }
}