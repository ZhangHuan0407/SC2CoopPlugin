using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Game;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;

namespace GitRepository
{
    public class DownloadResourceTool
    {
        public readonly RepositoryConfig Config;
        public readonly int CurrentVersion;
        public volatile int MaxClentVersion;

        public DownloadResourceTool(RepositoryConfig config, int currentVersion)
        {
            Config = config;
            CurrentVersion = currentVersion;
        }

        /// <returns>
        /// <see cref="ResourceUpdateResult.Success"/>,
        /// <see cref="ResourceUpdateResult.RegexError"/>
        /// </returns>
        private (ResourceUpdateResult state, string commit) GetLatestCommitVersion()
        {
            WebRequest webRequest = HttpWebRequest.Create(Config.CommitPageUri);
            webRequest.Method = "GET";
#if ALPHA
            UnityEngine.Debug.Log("WebRequest: " + webRequest.RequestUri);
#endif
            string webPageContent;
            using (WebResponse response = webRequest.GetResponse())
            {
                string characterSet = (response as HttpWebResponse).CharacterSet;
                Encoding encoding = Encoding.GetEncoding(characterSet);
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                {
                    webPageContent = reader.ReadToEnd().ToString();
                }
            }
            List<string> allCommits = new List<string>();
            foreach (Match match in Config.CommitRegex.Matches(webPageContent))
            {
                if (match.Success)
                    allCommits.Add(match.Groups["Commit"].Value);
            }
#if UNITY_EDITOR || ALPHA
            if (allCommits.Count > 0)
            {
                string log = string.Join(",  ", allCommits);
                UnityEngine.Debug.Log(allCommits[0] + "\n" + log);
            }
#endif
            if (allCommits.Count > 0)
                return (ResourceUpdateResult.Success, allCommits[0]);
            else
                return (ResourceUpdateResult.RegexError, string.Empty);
        }
        /// <returns>
        /// <see cref="ResourceUpdateResult.Success"/>,
        /// <see cref="ResourceUpdateResult.RegexError"/>
        /// </returns>
        private (ResourceUpdateResult state, string availableCommit) GetLatestAvailableCommitVersion()
        {
            WebRequest webRequest = HttpWebRequest.Create(Config.RemoteVersionMap);
            webRequest.Method = "GET";
#if ALPHA
            UnityEngine.Debug.Log("WebRequest: " + webRequest.RequestUri);
#endif
            string webPageContent;
            using (WebResponse response = webRequest.GetResponse())
            {
                string characterSet = (response as HttpWebResponse).CharacterSet;
                Encoding encoding = Encoding.GetEncoding(characterSet);
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                {
                    webPageContent = reader.ReadToEnd().ToString();
                }
            }
#if UNITY_EDITOR || ALPHA
            UnityEngine.Debug.Log("RemoteVersionMap:\n" + webPageContent);
#endif

            string[] lines = webPageContent.Split('\n');
            if (lines.Length < 1)
                return (ResourceUpdateResult.RegexError, string.Empty);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                line = line.Trim();
                string[] contents = line.Split(',');
                int.TryParse(contents[0], out int version);
                if (version > MaxClentVersion)
                    MaxClentVersion = version;
                if (CurrentVersion != version)
                    continue;
                if (contents.Length < 2)
                    return (ResourceUpdateResult.RegexError, string.Empty);
                return (ResourceUpdateResult.Success, contents[1].Trim());
            }
            return (ResourceUpdateResult.RegexError, string.Empty);
        }
        /// <returns>
        /// <paramref name="workTask"/>
        /// +<see cref="ResourceUpdateResult.NetworkError"/>
        /// </returns>
        private Task<(ResourceUpdateResult, T)> Try<T>(Func<(ResourceUpdateResult, T)> workTask, int times)
        {
            var task = Task<ResourceUpdateResult>.Run(async () =>
            {
                (ResourceUpdateResult state, T content) result = default;
                for (int i = 0; i < times; i++)
                {
                    try
                    {
                        result = workTask();
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError(nameof(DownloadResourceTool) + nameof(Try) + ex.ToString());
                        result.state = ResourceUpdateResult.NetworkError;
                        result.content = default;
                    }
                    if (result.state == ResourceUpdateResult.RegexError)
                        return result;
                    if (result.state == ResourceUpdateResult.Success)
                        break;
                    await Task.Delay(300);
                }
                return result;
            });
            return task;
        }

        public Task<ResourceUpdateResult> CheckUpdateAsync()
        {
            var task = Task<ResourceUpdateResult>.Run(async () =>
            {
                (ResourceUpdateResult state, string commit) latestAvailableCommit = await Try(GetLatestAvailableCommitVersion, 3);
                if (latestAvailableCommit.state != ResourceUpdateResult.Success)
                    return latestAvailableCommit.state;

                (ResourceUpdateResult state, string commit) latestCommit = default;
                if (latestAvailableCommit.commit == "LatestCommit")
                    latestCommit = await Try(GetLatestCommitVersion, 3);
                else
                    latestCommit = (ResourceUpdateResult.Success, latestAvailableCommit.commit);
                if (latestCommit.state != ResourceUpdateResult.Success)
                    return latestCommit.state;
                string localCommit = string.Empty;
                Config.IOLock.EnterReadLock();
                if (File.Exists(Config.LocalCommitVersion))
                    localCommit = File.ReadAllText(Config.LocalCommitVersion);
                Config.IOLock.ExitReadLock();
                if (latestAvailableCommit.commit != "LatestCommit")
                    return ResourceUpdateResult.NeedUpdateClent;
                else if(latestCommit.commit == localCommit)
                    return ResourceUpdateResult.Success;
                else
                    return ResourceUpdateResult.NeedUpdateResource;
            });
            return task;
        }

        public Task<ResourceUpdateResult> DownloadUpdateAsync()
        {
            var task = Task<ResourceUpdateResult>.Run(async () =>
            {
                (ResourceUpdateResult state, string commit) latestAvailableCommit = await Try(GetLatestAvailableCommitVersion, 3);
                if (latestAvailableCommit.state != ResourceUpdateResult.Success)
                    return latestAvailableCommit.state;

                (ResourceUpdateResult state, string commit) latestCommit = default;
                if (latestAvailableCommit.commit == "LatestCommit")
                    latestCommit = await Try(GetLatestCommitVersion, 3);
                else
                    latestCommit = (ResourceUpdateResult.Success, latestAvailableCommit.commit);

                (ResourceUpdateResult state, bool _) downloadResult = await Try(() => (DownloadUpdate_Internal(latestCommit.commit), true), 3);
                return downloadResult.state;
            });
            return task;
        }
        private ResourceUpdateResult DownloadUpdate_Internal(string commit)
        {
            // download zip
            WebRequest downloadRequest = HttpWebRequest.Create(Config.HttpCommitVersionCloneUri(commit));
            downloadRequest.Method = "GET";
#if ALPHA
            UnityEngine.Debug.Log("WebRequest: " + downloadRequest.RequestUri);
#endif
            Directory.CreateDirectory(GameDefined.TempDirectory);
            string zipFileName = $"{GameDefined.TempDirectory}/download_{commit}.zip";
            using (WebResponse downloadResponse = downloadRequest.GetResponse())
            {
                if (downloadResponse.ContentType.ToLowerInvariant().Contains("text/html"))
                {
                    using (StreamReader streamReader = new StreamReader(downloadResponse.GetResponseStream()))
                    {
                        string content = streamReader.ReadToEnd();
                        Debug.LogError(content);
                        return ResourceUpdateResult.UnhandledException;
                    }
                }
                Stream responseStream = downloadResponse.GetResponseStream();
                byte[] buffer = new byte[2040];
                using (FileStream tempFileStream = new FileStream(zipFileName, FileMode.Create, FileAccess.Write))
                {
                    int size = 0;
                    do
                    {
                        size = responseStream.Read(buffer, 0, 2040);
                        tempFileStream.Write(buffer, 0, size);
                    } while (size > 0);
                }
            }

            // delete old files and unzip
            Config.IOLock.EnterWriteLock();
            try
            {
                if (Directory.Exists(Config.LocalDirectory))
                    Directory.Delete(Config.LocalDirectory, true);
                Directory.CreateDirectory(Config.LocalDirectory);
                UnZip(zipFileName, Config.LocalDirectory);
                File.WriteAllText(Config.LocalCommitVersion, commit);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
                return ResourceUpdateResult.UnhandledException;
            }
            finally
            {
                Config.IOLock.ExitWriteLock();
            }
            Config.UpdateTimes++;
            return ResourceUpdateResult.Success;
        }
        private void UnZip(string zipFileName, string desDirectory)
        {
            byte[] buffer = new byte[2040];
            using (FileStream fileStreamIn = new FileStream(zipFileName, FileMode.Open, FileAccess.Read))
            {
                using (ZipInputStream zipInStream = new ZipInputStream(fileStreamIn))
                {
                    while (zipInStream.GetNextEntry() is ZipEntry entry)
                    {
                        WriteFile(zipInStream, entry);
                    }
                }
            }
            void WriteFile(ZipInputStream zipInStream, ZipEntry entry)
            {
                if (!entry.IsFile)
                    return;
                string fileSubPath = entry.Name.Substring(entry.Name.IndexOf("/") + 1);
                FileInfo fileInfo = new FileInfo($"{desDirectory}/{fileSubPath}");
                Directory.CreateDirectory(fileInfo.DirectoryName);
                using (FileStream fileStreamOut = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write))
                {
                    int size;
                    do
                    {
                        size = zipInStream.Read(buffer, 0, buffer.Length);
                        fileStreamOut.Write(buffer, 0, size);
                    } while (size > 0);
                    fileStreamOut.Close();
                };
            }
        }
    }
}