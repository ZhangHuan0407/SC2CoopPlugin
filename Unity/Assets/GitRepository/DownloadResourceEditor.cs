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
        private static async void CheckUpdate()
        {
            ResourceUpdateResult result;
            using (RepositoryConfig repositoryConfig = new RepositoryConfig(GameDefined.RemoteResourceRepository, GameDefined.LocalResourceDirectory))
            {
                int version = int.Parse(Application.version.Split('.')[0]);
                DownloadResourceTool tool = new DownloadResourceTool(repositoryConfig, version);
                result = await tool.CheckUpdateAsync();
            }
            Debug.Log(result);
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

        private static async void DownloadUpdate(string branch)
        {
            Debug.Log($"DownloadUpdate branch: {branch}");
            ResourceUpdateResult result;
            using (RepositoryConfig localRepositoryConfig = new RepositoryConfig(GameDefined.RemoteResourceRepository, GameDefined.LocalResourceDirectory, branch))
            {
                int version = int.Parse(Application.version.Split('.')[0]);
                DownloadResourceTool tool = new DownloadResourceTool(localRepositoryConfig, version);
                result = await tool.DownloadUpdateAsync();
            }
            Debug.Log(result);

            string streamingDirectory = $"{Application.streamingAssetsPath}/{GameDefined.LocalResourceDirectory}";
            if (Directory.Exists(streamingDirectory))
                Directory.Delete(streamingDirectory, true);
            new DirectoryInfo(GameDefined.LocalResourceDirectory).CopyFilesTo(new DirectoryInfo(streamingDirectory), true);
            AssetDatabase.Refresh();
            Debug.Log("finish");
        }

        [MenuItem("Tools/GitRepository/Copy Submodule(Disk)")]
        public static void CopySubmodule()
        {
            Debug.Log($"CopySubmodule");
            if (Directory.Exists(GameDefined.LocalResourceDirectory))
                Directory.Delete(GameDefined.LocalResourceDirectory, true);
            new DirectoryInfo(GameDefined.ResourceSubmoduleDirectory).CopyFilesTo(new DirectoryInfo(GameDefined.LocalResourceDirectory), true);
            string streamingDirectory = $"{Application.streamingAssetsPath}/{GameDefined.LocalResourceDirectory}";
            if (Directory.Exists(streamingDirectory))
                Directory.Delete(streamingDirectory, true);
            new DirectoryInfo(GameDefined.LocalResourceDirectory).CopyFilesTo(new DirectoryInfo(streamingDirectory), true);
            AssetDatabase.Refresh();
            Debug.Log("finish");
        }
    }
}
#endif