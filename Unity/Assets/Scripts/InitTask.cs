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

            Task newOCRProcessTask = OCRProcess.StartNewOCRProcessAsync();

            Global.ResourceRepositoryConfig = new RepositoryConfig(GameDefined.RemoteResourceRepository, GameDefined.LocalResourceDirectory);
            if (!Directory.Exists(GameDefined.LocalResourceDirectory))
            {
                Global.ResourceRepositoryConfig.IOLock.EnterWriteLock();
                new DirectoryInfo($"{Application.streamingAssetsPath}/{GameDefined.LocalResourceDirectory}")
                    .CopyFilesTo(new DirectoryInfo(GameDefined.LocalResourceDirectory), true);
                Global.ResourceRepositoryConfig.IOLock.ExitWriteLock();
            }

            Global.MapTime.NNModel = new LittleNN.NeuralNetworkModel();
            using (Stream stream = new FileStream(MapTimeParameter.NNModelFileName, FileMode.Open, FileAccess.Read))
            {
                Global.MapTime.NNModel.Read(stream);
            }

            yield return new WaitUntil(() => jsonMapInitTask.IsCompleted);

            Global.UserSetting = UserSetting.LoadSetting();
            TableManager.LoadInnerTables();
            TableManager.LoadLocalizationTable(Global.UserSetting.InterfaceLanguage);
            yield return null;

            if (Global.UserSetting.NewUser)
            {
                var dialog = Game.UI.CameraCanvas.PushDialog("Dialogs/SettingDialog");
                while (dialog.gameObject)
                {
                    yield return null;
                }
                UserSetting.Save();
            }

            yield return new WaitUntil(() => newOCRProcessTask.IsCompleted);
            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
            yield return null;
            Game.UI.CameraCanvas.PushDialog("Dialogs/MainManuDialog");

            Debug.Log($"Finish init task {stopwatch.ElapsedMilliseconds} ms");
            Destroy(gameObject);
        }
    }
}