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
        };

        public const string RemoteResourceRepository = "https://gitcode.net/qq_34919016/sc2coopplugin-resource";
        public const string LocalResourceDirectory = "LocalResourceRepository";
        public const string ResourceSubmoduleDirectory = "../sc2coopplugin-resource";
        public const string TempDirectory = "Temp";

        public static readonly string UserSettingFilePath = $"{Application.persistentDataPath}/UserSetting.json";

        public const string DrawGizmosDialogPath = "Dialogs/DrawGizmosDialog";
        public const string SettingDialogPath = "Dialogs/SettingDialog";
        public const string OpenCommanderFileDialog = "Dialogs/OpenCommanderFileDialog";
        public const string MainManuDialog = "Dialogs/MainManuDialog";
        public const string TestDialog = "Dialogs/TestDialog";
        public const string UpdateResourceDialog = "Dialogs/UpdateResourceDialog";

        public const string MaxClentVersionKey = "MaxClentVersion";
        public const string ClientNewVersionWebPage = "";

        /* ctor */
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
            Version = int.Parse(Application.version.Split('.')[0]);
            ScreenWidth = Screen.width;
            ScreenHeight = Screen.height;
            Debug.Log(UserSettingFilePath);
        }
    }
}