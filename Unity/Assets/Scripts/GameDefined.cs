using Game.Model;
using System;
using System.Collections.Generic;
using System.IO;
using Table;
using UnityEngine;

namespace Game
{
    public static class GameDefined
    {
        public static int Version { get; private set; }
        public static int ScreenWidth { get; private set; }
        public static int ScreenHeight { get; private set; }

        public const int DialogSortingOrderPadding = 100;

        /// <summary>
        /// JSON 序列化接口注册
        /// </summary>
        public static readonly JSONSerialized[] JSONSerializedRegisterTypes = new JSONSerialized[]
        {
            EventModelFactory.EventModelSerialized,
        };

        public const string RemoteResourceRepository = "https://gitcode.net/qq_34919016/sc2coopplugin-resource";
        public const string LocalResourceDirectory = "LocalResourceRepository";
        public const string ResourceSubmoduleDirectory = "../sc2coopplugin-resource";
        public const string TempDirectory = "Temp";

        public static string UserSettingFilePath { get; private set; }
        public static string CustomCommanderPipelineDirectoryPath { get; private set; }

        public const string CommanderContentDialogPath = "Dialogs/CommanderContentDialog";
        public const string CommanderEditorDialogPath = "Dialogs/CommanderEditorDialog";
        public const string CoopTimelineDialogPath = "Dialogs/CoopTimelineDialog";
        public const string DrawGizmosDialogPath = "Dialogs/DrawGizmosDialog";
        public const string MainManuDialog = "Dialogs/MainManuDialog";
        public const string OpenCommanderFileDialog = "Dialogs/OpenCommanderFileDialog";
        public const string SaveCommanderFileDialog = "Dialogs/SaveCommanderFileDialog";
        public const string SettingDialogPath = "Dialogs/SettingDialog";
        public const string TestDialog = "Dialogs/TestDialog";
        public const string UpdateResourceDialog = "Dialogs/UpdateResourceDialog";
        public const string UnitListDialog = "Dialogs/UnitListDialog";
        public const string TechnologyListDialog = "Dialogs/TechnologyListDialog";

        public const string MaxClentVersionKey = "MaxClentVersion";
        public const string LastUpdateCheckKey = "LastUpdateCheck";
        public const string CommanderPreferenceLanguage = "CommanderPreferenceLanguage";
        public const string ClientNewVersionWebPage = "https://gitcode.net/qq_34919016/sc2coopplugin-resource/-/releases/";

        public const string ShareCommanderPipelineChineseWiki = "https://gitcode.net/qq_34919016/sc2coopplugin-resource/-/wikis/Chinese/ShareCommanderPipeline";
        public const string CommanderEditorChineseWiki = "https://gitcode.net/qq_34919016/sc2coopplugin-resource/-/wikis/Chinese/CommanderEditor";

        public static IReadOnlyList<SystemLanguage> SupportedLanguages = new SystemLanguage[]
        {
            SystemLanguage.ChineseSimplified,
            SystemLanguage.ChineseTraditional,
            SystemLanguage.English,
            SystemLanguage.French,
            SystemLanguage.German,
            SystemLanguage.Korean,
        };

        public static void CallFromLoadingThread()
        {
            Version = int.Parse(Application.version.Split('.')[0]);
            ScreenWidth = Screen.width;
            ScreenHeight = Screen.height;
            UserSettingFilePath = $"{Application.persistentDataPath}/UserSetting.json";
            CustomCommanderPipelineDirectoryPath = $"{Application.persistentDataPath}/CCP";
            Directory.CreateDirectory(CustomCommanderPipelineDirectoryPath);
            Debug.Log(UserSettingFilePath);
        }
    }
}