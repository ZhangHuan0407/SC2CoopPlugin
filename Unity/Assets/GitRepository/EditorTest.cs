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
            //request.Method = "GET";
            //var response = request.GetResponse();
            //var stream = response.GetResponseStream();
            //StreamReader reader = new StreamReader(stream);
            //string content = reader.ReadToEnd();

            //Debug.Log(content);
            //return;

            //byte[] bytes = new byte[stream.Length];
            //int size = 0;
            //int point = 0;
            //do
            //{
            //    size = stream.Read(bytes, point, bytes.Length - point);
            //    point += size;
            //} while (size > 0);
            //File.WriteAllBytes("a.txt", bytes);
            //Debug.Log($"{point},{stream.Length}");
            //response.Dispose();
        }
    }
}
#endif