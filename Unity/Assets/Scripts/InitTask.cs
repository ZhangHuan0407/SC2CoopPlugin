using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Table;
using UnityEngine;
using UnityEngine.SceneManagement;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Game
{
    public class InitTask : MonoBehaviour
    {
        /* ctor */
        private IEnumerator Start()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            DontDestroyOnLoad(gameObject);

            GameObject loadingGo = new GameObject("Loading", typeof(SpriteRenderer));
            loadingGo.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/Loading");
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
            yield return new WaitUntil(() => jsonMapInitTask.IsCompleted);
            
            Global.UserSetting = UserSetting.LoadSetting();
            TableManager.LoadInnerTables();
            TableManager.LoadDefaultDescribeTable();
            yield return null;

            if (Global.UserSetting.NewUser)
            {
                // load user setting dialog
                GameObject dialog = null;
                while (dialog)
                {
                    yield return null;
                }
                UserSetting.Save();
            }

            SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);

            Debug.Log($"Finish init task {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Stop();
            Destroy(loadingGo);
            Destroy(gameObject);
        }
    }
}