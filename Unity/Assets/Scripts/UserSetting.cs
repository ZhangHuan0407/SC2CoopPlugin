using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class UserSetting
    {
        [SerializeField]
        private bool m_NewUser;
        public bool NewUser
        {
            get => m_NewUser;
            set => m_NewUser = value;
        }

        public static UserSetting LoadSetting()
        {
            return new UserSetting();
        }
        public static void Save()
        {

        }
    }
}