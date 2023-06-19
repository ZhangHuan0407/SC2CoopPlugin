using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using RectAnchor = Game.OCR.RectAnchor;

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

        [SerializeField]
        private Vector2Int m_CanvasResolution;
        public Vector2Int CanvasResolution
        {
            get => m_CanvasResolution;
            set => m_CanvasResolution = value;
        }

        public UserSetting()
        {
            m_Version = 0;
            m_NewUser = true;
            m_InGameLanguage = string.Empty;
            m_IsProgrammer = false;
            m_RectPositions = new Dictionary<RectAnchorKey, RectAnchor>();
        }
        internal void AppendDefaultField()
        {
            if (m_Version == 0)
                m_Version = GameDefined.Version;
            int screenWidth = GameDefined.ScreenWidth;
            int screenHeight = GameDefined.ScreenHeight;
            if (string.IsNullOrEmpty(m_InGameLanguage))
                m_InGameLanguage = "zh-Hans-CN";
            if (!m_RectPositions.ContainsKey(RectAnchorKey.CommanderName))
            {
                RectAnchor rectAnchor = new RectAnchor();
                rectAnchor.Left = Mathf.FloorToInt(screenWidth * 0.1558f - 202f);
                rectAnchor.Top = Mathf.FloorToInt(screenHeight * 0.4667f - 33f);
                rectAnchor.Width = 250;
                rectAnchor.Height = 53;
                m_RectPositions[RectAnchorKey.CommanderName] = rectAnchor;
            }
            if (!m_RectPositions.ContainsKey(RectAnchorKey.CoopMenu))
            {
                RectAnchor rectAnchor = new RectAnchor();
                rectAnchor.Left = Mathf.FloorToInt(screenWidth * 0.0634f + 42.2f);
                rectAnchor.Top = Mathf.FloorToInt(12f);
                rectAnchor.Width = Mathf.FloorToInt(screenWidth * 0.0635f);
                rectAnchor.Height = Mathf.FloorToInt(screenHeight * 0.0333f + 24f);
                m_RectPositions[RectAnchorKey.CoopMenu] = rectAnchor;
            }
            if (!m_RectPositions.ContainsKey(RectAnchorKey.Masteries))
            {
                RectAnchor rectAnchor = new RectAnchor();
                rectAnchor.Left = 15;
                rectAnchor.Top = Mathf.FloorToInt(screenHeight * 0.3f);
                rectAnchor.Width = Mathf.FloorToInt(screenWidth * 0.3f);
                rectAnchor.Height = Mathf.FloorToInt(screenHeight * 0.4f);
                m_RectPositions[RectAnchorKey.Masteries] = rectAnchor;
            }
            //if (!m_RectPositions.ContainsKey(RectAnchorKey.LoadingMapName))
            //{
            //    RectAnchor rectAnchor = new RectAnchor();
            //    //rectAnchor.Left = 15;
            //    //rectAnchor.Top = Mathf.FloorToInt(screenHeight * 0.3f);
            //    //rectAnchor.Width = Mathf.FloorToInt(screenWidth * 0.3f);
            //    //rectAnchor.Height = Mathf.FloorToInt(screenHeight * 0.4f);
            //    m_RectPositions[RectAnchorKey.LoadingMapName] = rectAnchor;
            //}
            if (!m_RectPositions.ContainsKey(RectAnchorKey.MapTime))
            {
                RectAnchor rectAnchor = new RectAnchor();
                rectAnchor.Left = Mathf.FloorToInt(screenWidth * 0.0057f + 252f);
                rectAnchor.Top = Mathf.FloorToInt(screenHeight * 0.4333f + 299f);
                rectAnchor.Width = 80;
                rectAnchor.Height = 32;
                m_RectPositions[RectAnchorKey.MapTime] = rectAnchor;
            }
            if (!m_RectPositions.ContainsKey(RectAnchorKey.MapTask))
            {
                RectAnchor rectAnchor = new RectAnchor();
                rectAnchor.Left = 0;
                rectAnchor.Top = 0;
                rectAnchor.Width = Mathf.FloorToInt(screenWidth * 0.0423f + 172.8f);
                rectAnchor.Height = Mathf.FloorToInt(screenHeight * 0.05f + 250f);
                m_RectPositions[RectAnchorKey.MapTask] = rectAnchor;
            }
            if (!m_RectPositions.ContainsKey(RectAnchorKey.PluginDialog))
            {
                RectAnchor rectAnchor = new RectAnchor();
                rectAnchor.Left = 10;
                rectAnchor.Top = Mathf.FloorToInt(screenHeight * 0.5f - 250f);
                rectAnchor.Width = 250;
                rectAnchor.Height = 500;
                m_RectPositions[RectAnchorKey.PluginDialog] = rectAnchor;
            }
            if (m_CanvasResolution == Vector2Int.zero)
            {
                int pixelCount = Screen.width * Screen.height;
                int delta = int.MaxValue;
                foreach (Vector2Int StdResolution in GameDefined.StdCanvasResolution)
                {
                    int delta2 = Math.Abs(pixelCount - StdResolution.x * StdResolution.y);
                    if (delta2 < delta)
                    {
                        delta = delta2;
                        m_CanvasResolution = StdResolution;
                    }
                }
            }
        }

        public static UserSetting LoadSetting()
        {
            UserSetting userSetting;
            try
            {
                if (File.Exists(GameDefined.UserSettingFilePath))
                {
                    string content = File.ReadAllText(GameDefined.UserSettingFilePath);
                    userSetting = JSONMap.ParseJSON<UserSetting>(JSONObject.Create(content));
                    userSetting.AppendDefaultField();
                    userSetting.VersionFix();
                    return userSetting;
                }
            }
            catch (Exception ex)
            {
                LogService.Error(nameof(LoadSetting), ex.ToString());
            }

            userSetting = new UserSetting();
            userSetting.AppendDefaultField();
            if (GameDefined.SupportedLanguages.Contains(Application.systemLanguage))
                userSetting.InterfaceLanguage = Application.systemLanguage;
            else
                userSetting.InterfaceLanguage = SystemLanguage.ChineseSimplified;
            return userSetting;
        }
        public static void Save(bool systemAutoSave)
        {
            UserSetting userSetting = Global.UserSetting;
            LogService.System(nameof(UserSetting) + nameof(Save), $"NewUser: {userSetting.NewUser}");
            if (!systemAutoSave)
            {
                userSetting.NewUser = false;
            }
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