using GitRepository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Table;
using UnityEngine;
using UnityEngine.SceneManagement;
using Stopwatch = System.Diagnostics.Stopwatch;
using Game.OCR;
using UnityEditor;

namespace Game
{
    public class InitTask : MonoBehaviour
    {
        /* ctor */
        private IEnumerator Start()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            DontDestroyOnLoad(gameObject);

#if !UNITY_EDITOR && !ALPHA
            Debug.unityLogger.logEnabled = false;
#endif
            yield return null;

            JSONMap.CallFromLoadingThread();

            Application.logMessageReceived += (string condition, string stackTrace, LogType logType) => 
            {
                switch (logType)
                {
                    case LogType.Error:
                    case LogType.Exception:
                    case LogType.Assert:
                        LogService.Error($"Unity Error Log, logType: {logType}",
                                         $"condition: {condition}\nstackTrace: {stackTrace}",
                                         true);
                        break;
                    default:
                        break;
                }
            };

            Task jsonMapInitTask = Task.Run(() =>
            {
                JSONMap.RegisterDefaultType();
                JSONMap.RegisterAllTypes();
                foreach (JSONSerialized serialized in GameDefined.JSONSerializedRegisterTypes)
                    JSONMap.RegisterType(serialized);
            });

            Global.BackThread = BackThread.CreateNew("BackThread");

            Task newOCRProcessTask = OCRProcess.StartNewOCRProcessAsync();
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += (i) =>
            {
                if (i == PlayModeStateChange.ExitingPlayMode)
                {
                    OCRConnector.Instance?.Dispose();
                }
            };
#endif

            Global.ResourceRepositoryConfig = new RepositoryConfig(GameDefined.RemoteResourceRepository, GameDefined.LocalResourceDirectory);
            if (!Directory.Exists(GameDefined.LocalResourceDirectory))
            {
                Global.ResourceRepositoryConfig.IOLock.EnterWriteLock();
                new DirectoryInfo($"{Application.streamingAssetsPath}/{GameDefined.LocalResourceDirectory}")
                    .CopyFilesTo(new DirectoryInfo(GameDefined.LocalResourceDirectory), true);
                Global.ResourceRepositoryConfig.IOLock.ExitWriteLock();
            }
            DownloadResourceTool tool = new DownloadResourceTool(Global.ResourceRepositoryConfig, GameDefined.Version);

            Task<ResourceUpdateResult> checkUpdateTask = tool.CheckUpdateAsync();
            Tween.WaitTween.WaitUntil(() => checkUpdateTask.IsCompleted)
                            .OnComplete(() =>
                            {
                                int maxClientVersion = PlayerPrefs.GetInt(GameDefined.MaxClentVersionKey);
                                PlayerPrefs.SetInt(GameDefined.MaxClentVersionKey, Mathf.Max(tool.MaxClentVersion, maxClientVersion));
                            });

            if (Directory.Exists(GameDefined.TempDirectory))
                Directory.Delete(GameDefined.TempDirectory, true);

            Global.MapTime = new MapTime(LittleNN.NeuralNetwork.LoadFrom(MapTimeParameter.NNModelFileName));
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += (i) =>
            {
                if (i == PlayModeStateChange.ExitingPlayMode)
                {
                    Global.MapTime.Dispose();
                }
            };
#endif

            yield return new WaitUntil(() => jsonMapInitTask.IsCompleted);

            Global.UserSetting = UserSetting.LoadSetting();
            TableManager.LoadInnerTables();
            TableManager.LoadLocalizationTable(Global.UserSetting.InterfaceLanguage);
            yield return null;

            if (Global.UserSetting.NewUser && false)
            {
                var dialog = Game.UI.CameraCanvas.PushDialog(GameDefined.SettingDialogPath);
                while (dialog.gameObject)
                {
                    yield return null;
                }
                UserSetting.Save();
            }

            yield return new WaitUntil(() => newOCRProcessTask.IsCompleted);
            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
            yield return null;
            Game.UI.CameraCanvas.PushDialog(GameDefined.MainManuDialog);

            Debug.Log($"Finish init task {stopwatch.ElapsedMilliseconds} ms");
            Destroy(gameObject);
        }
    }
}