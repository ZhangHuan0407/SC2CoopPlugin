using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class UserSetting
    {
        [SerializeField]
        private int m_Version;

        [SerializeField]
        private bool m_NewUser;
        public bool NewUser
        {
            get => m_NewUser;
            set => m_NewUser = value;
        }

        [SerializeField]
        private string m_InGameLanguage;
        public string InGameLanguage
        {
            get => m_InGameLanguage;
            set => m_InGameLanguage = value;
        }

        [SerializeField]
        private SystemLanguage m_InterfaceLanguage;
        public SystemLanguage InterfaceLanguage
        {
            get => m_InterfaceLanguage;
            set => m_InterfaceLanguage = value;
        }

        [SerializeField]
        private bool m_IsProgrammer;
        public bool IsProgrammer
        {
            get => m_IsProgrammer;
            set => m_IsProgrammer = value;
        }

        [SerializeField]
        private Dictionary<RectAnchorKey, Game.OCR.RectAnchor> m_RectPositions;
        public Dictionary<RectAnchorKey, Game.OCR.RectAnchor> RectPositions => m_RectPositions;

        public static UserSetting LoadSetting()
        {
            UserSetting userSetting;
            try
            {
                if (File.Exists(GameDefined.UserSettingFilePath))
                {
                    string content = File.ReadAllText(GameDefined.UserSettingFilePath);
                    userSetting = JSONMap.ParseJSON<UserSetting>(JSONObject.Create(content));
                    userSetting.VersionFix();
                    return userSetting;
                }
            }
            catch (Exception ex)
            {
                LogService.Error(nameof(LoadSetting), ex.ToString());
            }

            userSetting = new UserSetting()
            {
                m_Version = GameDefined.Version,
                m_NewUser = true,
                m_InGameLanguage = string.Empty,
                m_IsProgrammer = false,
                m_RectPositions = new Dictionary<RectAnchorKey, OCR.RectAnchor>(),
            };
            switch (Application.systemLanguage)
            {
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    userSetting.InterfaceLanguage = SystemLanguage.ChineseSimplified;
                    break;
                default:
                    userSetting.InterfaceLanguage = SystemLanguage.English;
                    break;
            }
            return userSetting;
        }
        public static void Save()
        {
            UserSetting userSetting = Global.UserSetting;
            LogService.System(nameof(UserSetting) + nameof(Save), $"NewUser: {userSetting.NewUser}");
            userSetting.NewUser = false;
             string content = JSONMap.ToJSON(userSetting).ToString();
            File.WriteAllText(GameDefined.UserSettingFilePath, content);
        }
        public void VersionFix()
        {
            if (m_Version >= GameDefined.Version)
                return;



            m_Version = GameDefined.Version;
        }
    }
}