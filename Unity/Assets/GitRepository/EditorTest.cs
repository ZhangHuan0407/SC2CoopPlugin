#if UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Game;
using System.Net;
using System.IO;
using System.Text;

namespace GitRepository
{
    public class EditorTest
    {
        [MenuItem("Tools/GitRepository/CheckUpdate")]
        public static void CheckUpdate()
        {
            RepositoryConfig repositoryConfig = new RepositoryConfig(GameDefined.RemoteResourceRepository, GameDefined.LocalResourceDirectory);
            DownloadResourceTool tool = new DownloadResourceTool(repositoryConfig);
            tool.CheckUpdateAsync()
                .ContinueWith((task) =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                        Debug.Log(task.Result);
                    else
                        Debug.Log(task.Status);
                    repositoryConfig.Dispose();
                });
        }

        [MenuItem("Tools/GitRepository/DownloadUpdate")]
        public static void DownloadUpdate()
        {
            RepositoryConfig repositoryConfig = new RepositoryConfig(GameDefined.RemoteResourceRepository, GameDefined.LocalResourceDirectory);
            DownloadResourceTool tool = new DownloadResourceTool(repositoryConfig);
            tool.DownloadUpdateAsync()
                .ContinueWith((task) =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                        Debug.Log(task.Result);
                    else
                        Debug.Log(task.Status);
                    repositoryConfig.Dispose();
                });
        }

        [MenuItem("Tools/GitRepository/StreamingAssetsPath")]
        public static void StreamingAssetsPath()
        {
            Debug.Log(Application.streamingAssetsPath);
        }
    }
}
#endif