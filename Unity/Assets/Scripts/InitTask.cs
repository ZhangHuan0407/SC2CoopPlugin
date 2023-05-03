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
using Tween;

namespace Game
{
    public class InitTask : MonoBehaviour
    {
        /* ctor */
        private void Awake()
        {
            Camera.main.GetComponent<TransparentWindow>().SetWindowState(WindowState.TopMostAndBlockRaycast);
            DontDestroyOnLoad(Camera.main);
        }

        private IEnumerator Start()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            DontDestroyOnLoad(gameObject);

#if !UNITY_EDITOR && !ALPHA
            Debug.unityLogger.logEnabled = false;
#endif
            yield return null;

            JSONMap.CallFromLoadingThread();
            GameDefined.CallFromLoadingThread();

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
                    OCRConnectorA.Instance?.Dispose();
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
            int lastUpdateCheck = PlayerPrefs.GetInt(GameDefined.LastUpdateCheckKey, 0);
            int currentTime = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
            if (Mathf.Abs(currentTime - lastUpdateCheck) > 86400)
            {
                DownloadResourceTool tool = new DownloadResourceTool(Global.ResourceRepositoryConfig, GameDefined.Version);
                Task<ResourceUpdateResult> checkUpdateTask = tool.CheckUpdateAsync();
                Tweener tweener = Tween.LogicTween.WaitUntil(() => checkUpdateTask.IsCompleted)
                                                    .OnComplete(() =>
                                                    {
                                                        int maxClientVersion = PlayerPrefs.GetInt(GameDefined.MaxClentVersionKey);
                                                        PlayerPrefs.SetInt(GameDefined.MaxClentVersionKey, Mathf.Max(tool.MaxClentVersion, maxClientVersion));
                                                        PlayerPrefs.SetInt(GameDefined.LastUpdateCheckKey, currentTime);
                                                    });
                tweener.DoIt();
            }

#if !UNITY_EDITOR
            if (Directory.Exists(GameDefined.TempDirectory))
                Directory.Delete(GameDefined.TempDirectory, true);
#endif

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
            if (jsonMapInitTask.Exception != null)
                throw jsonMapInitTask.Exception;

            Global.UserSetting = UserSetting.LoadSetting();
            TableManager.LoadInnerTables();
            TableManager.LoadLocalizationTable(Global.UserSetting.InterfaceLanguage);
            yield return null;

            yield return new WaitUntil(() => newOCRProcessTask.IsCompleted);
            if (newOCRProcessTask.Exception != null)
                throw newOCRProcessTask.Exception;
            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
            yield return null;
            Game.UI.CameraCanvas.PushDialog(GameDefined.MainManuDialog);

            Debug.Log($"Finish init task {stopwatch.ElapsedMilliseconds} ms");
            Destroy(gameObject);
        }
    }
}