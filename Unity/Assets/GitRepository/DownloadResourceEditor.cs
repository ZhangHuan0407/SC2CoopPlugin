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
    public class DownloadResourceEditor
    {
        [MenuItem("Tools/GitRepository/CheckUpdate")]
        private static void CheckUpdate()
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

        [MenuItem("Tools/GitRepository/DownloadUpdate master")]
        private static void DownloadUpdateMaster()
        {
            DownloadUpdate("master");
        }

        [MenuItem("Tools/GitRepository/DownloadUpdate develop")]
        private static void DownloadUpdateDevelop()
        {
            DownloadUpdate("develop");
        }

        private static void DownloadUpdate(string branch)
        {
            RepositoryConfig localRepositoryConfig = null;
            Task.Run(async () =>
            {
                localRepositoryConfig = new RepositoryConfig(GameDefined.RemoteResourceRepository, GameDefined.LocalResourceDirectory, branch);
                DownloadResourceTool tool = new DownloadResourceTool(localRepositoryConfig);
                return await tool.DownloadUpdateAsync();
            })
                .ContinueWith((task) =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                        Debug.Log(task.Result);
                    else
                        Debug.Log(task.Status);
                    localRepositoryConfig.Dispose();
                })
                .ContinueWith((task) =>
                {
                    new DirectoryInfo(GameDefined.LocalResourceDirectory)
                        .CopyFilesTo(new DirectoryInfo($"{Application.streamingAssetsPath}/{GameDefined.LocalResourceDirectory}"), true);
                    Debug.Log("finish");
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