using Game.Model;
using System;
using System.Collections.Generic;
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

        
        public const string CommanderContentDialogPath = "Dialogs/CommanderContentDialog";
        public const string CommanderEditorDialogPath = "Dialogs/CommanderEditorDialog";
        public const string DrawGizmosDialogPath = "Dialogs/DrawGizmosDialog";
        public const string MainManuDialog = "Dialogs/MainManuDialog";
        public const string OpenCommanderFileDialog = "Dialogs/OpenCommanderFileDialog";
        public const string SaveCommanderFileDialog = "Dialogs/SaveCommanderFileDialog";
        public const string SettingDialogPath = "Dialogs/SettingDialog";
        public const string TestDialog = "Dialogs/TestDialog";
        public const string UpdateResourceDialog = "Dialogs/UpdateResourceDialog";

        public const string MaxClentVersionKey = "MaxClentVersion";
        public const string ClientNewVersionWebPage = "";

        public const string HowToShareWebPage = "";
        public const string HowToUseWebPage = "";

        public static void CallFromLoadingThread()
        {
            Version = int.Parse(Application.version.Split('.')[0]);
            ScreenWidth = Screen.width;
            ScreenHeight = Screen.height;
            UserSettingFilePath = $"{Application.persistentDataPath}/UserSetting.json";
            Debug.Log(UserSettingFilePath);
        }
    }
}