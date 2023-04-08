using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Extension
{
    [RequireComponent(typeof(InputField))]
    public class InputTryParse : MonoBehaviour
    {
        public enum Type
        {
            Int32,
            String,
        }

        [SerializeField]
        private Type m_Type;
        public void TryParse()
        {
            InputField inputFieldText = GetComponent<InputField>();
            switch (m_Type)
            {
                case Type.Int32:
                    if (!int.TryParse(inputFieldText.text, out int _))
                        inputFieldText.text = "0";
                    break;
                case Type.String:
                    inputFieldText.text = inputFieldText.text ?? string.Empty;
                    break;
                default:
                    break;
            }
        }
    }
}